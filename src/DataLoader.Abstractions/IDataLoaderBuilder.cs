using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This simplifies the creation of <see cref="IDataLoader"/> instances.
	/// </summary>
	public interface IDataLoaderBuilder
	{
		/// <summary>
		/// The name of the <see cref="IDataLoader"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		string Name { get; set; }

		/// <summary>
		/// The type of data loaded by the <see cref="IDataLoader"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Type DataType { get; set; }

		/// <summary>
		/// The method used to load the data of the <see cref="IDataLoader"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		DataLoaderDelegate LoadMethod { get; set; }

		/// <summary>
		/// The mode to used when concurrent loads occur.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		DataLoaderConcurrentMode ConcurrentMode { get; set; }

		/// <summary>
		/// The <see cref="IDataLoaderStrategy"/> that will be used to load the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		IList<DelegatingDataLoaderStrategy> DelegatingStrategies { get; set; }

		/// <summary>
		/// The list of triggers that will reload the <see cref="IDataLoader"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		IList<Func<IDataLoader, IDataLoaderTrigger>> TriggerProviders { get; set; }

		/// <summary>
		/// The method to invoke to determine whether a <see cref="IDataLoaderState"/> is considered empty.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Func<IDataLoaderState, bool> EmptySelector { get; set; }

		/// <summary>
		/// Builds a new instance of <see cref="IDataLoader"/>.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		IDataLoader Build();
	}

	/// <summary>
	/// This simplifies the creation of <see cref="IDataLoader{TData}"/> instances.
	/// </summary>
	public interface IDataLoaderBuilder<TData> : IDataLoaderBuilder
	{
		/// <inheritdoc cref="IDataLoaderBuilder.EmptySelector"/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new Func<IDataLoaderState<TData>, bool> EmptySelector { get; set; }

		/// <inheritdoc cref="IDataLoaderBuilder.Build"/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new IDataLoader<TData> Build();
	}	
}
