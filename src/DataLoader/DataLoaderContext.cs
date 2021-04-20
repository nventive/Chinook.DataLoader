using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This is a default implementation of <see cref="IDataLoaderContext"/>.
	/// </summary>
	[Preserve(AllMembers = true)]
	public class DataLoaderContext : IDataLoaderContext
	{
		private readonly IDictionary<string, object> _values;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderContext"/> class.
		/// </summary>
		/// <param name="values">The values.</param>
		public DataLoaderContext(IDictionary<string, object> values = null)
		{
			_values = values ?? new Dictionary<string, object>();
		}

		/// <inheritdoc />
		public object this[string key]
		{
			get => _values.TryGetValue(key, out var value) ? value : null;
			set => _values[key] = value;
		}

		public IEnumerable<string> Keys => _values.Keys;

		public IEnumerable<object> Values => _values.Values;

		public int Count => _values.Count;

		public void Add(string key, object value) => _values.Add(key, value);

		public void Clear() => _values.Clear();

		public bool ContainsKey(string key) => _values.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();

		public bool Remove(string key) => _values.Remove(key);

		public bool TryGetValue(string key, out object value) => _values.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
	}
}
