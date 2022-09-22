using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This controller encapsulates all the reusable logic needed by <see cref="DataLoaderView"/> or other view implementations of <see cref="IDataLoader"/>.<br/>
	/// </summary>
	public class DataLoaderViewController : DataLoaderViewControllerBase<CoreDispatcher>
	{
		public DataLoaderViewController(IDataLoaderViewDelegate dataLoaderViewDelegate, CoreDispatcher dispatcher) : base(dataLoaderViewDelegate, dispatcher)
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
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
			}
		}
	}
}
