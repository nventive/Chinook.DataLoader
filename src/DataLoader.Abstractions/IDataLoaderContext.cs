using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderContext"/> represents metadata added to the <see cref="IDataLoaderRequest"/> before and during its execution.
	/// </summary>
	/// <remarks>
	/// We don't implement <see cref="IReadOnlyDictionary{TKey, TValue}"/> because the xaml binding engine has a special behavior for types implementing this interface.
	/// The xaml engine uses <see cref="IReadOnlyDictionary{TKey, TValue}.ContainsKey(TKey)"/> instead of the indexer, which we don't want.
	/// </remarks>
	public interface IDataLoaderContext : IEnumerable<KeyValuePair<string, object>>
	{
		/// <inheritdoc cref="IDictionary{TKey, TValue}.Add"/>
		void Add(string key, object value);

		/// <inheritdoc cref="IDictionary{TKey, TValue}.Remove(TKey)"/>
		bool Remove(string key);

		/// <summary>
		/// Gets the count of items in the <see cref="IDataLoaderContext"/>.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Gets or sets a value for the given key.
		/// </summary>
		/// <param name="key">The key name of the desired item.</param>
		/// <returns>The value associated to the key when available; null otherwise.</returns>
		object this[string key] { get; set; }

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.Keys"/>
		IEnumerable<string> Keys { get; }

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.Values"/>
		IEnumerable<object> Values { get; }

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.ContainsKey(TKey)"/>
		bool ContainsKey(string key);

		/// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
		bool TryGetValue(string key, out object value);

		/// <summary>
		/// Removes all items from the <see cref="IDataLoaderContext"/>.
		/// </summary>
		void Clear();
	}
}
