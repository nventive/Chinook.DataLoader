using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if WINUI
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Chinook.DataLoader
{
	/// <summary>
	/// This controller encapsulates all the reusable logic needed by <see cref="DataLoaderView"/> or other view implementations of <see cref="IDataLoader"/>.<br/>
	/// It limits the view updates of the associated <see cref="IDataLoader"/> using <see cref="StateChangingThrottleDelay"/> and <see cref="StateMinimumDuration"/>.
	/// </summary>
	public abstract class DataLoaderViewControllerBase<TDispatcher>
	{
		// Data states
		public const string DataVisualGroup = "DataStates";
		public const string InitialVisualStateName = "Initial";
		public const string DataVisualStateName = "Data";
		public const string EmptyVisualStateName = "Empty";

		// Loading states
		public const string LoadingVisualGroup = "LoadingStates";
		public const string LoadingVisualStateName = "Loading";
		public const string NotLoadingVisualStateName = "NotLoading";

		// Error states
		public const string ErrorVisualGroup = "ErrorStates";
		public const string ErrorVisualStateName = "Error";
		public const string NoErrorVisualStateName = "NoError";

		// Combined states
		public const string CombinedVisualGroup = "CombinedStates";

		private readonly WeakReference<IDataLoaderViewDelegate> _delegate;
		private readonly ILogger _logger;

		private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private IDataLoader _dataLoader;
		private IDataLoaderState _nextState;
		private IDataLoaderState _lastState;
		private DataLoaderViewState _lastViewState;
		private bool _isLoaded;
		private bool _isControlTemplateApplied;
		private bool _isSubscribedToSource;
		private bool _isVisualStateRefreshRequired;
		private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue; // This is a timestamp of when the last UI update was done.
		private int _updateId; // This is a counter for the Update method. It's used to deal with concurrency.

		/// <summary>
		/// ­­Initializes a new instance of the <see cref="DataLoaderViewController"/> class.
		/// </summary>
		/// <param name="dataLoaderViewDelegate">The delegate actioning the controller's logic.</param>
		/// <param name="dispatcher">The dispatcher required for UI updates.</param>
		public DataLoaderViewControllerBase(IDataLoaderViewDelegate dataLoaderViewDelegate, TDispatcher dispatcher)
		{
			_delegate = new WeakReference<IDataLoaderViewDelegate>(dataLoaderViewDelegate);
			Dispatcher = dispatcher;

			_logger = this.Log();
		}

		private IDataLoaderViewDelegate Delegate
		{
			get
			{
				if (_delegate.TryGetTarget(out var del))
				{
					return del;
				}

				throw new MemberAccessException("The reference to the IDataLoaderViewDelegate was lost. DataLoaderViewController only keeps a weak reference of the IDataLoaderViewDelegate instance to avoid the high risks of circular dependencies.");
			}
		}

		/// <inheritdoc cref="DataLoaderView.StateMinimumDuration"/>
		public TimeSpan StateMinimumDuration { get; set; } = TimeSpan.Zero;

		/// <inheritdoc cref="DataLoaderView.StateChangingThrottleDelay"/>
		public TimeSpan StateChangingThrottleDelay { get; set; } = TimeSpan.Zero;

		/// <summary>
		/// Gets the dispatcher required for UI updates.
		/// </summary>
		public TDispatcher Dispatcher { get; }

		/// <summary>
		/// Sets the <see cref="IDataLoader"/> to be observed by this controller.
		/// </summary>
		/// <param name="dataLoader">The data loader.</param>
		public void SetDataLoader(IDataLoader dataLoader)
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
			}
		}

		/// <summary>
		/// Subscribes to the <see cref="IDataLoader"/>'s <see cref="IDataLoader.StateChanged"/> event and updates the view accordingly.
		/// </summary>
		public void OnViewLoaded()
		{
			if (!_isSubscribedToSource && _dataLoader != null)
			{
				_dataLoader.StateChanged += OnDataLoaderStateChanged;
				_isSubscribedToSource = true;
			}

			_isLoaded = true;
			Update(_dataLoader?.State);
		}

		/// <summary>
		/// Unsubscribes from the <see cref="IDataLoader"/>'s <see cref="IDataLoader.StateChanged"/> event.
		/// </summary>
		public void OnViewUnloaded()
		{
			if (_dataLoader != null)
			{
				_dataLoader.StateChanged -= OnDataLoaderStateChanged;
				_isSubscribedToSource = false;
			}

			_isLoaded = false;
		}

		public void OnControlTemplateApplied()
		{
			_isControlTemplateApplied = true;
			Update(_dataLoader?.State);
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
						if (_isVisualStateRefreshRequired && _isLoaded && _isControlTemplateApplied)
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
					var throttleDelay = StateChangingThrottleDelay;
					var minStateDelay = StateMinimumDuration - elapsedSinceLastUpdate;

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
			var view = Delegate.GetView();
			var viewState = CreateDataLoaderViewState(safeState, view);

			Delegate.SetState(viewState);

			if (view != null)
			{
				var dataVisualState = GetDataVisualState(safeState);
				GoToState(view, DataVisualGroup, dataVisualState);

				var errorVisualState = GetErrorVisualState(safeState);
				GoToState(view, ErrorVisualGroup, errorVisualState);

				var loadingVisualState = GetLoadingStateVisualState(safeState);
				GoToState(view, LoadingVisualGroup, loadingVisualState);

				var combinedVisualState = $"{dataVisualState}_{errorVisualState}_{loadingVisualState}";
				GoToState(view, CombinedVisualGroup, combinedVisualState);

				if (_isLoaded && _isControlTemplateApplied)
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

			_lastUpdate = DateTime.Now;
			_lastState = safeState;
			_lastViewState = viewState;
		}

		/// <summary>
		/// Applies the already saved visual states.
		/// </summary>
		private void RefreshVisualStates()
		{
			var view = Delegate.GetView();
			if (view != null)
			{
				RefreshVisualState(view, DataVisualGroup);
				RefreshVisualState(view, ErrorVisualGroup);
				RefreshVisualState(view, LoadingVisualGroup);
				var combinedVisualState = RefreshVisualState(view, CombinedVisualGroup);

				_isVisualStateRefreshRequired = false;

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation($"Updated VisualStates to '{combinedVisualState}'.");
				}
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

		private DataLoaderViewState CreateDataLoaderViewState(IDataLoaderState state, Control view)
		{
			return new DataLoaderViewState()
			{
				View = view,
				Parent = Delegate.GetDataContext(),
				Source = _dataLoader,
				Request = state.Request,
				Data = state.Data,
				Error = state.Error,
				IsLoading = state.IsLoading,
				IsInitial = state.IsInitial,
				IsEmpty = state.IsEmpty
			};
		}

		protected abstract Task RunOnDispatcher(Action action);

		// This is an optimization to avoid calling VisualStateManager.GoToState if we're already in the right state.
		// It's also used to be able to save the visual states for each group when the control isn't loaded yet and restore them later.
		private Dictionary<string, string> _currentGroupStates = new Dictionary<string, string>();

		private void GoToState(Control control, string group, string visualState, bool useTransitions = true)
		{
			if (_currentGroupStates.TryGetValue(group, out var currentState))
			{
				if (currentState != visualState)
				{
					_currentGroupStates[group] = visualState;
					if (_isLoaded && _isControlTemplateApplied)
					{
						VisualStateManager.GoToState(control, visualState, useTransitions);
					}
				}
			}
			else
			{
				_currentGroupStates[group] = visualState;
				if (_isLoaded && _isControlTemplateApplied)
				{
					VisualStateManager.GoToState(control, visualState, useTransitions);
				}
			}
		}

		private string RefreshVisualState(Control control, string group, bool useTransitions = true)
		{
			if (_currentGroupStates.TryGetValue(group, out var currentState))
			{
				VisualStateManager.GoToState(control, currentState, useTransitions);
				return currentState;
			}
			else
			{
				return null;
			}
		}

		private async Task<IDisposable> LockAsync()
		{
			await _semaphore.WaitAsync();

			return new ActionDisposable(() => _semaphore.Release());
		}
	}
}
