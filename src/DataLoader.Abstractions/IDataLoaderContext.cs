using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderContext"/> represents metadata that
	/// was added to the <see cref="IDataLoaderRequest"/> before and after its execution.
	/// </summary>
	public interface IDataLoaderContext
	{
		/// <summary>
		/// Gets or sets a value for the given key.
		/// </summary>
		/// <param name="key">Key</param>
		/// <remarks>If the key is not found, null is returned.</remarks>
		/// <returns>Value</returns>
		object this[string key] { get; set; }
	}
}
