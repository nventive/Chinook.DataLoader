using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This controller encapsulates all the reusable logic needed by <see cref="DataLoaderView"/> or other view implementations of <see cref="IDataLoader"/>.<br/>
	/// </summary>
	public class DataLoaderViewController : DataLoaderViewControllerBase<DispatcherQueue>
	{
		public DataLoaderViewController(IDataLoaderViewDelegate dataLoaderViewDelegate, DispatcherQueue dispatcher) : base(dataLoaderViewDelegate, dispatcher)
		{
		}

		protected override async Task RunOnDispatcher(Action action)
		{
			if (Dispatcher.HasThreadAccess)
			{
				action();
			}
			else
			{
				Dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => action());
			}
		}
	}
}
