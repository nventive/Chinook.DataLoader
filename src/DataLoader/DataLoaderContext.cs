using System;
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
	}
}
