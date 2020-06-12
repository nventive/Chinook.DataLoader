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
		/// <param name="values">Values</param>
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

		public bool IsReadOnly => _values.IsReadOnly;

		ICollection<string> IDictionary<string, object>.Keys => _values.Keys;

		ICollection<object> IDictionary<string, object>.Values => _values.Values;

		public void Add(string key, object value) => _values.Add(key, value);

		public void Add(KeyValuePair<string, object> item) => _values.Add(item);

		public void Clear() => _values.Clear();

		public bool Contains(KeyValuePair<string, object> item) => _values.Contains(item);

		public bool ContainsKey(string key) => _values.ContainsKey(key);

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();

		public bool Remove(string key) => _values.Remove(key);

		public bool Remove(KeyValuePair<string, object> item) => _values.Remove(item);

		public bool TryGetValue(string key, out object value) => _values.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
	}
}
