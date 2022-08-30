using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// State of a <see cref="IDataLoader"/>.
	/// </summary>
	public interface IDataLoaderState
	{
		/// <summary>
		/// The request that created that state.
		/// </summary>
		IDataLoaderRequest Request { get; }

		/// <summary>
		/// Determines whether the data loader was ever successfully completed.
		/// This is true by default until <see cref="Data"/> is set.
		/// </summary>
		bool IsInitial { get; }

		/// <summary>
		/// Determines whether the data loader should be considered empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Determines whether the state is final.
		/// This is true while the data loader's Load method is being executed.
		/// </summary>
		bool IsLoading { get; }

		/// <summary>
		/// The data that resulted from the request.
		/// </summary>
		object Data { get; }

		/// <summary>
		/// The error that resulted from the request, if any.
		/// </summary>
		Exception Error { get; }
	}

	/// <summary>
	/// Typed version of <see cref="IDataLoaderState"/>.
	/// </summary>
	/// <typeparam name="TData">The type of data.</typeparam>
	public interface IDataLoaderState<out TData> : IDataLoaderState
	{
		/// <inheritdoc cref="IDataLoaderState.Data"/>
		new TData Data { get; }
	}
}
