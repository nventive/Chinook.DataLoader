using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This represents an object that can trigger a Load operation on an <see cref="IDataLoader"/>.
	/// </summary>
	public interface IDataLoaderTrigger : IDisposable
	{
		/// <summary>
		/// Name of the the trigger.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Occurs when a load is requested.
		/// </summary>
		event LoadRequestedEventHandler LoadRequested;
	}

	public delegate void LoadRequestedEventHandler(IDataLoaderTrigger trigger, IDataLoaderContext context);
}
