using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Chinook.DataLoader
{
	[TemplateVisualState(GroupName = DataVisualGroup, Name = InitialVisualStateName)]
	[TemplateVisualState(GroupName = DataVisualGroup, Name = DataVisualStateName)]
	[TemplateVisualState(GroupName = DataVisualGroup, Name = EmptyVisualStateName)]
	[TemplateVisualState(GroupName = LoadingVisualGroup, Name = LoadingVisualStateName)]
	[TemplateVisualState(GroupName = LoadingVisualGroup, Name = NotLoadingVisualStateName)]
	[TemplateVisualState(GroupName = ErrorVisualGroup, Name = ErrorVisualStateName)]
	[TemplateVisualState(GroupName = ErrorVisualGroup, Name = NoErrorVisualStateName)]
	[ContentProperty(Name = "ContentTemplate")]
	public partial class DataLoaderView : Control
	{
		// Data states
		private const string DataVisualGroup = "DataStates";
		private const string InitialVisualStateName = "Initial";
		private const string DataVisualStateName = "Data";
		private const string EmptyVisualStateName = "Empty";

		// Loading states
		private const string LoadingVisualGroup = "LoadingStates";
		private const string LoadingVisualStateName = "Loading";
		private const string NotLoadingVisualStateName = "NotLoading";

		// Error states
		private const string ErrorVisualGroup = "ErrorStates";
		private const string ErrorVisualStateName = "Error";
		private const string NoErrorVisualStateName = "NoError";

		// Combined states
		private const string CombinedVisualGroup = "CombinedStates";

		private readonly ILogger _logger;

		private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private IDataLoader _dataLoader;
		private IDataLoaderState _nextState;
		private IDataLoaderState _lastState;
		private DataLoaderViewState _lastViewState;
		private bool _isLoaded;
		private bool _isSubscribedToSource;
		private bool _isVisualStateRefreshRequired;
		private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue; // This is a timestamp of when the last UI update was done.
		private TimeSpan _stateMinDuration = TimeSpan.Zero; // This is the non-"UI thread dependent" version of StateMinimumDuration.
		private TimeSpan _stateChangingThrottleDelay = TimeSpan.Zero; // This is the non-"UI thread dependent" version of StateChangingThrottleDelay.
		private int _updateId; // This is a counter for the Update method. It's used to deal with concurrency.

		public DataLoaderView()
		{
			this.DefaultStyleKey = typeof(DataLoaderView);

			_logger = this.Log();

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Update(_dataLoader?.State);
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (!_isSubscribedToSource && _dataLoader != null)
			{
				_dataLoader.StateChanged += OnDataLoaderStateChanged;
				_isSubscribedToSource = true;
			}

			_isLoaded = true;
			Update(_dataLoader?.State);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (_dataLoader != null)
			{
				_dataLoader.StateChanged -= OnDataLoaderStateChanged;
				_isSubscribedToSource = false;
			}

			_isLoaded = false;
		}

		private void OnSourceChanged(IDataLoader dataLoader)
		{
			if (_dataLoader != null)
			{
				_dataLoader.StateChanged -= OnDataLoaderStateChanged;
				_isSubscribedToSource = false;
			}

			_dataLoader = dataLoader;

			Update(_dataLoader?.State);

			if (dataLoader != null)
			{
				_dataLoader.StateChanged += OnDataLoaderStateChanged;
				_isSubscribedToSource = true;

				if (AutoLoad)
				{
					var manualDataLoaderTrigger = new ManualDataLoaderTrigger($"{nameof(DataLoaderView)}.{nameof(AutoLoad)}");

					_dataLoader.AddTrigger(manualDataLoaderTrigger);

					manualDataLoaderTrigger.Trigger();
				}
			}
		}

		private void OnDataLoaderStateChanged(IDataLoader dataLoader, IDataLoaderState newState)
		{
			Update(newState);
		}

		/// <summary>
		/// This method is reponsible for the wait mechanism (for <see cref="StateChangingThrottleDelay"/> and <see cref="StateMinimumDuration"/>).
		/// It will be called concurrently because of that reason.
		/// </summary>
		/// <param name="newState">The new state to update to.</param>
		/// <param name="source">The method caller name (used for tracing only).</param>
		private async void Update(IDataLoaderState newState, [CallerMemberName] string source = null)
		{
			try
			{
				var updateId = Interlocked.Increment(ref _updateId);

				if (_logger.IsEnabled(LogLevel.Trace))
				{
					_logger.LogTrace($"Got update request '#{updateId}' to update to '{newState}' from '{source}'.");
				}

				// Update the _nextState before the LockAsync.
				// This allows a previous execution to update to this newer state.
				_nextState = newState;

				// We use a lock async so that we can only have 1 execution changing the view properies (this.State and VisualStates) at a time.
				// This is very necessary because of the waiting mechanism. There will be fewer DataLoaderView visual updates than DataLoader updates.
				using (await LockAsync())
				{
					// Early-out condition 1.
					if (updateId < _updateId)
					{
						// When the local requestId is less than the _requestId of the control, it means that this request is obsolete.
						if (_logger.IsEnabled(LogLevel.Trace))
						{
							_logger.LogTrace($"Skipping update '#{updateId}' because it's no longer relevant.");
						}
						return;
					}

					// Early-out condition 2.
					if (Object.Equals(_nextState, _lastState))
					{
						// When the states are equal, we usually skip the update, unless a visual state refresh is required. (That happens when the DataLoader changes state before the DataLoaderView is Loaded.)
						if (_isVisualStateRefreshRequired && _isLoaded)
						{
							await RunOnDispatcher(() =>
							{
								RefreshVisualStates();
							});

							if (_logger.IsEnabled(LogLevel.Trace))
							{
								_logger.LogTrace($"Refreshed visual states now that the control is loaded.");
							}
						}
						else
						{
							if (_logger.IsEnabled(LogLevel.Trace))
							{
								_logger.LogTrace($"Skipping update '#{updateId}:{newState}' because we're already in that state.");
							}
						}

						// Leave the update because the processing is already done.
						return;
					}

					var elapsedSinceLastUpdate = DateTimeOffset.Now - _lastUpdate;
					var throttleDelay = _stateChangingThrottleDelay;
					var minStateDelay = _stateMinDuration - elapsedSinceLastUpdate;

					// We check whether waiting is necessary (either from the Throttle or MinState properties).
					var isWaitRequired = throttleDelay > TimeSpan.Zero || minStateDelay > TimeSpan.Zero;

					if (isWaitRequired)
					{
						TimeSpan waitDuration;
						if (minStateDelay > TimeSpan.Zero && minStateDelay > throttleDelay)
						{
							if (_logger.IsEnabled(LogLevel.Trace))
							{
								_logger.LogTrace($"Starting '{nameof(minStateDelay)}' for update from '#{updateId}:{newState}' because the last update was {elapsedSinceLastUpdate.TotalMilliseconds}ms ago.");
							}

							waitDuration = minStateDelay;
						}
						else
						{
							if (_logger.IsEnabled(LogLevel.Trace))
							{
								_logger.LogTrace($"Starting '{nameof(throttleDelay)}' for update from '#{updateId}:{newState}'.");
							}

							waitDuration = throttleDelay;
						}

						await Task.Delay(waitDuration);
					}

					if (_logger.IsEnabled(LogLevel.Trace))
					{
						_logger.LogTrace($"Updating VisualStates for '#{_updateId}(from update #{updateId}):{_nextState}'.");
					}

					await RunOnDispatcher(() =>
					{
						UpdateUI(_nextState);
					});
				}
			}
			catch (Exception e)
			{
				if (_logger.IsEnabled(LogLevel.Error))
				{
					_logger.LogError(e, "Caught unhandled exception in Update.");
				}
			}
		}

		private void UpdateUI(IDataLoaderState newState)
		{
			var safeState = newState ?? DataLoaderState.Default;

			SetState(CreateDataLoaderViewState(safeState));

			var dataVisualState = GetDataVisualState(safeState);
			GoToState(DataVisualGroup, dataVisualState);

			var errorVisualState = GetErrorVisualState(safeState);
			GoToState(ErrorVisualGroup, errorVisualState);

			var loadingVisualState = GetLoadingStateVisualState(safeState);
			GoToState(LoadingVisualGroup, loadingVisualState);

			var combinedVisualState = $"{dataVisualState}_{errorVisualState}_{loadingVisualState}";
			GoToState(CombinedVisualGroup, combinedVisualState);

			_lastUpdate = DateTime.Now;
			_lastState = safeState;
			_lastViewState = State;

			if (_isLoaded)
			{
				_isVisualStateRefreshRequired = false;

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation($"Updated VisualStates to '{combinedVisualState}'.");
				}
			}
			else
			{
				// If the control isn't loaded, the visual states are not applied.
				_isVisualStateRefreshRequired = true;

				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogTrace($"Saved VisualStates to '{combinedVisualState}'. (This control isn't loaded yet.)");
				}
			}
		}

		/// <summary>
		/// Sets the <see cref="State"/>.
		/// </summary>
		/// <remarks>
		/// This method is virtual to allow creation of more strongly typed version of DataLoaderView to allow usage of x:Bind inside the ContentTemplate.
		/// </remarks>
		/// <param name="state">The <see cref="DataLoaderViewState"/> to set as the current state.</param>
		protected virtual void SetState(DataLoaderViewState state)
		{
			State = state;
		}

		/// <summary>
		/// Applies the already saved visual states.
		/// </summary>
		private void RefreshVisualStates()
		{
			RefreshVisualState(DataVisualGroup);
			RefreshVisualState(ErrorVisualGroup);
			RefreshVisualState(LoadingVisualGroup);
			var combinedVisualState = RefreshVisualState(CombinedVisualGroup);
			_isVisualStateRefreshRequired = false;

			if (_logger.IsEnabled(LogLevel.Information))
			{
				_logger.LogInformation($"Updated VisualStates to '{combinedVisualState}'.");
			}
		}

		private static string GetDataVisualState(IDataLoaderState state)
		{
			if (state.IsInitial)
			{
				return InitialVisualStateName;
			}
			else if (state.IsEmpty)
			{
				return EmptyVisualStateName;
			}
			else
			{
				return DataVisualStateName;
			}
		}

		private static string GetLoadingStateVisualState(IDataLoaderState state)
		{
			return state.IsLoading
				? LoadingVisualStateName
				: NotLoadingVisualStateName;
		}

		private static string GetErrorVisualState(IDataLoaderState state)
		{
			return state.Error != null
				? ErrorVisualStateName
				: NoErrorVisualStateName;
		}

		private DataLoaderViewState CreateDataLoaderViewState(IDataLoaderState state)
		{
			return new DataLoaderViewState()
			{
				View = this,
				Parent = DataContext,
				Source = _dataLoader,
				Request = state.Request,
				Data = state.Data,
				Error = state.Error,
				IsLoading = state.IsLoading,
				IsInitial = state.IsInitial,
				IsEmpty = state.IsEmpty
			};
		}

		private async Task RunOnDispatcher(Action action)
		{
			if (Dispatcher.HasThreadAccess)
			{
				action();
			}
			else
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
			}
		}

		// This is an optimization to avoid calling VisualStateManager.GoToState if we're already in the right state.
		// It's also used to be able to save the visual states for each group when the control isn't loaded yet and restore them later.
		private Dictionary<string, string> _currentGroupStates = new Dictionary<string, string>();
		private void GoToState(string group, string visualState, bool useTransitions = true)
		{
			if (_currentGroupStates.TryGetValue(group, out var currentState))
			{
				if (currentState != visualState)
				{
					_currentGroupStates[group] = visualState;
					if (_isLoaded)
					{
						VisualStateManager.GoToState(this, visualState, useTransitions);
					}
				}
			}
			else
			{
				_currentGroupStates[group] = visualState;
				if (_isLoaded)
				{
					VisualStateManager.GoToState(this, visualState, useTransitions);
				}
			}
		}

		private string RefreshVisualState(string group, bool useTransitions = true)
		{
			if (_currentGroupStates.TryGetValue(group, out var currentState))
			{
				VisualStateManager.GoToState(this, currentState, useTransitions);
				return currentState;
			}
			else
			{
				return null;
			}
		}

		private static TimeSpan Max(TimeSpan t1, TimeSpan t2)
		{
			if (t1 > t2)
			{
				return t1;
			}
			else
			{
				return t2;
			}
		}

		public async Task<IDisposable> LockAsync()
		{
			await _semaphore.WaitAsync();

			return new ActionDisposable(() => _semaphore.Release());
		}
	}
}
