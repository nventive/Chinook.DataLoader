using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This is a default implementation of <see cref="IDataLoaderBuilderFactory"/>.
	/// </summary>
	[Preserve(AllMembers = true)]
	public class DataLoaderBuilderFactory : IDataLoaderBuilderFactory
	{
		private readonly Func<IDataLoaderBuilder, IDataLoaderBuilder> _defaultConfigure;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderBuilderFactory"/> class.
		/// </summary>
		/// <param name="defaultConfigure">Default builder configuration</param>
		public DataLoaderBuilderFactory(Func<IDataLoaderBuilder, IDataLoaderBuilder> defaultConfigure = null)
		{
			_defaultConfigure = defaultConfigure;
		}

		/// <inheritdoc />
		public IDataLoaderBuilder Create()
		{
			IDataLoaderBuilder builder = new DataLoaderBuilder();

			if (_defaultConfigure != null)
			{
				builder = _defaultConfigure(builder);
			}

			return builder;
		}

		/// <inheritdoc />
		public IDataLoaderBuilder<TData> CreateTyped<TData>()
		{
			IDataLoaderBuilder<TData> builder = new DataLoaderBuilder<TData>();

			if (_defaultConfigure != null)
			{
				builder = (IDataLoaderBuilder<TData>)_defaultConfigure(builder);
			}

			return builder;
		}
	}
}
