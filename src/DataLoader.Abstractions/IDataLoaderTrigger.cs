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

	/// <summary>
	/// The delegate used for the <see cref="IDataLoaderTrigger.LoadRequested"/> event.
	/// </summary>
	/// <param name="trigger">The trigger raising the event.</param>
	/// <param name="context">Additional metadata. Items added are kept for the lifetime of the resulting <see cref="IDataLoaderRequest"/>.</param>
	public delegate void LoadRequestedEventHandler(IDataLoaderTrigger trigger, IDataLoaderContext context);
}
