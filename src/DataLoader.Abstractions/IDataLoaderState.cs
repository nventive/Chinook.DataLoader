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
		/// Determines whether or not loader was ever successfully completed.
		/// </summary>
		bool IsInitial { get; }

		/// <summary>
		/// Determines whether or not loader should be considered empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Determines whether or not the state is final.
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
	/// <typeparam name="TData"></typeparam>
	public interface IDataLoaderState<out TData> : IDataLoaderState
	{
		/// <inheritdoc cref="IDataLoaderState.Data"/>
		new TData Data { get; }
	}
}
