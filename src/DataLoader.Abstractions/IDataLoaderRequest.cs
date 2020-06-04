using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderRequest"/> is a request to load a <see cref="IDataLoader"/>.
	/// </summary>
	public interface IDataLoaderRequest
	{
		/// <summary>
		/// Trigger that originated the request.
		/// </summary>
		IDataLoaderTrigger SourceTrigger { get; }

		/// <summary>
		/// Metadata contained in the request.
		/// </summary>
		IDataLoaderContext Context { get; }
	}
}
