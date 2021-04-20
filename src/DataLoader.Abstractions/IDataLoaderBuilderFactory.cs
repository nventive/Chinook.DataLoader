using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderBuilderFactory"/> creating <see cref="IDataLoaderBuilder"/> instances.
	/// </summary>
	/// <remarks>
	/// The goal of implementors should be to inject default properties when creating builders.
	/// If you want all the <see cref="IDataLoader"/>s of your app to use the <see cref="DataLoaderConcurrentMode.DiscardNew"/> mode,
	/// you should leverage the <see cref="IDataLoaderBuilderFactory"/> to inject that property in all <see cref="IDataLoaderBuilder"/> instances.
	/// </remarks>
	public interface IDataLoaderBuilderFactory
	{
		/// <summary>
		/// Creates a new <see cref="IDataLoader"/>.
		/// </summary>
		IDataLoaderBuilder Create();

		/// <summary>
		/// Creates a new <see cref="IDataLoader{TData}"/>.
		/// </summary>
		/// <typeparam name="TData">The type of the data loaded by the <see cref="IDataLoader{TData}"/>.</typeparam>
		IDataLoaderBuilder<TData> CreateTyped<TData>();
	}
}
