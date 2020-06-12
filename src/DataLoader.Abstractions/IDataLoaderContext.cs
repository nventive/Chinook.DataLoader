using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderContext"/> represents metadata added to the <see cref="IDataLoaderRequest"/> before and during its execution.
	/// </summary>
	public interface IDataLoaderContext : IDictionary<string, object>
	{
		/// <summary>
		/// Gets or sets a value for the given key.
		/// If the key is not found, null is returned.
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns>The value associated to the key when available; null otherwise.</returns>
		new object this[string key] { get; set; }

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.Keys"/>
		/// <remarks>
		/// We hide <see cref="IDictionary{TKey, TValue}.Keys"/> because it's mutable and we want an immutable collection of keys.
		/// </remarks>
		new IEnumerable<string> Keys { get; }

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.Values"/>
		/// <remarks>
		/// We hide <see cref="IDictionary{TKey, TValue}.Values"/> because it's mutable and we want an immutable collection of values.
		/// </remarks>
		new IEnumerable<object> Values { get; }
	}
}
