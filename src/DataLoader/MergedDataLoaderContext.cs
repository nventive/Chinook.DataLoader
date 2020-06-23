using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This <see cref="IDataLoaderContext"/> implementation is used by <see cref="DataLoader"/> to merge the context values from a trigger and from the dataloader itself.
	/// Mutations on this class are reflected in the loaderValues dictionary provided from the constructor.
	/// </summary>
	[Preserve(AllMembers = true)]
	public class MergedDataLoaderContext : IDataLoaderContext
	{
		private readonly IEnumerable<KeyValuePair<string, object>> _triggerValues;
		private readonly IDictionary<string, object> _loaderValues;

		/// <summary>
		/// Initializes a new instance of the <see cref="MergedDataLoaderContext"/> class.
		/// </summary>
		public MergedDataLoaderContext(IEnumerable<KeyValuePair<string, object>> triggerValues, IDictionary<string, object> loaderValues)
		{
			_triggerValues = triggerValues;
			_loaderValues = loaderValues;
		}

		private IReadOnlyDictionary<string, object> _mergedValues =>
			_triggerValues
			.Concat(_loaderValues)
			.ToDictionary(
				keySelector: pair => pair.Key,
				elementSelector: pair => pair.Value
			);

		/// <inheritdoc />
		public object this[string key]
		{
			get => _mergedValues.TryGetValue(key, out var value) ? value : null;
			set => _loaderValues[key] = value;
		}

		public IEnumerable<string> Keys => _mergedValues.Keys;

		public IEnumerable<object> Values => _mergedValues.Values;

		public int Count => _mergedValues.Count;

		public bool IsReadOnly => _loaderValues.IsReadOnly;

		ICollection<string> IDictionary<string, object>.Keys => _loaderValues.Keys;

		ICollection<object> IDictionary<string, object>.Values => _loaderValues.Values;

		public void Add(string key, object value) => _loaderValues.Add(key, value);

		public void Add(KeyValuePair<string, object> item) => _loaderValues.Add(item);

		public void Clear() => _loaderValues.Clear();

		public bool Contains(KeyValuePair<string, object> item) => _mergedValues.Contains(item);

		public bool ContainsKey(string key) => _mergedValues.ContainsKey(key);

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException($"{nameof(CopyTo)} is not supported on {nameof(MergedDataLoaderContext)}.");

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _mergedValues.GetEnumerator();

		public bool Remove(string key) => _loaderValues.Remove(key);

		public bool Remove(KeyValuePair<string, object> item) => _loaderValues.Remove(item);

		public bool TryGetValue(string key, out object value) => _mergedValues.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _mergedValues.GetEnumerator();
	}
}
