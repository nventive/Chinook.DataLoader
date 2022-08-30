using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This is a default implementation of <see cref="IDataLoader"/>.
	/// </summary>
	public class DataLoader : IDataLoader
	{
		private readonly object _ctsMutex = new object(); // The mutex to use when assigning _cts.
		private readonly IDataLoaderStrategy _strategy;
		private readonly List<IDataLoaderTrigger> _triggers;
		private readonly ILogger _logger;
		private readonly DataLoaderConcurrentMode _concurrentMode;
		private readonly Func<IDataLoaderState, bool> _emptySelector;
		private readonly Dictionary<string, object> _contextValues; // The context values set by the user from the Load method (via IDataLoaderContext).
		private DataLoaderState _state;
		private ManualDataLoaderTrigger _manualTrigger; // The manual trigger used when invoking this.Load().
		private CancellationTokenSource _cts; // The CancellationTokenSource for the current load execution. Assigning this field mush be done inside a lock using _ctsMutex.
		private int _sequenceId = -1; // This field is used to generate the SequenceId of each IDataLoaderRequest.

		public DataLoader(string name, IDataLoaderStrategy strategy, DataLoaderConcurrentMode concurrentMode, Func<IDataLoaderState, bool> emptySelector)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			_concurrentMode = concurrentMode;
			_emptySelector = emptySelector ?? throw new ArgumentNullException(nameof(emptySelector));
			_strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
			_state = DataLoaderState.Default;
			_triggers = new List<IDataLoaderTrigger>();
			_contextValues = new Dictionary<string, object>();
			_logger = this.Log();
		}

		/// <inheritdoc cref="IDataLoader.Name"/>
		public string Name { get; }

		/// <inheritdoc cref="IDataLoader.State"/>
		public IDataLoaderState State => _state;

		/// <inheritdoc cref="IDataLoader.StateChanged"/>
		public event StateChangedEventHandler StateChanged;

		/// <inheritdoc cref="IDataLoader.Triggers"/>
		public IEnumerable<IDataLoaderTrigger> Triggers => _triggers;

		/// <inheritdoc cref="IDataLoader.AddTrigger(IDataLoaderTrigger)"/>
		public void AddTrigger(IDataLoaderTrigger trigger)
		{
			trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));

			_triggers.Add(trigger);
			trigger.LoadRequested += OnLoadRequested;
		}

		/// <inheritdoc cref="IDataLoader.RemoveTrigger(IDataLoaderTrigger)"/>
		public void RemoveTrigger(IDataLoaderTrigger trigger)
		{
			trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));

			_triggers.Remove(trigger);
			trigger.LoadRequested -= OnLoadRequested;
		}

		/// <inheritdoc cref="IDataLoader.Load(CancellationToken, IDataLoaderContext)"/>
		public Task<object> Load(CancellationToken ct, IDataLoaderContext context = null)
		{
			context = context ?? new DataLoaderContext();
			_manualTrigger = _manualTrigger ?? new ManualDataLoaderTrigger($"{Name}.ManualTrigger");

			var sequenceId = Interlocked.Increment(ref _sequenceId);
			PopulateContext(context);

			return InnerLoad(ct, new DataLoaderRequest(sequenceId, _manualTrigger, context, _contextValues));
		}

		private void OnLoadRequested(IDataLoaderTrigger trigger, IDataLoaderContext context)
		{
			// This operation is not cancellable from the trigger itself.
			// The operation will be cancelled if the loader is disposed.
			context = context ?? new DataLoaderContext();
			var cancellationToken = CancellationToken.None;
			var sequenceId = Interlocked.Increment(ref _sequenceId);
			PopulateContext(context);

			_ = LoadFromTrigger(cancellationToken, new DataLoaderRequest(sequenceId, trigger, context, _contextValues));

			async Task LoadFromTrigger(CancellationToken ct, DataLoaderRequest request)
			{
				try
				{
					await InnerLoad(ct, request);
				}
				catch (Exception e)
				{
					if (_logger.IsEnabled(LogLevel.Error))
					{
						_logger.LogError(e, "Caught unhandled exception during Load operation. Consider injecting a strategy that handles all exceptions in your application code.");
					}
				}
			}
		}

		private async Task<object> InnerLoad(CancellationToken ct, DataLoaderRequest request)
		{
			var sequenceId = request.SequenceId;

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"Starting load request #{sequenceId} from trigger '{request.SourceTrigger.Name}'.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation($"Cancelled load request #{sequenceId} from trigger '{request.SourceTrigger.Name}' because of cancellation token.");
				}

				return null;
			}

			var ctsClosure = default(CancellationTokenSource);
			lock (_ctsMutex)
			{
				if (_cts == null)
				{
					ctsClosure = _cts = new CancellationTokenSource();
				}
				else
				{
					if (TryDealWithConcurrentLoad())
					{
						return null;
					}
				}
			}

			var result = default(object);
			try
			{
				if (_logger.IsEnabled(LogLevel.Trace))
				{
					_logger.LogTrace($"Starting load strategy for request #{sequenceId}.");
				}

				UpdateState(s => s
					.WithIsLoading(true)
					.WithRequest(request)
				);

				result = await _strategy.Load(ctsClosure.Token, request);

				if (ctsClosure == _cts)
				{
					if (_logger.IsEnabled(LogLevel.Trace))
					{
						_logger.LogTrace($"Finished load strategy for request #{sequenceId}. Pushing data.");
					}

					// We only update the state if the current cts is the same as the one that was used for this load.
					// When they're different, it means a new request is loading.
					UpdateState(s => s
						.WithData(result)
						.WithIsLoading(false)
						.WithError(null)
					);

					if (_logger.IsEnabled(LogLevel.Trace))
					{
						_logger.LogTrace($"Pushed data for request #{sequenceId}.");
					}

					if (_logger.IsEnabled(LogLevel.Information))
					{
						_logger.LogInformation($"Finished load for request #{sequenceId} from trigger '{request.SourceTrigger.Name}'.");
					}
				}
				else
				{
					if (_logger.IsEnabled(LogLevel.Debug))
					{
						_logger.LogDebug($"Finished load strategy for request #{sequenceId}, but a new request is processing so the data wasn't pushed.");
					}
				}
			}
			catch (Exception e)
			{
				var isCancellation = e is OperationCanceledException;

				// We only update the state if the current cts is the same as the one that was used for this load.
				// When they're different, it means a new request is loading.
				if (ctsClosure == _cts)
				{
					if (_logger.IsEnabled(LogLevel.Trace))
					{
						_logger.LogTrace($"{(isCancellation ? "Cancelled" : "Failed")} load strategy for request #{sequenceId}. Pushing result.");
					}

					UpdateState(s => s
						.WithIsLoading(false)
						.WithError(e)
					);

					if (_logger.IsEnabled(LogLevel.Trace))
					{
						_logger.LogTrace($"Pushed load {(isCancellation ? "cancellation error" : "error")} for request #{sequenceId}.");
					}

					if (_logger.IsEnabled(LogLevel.Information))
					{
						_logger.LogInformation($"{(isCancellation ? "Cancelled" : "Failed")} load for request #{sequenceId} from trigger '{request.SourceTrigger.Name}'.");
					}
				}
				else
				{
					if (_logger.IsEnabled(LogLevel.Debug))
					{
						_logger.LogDebug($"{(isCancellation ? "Cancelled" : "Failed")} load strategy for request #{sequenceId}, but a new request is processing so the error wasn't pushed.");
					}
				}
			}

			lock (_ctsMutex)
			{
				if (_cts == ctsClosure)
				{
					_cts = null;
				}
			}

			ctsClosure?.Dispose();
			ctsClosure = null;

			return result;

			// Returns true when the current load request should be aborted.
			bool TryDealWithConcurrentLoad()
			{
				if (_logger.IsEnabled(LogLevel.Trace))
				{
					_logger.LogTrace($"Detected potential concurrent load for request #{sequenceId}.");
				}

				switch (_concurrentMode)
				{
					case DataLoaderConcurrentMode.CancelPrevious:
						if (_logger.IsEnabled(LogLevel.Trace))
						{
							_logger.LogTrace($"Cancelling previous request from trigger '{State.Request.SourceTrigger.Name}' because a new request (#{sequenceId}) was created by trigger '{request.SourceTrigger.Name}'.");
						}

						// Cancel previous request.
						_cts.Cancel();
						_cts?.Dispose();

						// Create a new cts for the new request.
						ctsClosure = _cts = new CancellationTokenSource();

						if (_logger.IsEnabled(LogLevel.Information))
						{
							_logger.LogInformation($"Cancelled previous request from trigger '{State.Request.SourceTrigger.Name}' because a new request (#{sequenceId}) was created by trigger '{request.SourceTrigger.Name}'.");
						}
						break;

					case DataLoaderConcurrentMode.DiscardNew:
						if (_logger.IsEnabled(LogLevel.Information))
						{
							_logger.LogInformation($"Discarded new request (#{sequenceId}) from trigger '{request.SourceTrigger.Name}' because another request is currently loading.");
						}
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Adds data to the provided <see cref="IDataLoaderContext"/>.
		/// </summary>
		/// <param name="context">The context to populate.</param>
		private void PopulateContext(IDataLoaderContext context)
		{
			context[DataLoaderContextExtensions.DataLoaderNameKey] = Name;
		}

		private void UpdateState(Func<DataLoaderState, DataLoaderState> update)
		{
			var previousState = _state;
			_state = update(_state);
			_state = _state.WithIsEmpty(_emptySelector(_state));

			if (!Equals(previousState, _state))
			{
				StateChanged?.Invoke(this, _state);
			}
			else
			{
				if (_logger.IsEnabled(LogLevel.Trace))
				{
					_logger.LogTrace($"Discarded state changed event because the new state is equal to the previous.");
				}
			}
		}

		/// <summary>
		/// Disposes of every strategy and trigger this <see cref="DataLoader"/> had.
		/// </summary>
		public void Dispose()
		{
			foreach (var trigger in _triggers)
			{
				trigger.Dispose();
			}

			foreach (var item in _contextValues.Values)
			{
				if (item is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			_manualTrigger?.Dispose();
			_triggers.Clear();
			_strategy.Dispose();

			StateChanged = null;

			_cts?.Cancel();
			_cts?.Dispose();
		}
	}
}
