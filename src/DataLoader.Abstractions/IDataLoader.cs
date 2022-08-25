using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoader"/> is a state machine responsible of loading data asynchronously.
	/// </summary>
	/// <remarks>
	/// Loading data goes through multiple states, each state being represented by a<see cref="IDataLoaderState"/>.
	/// Loading data can be triggered by using <see cref="IDataLoaderTrigger"/>.
	/// </remarks>
	public interface IDataLoader : IDisposable
	{
		/// <summary>
		/// Name of the the loader.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Represents the current state of the <see cref="IDataLoader"/>.
		/// </summary>
		IDataLoaderState State { get; }

		/// <summary>
		/// Occurs when the <see cref="State"/> has changed.
		/// </summary>
		event StateChangedEventHandler StateChanged;

		/// <summary>
		/// Manually loads data from the <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="ct">The <see cref="CancellationToken"/>.</param>
		/// <param name="context">The optional <see cref="IDataLoaderContext"/>.</param>
		/// <returns>The loaded data.</returns>
		Task<object> Load(CancellationToken ct, IDataLoaderContext context = null);

		/// <summary>
		/// The triggers causing the loads of this <see cref="IDataLoader"/>.
		/// </summary>
		IEnumerable<IDataLoaderTrigger> Triggers { get; }

		/// <summary>
		/// Adds a trigger to <see cref="Triggers"/>.
		/// </summary>
		/// <param name="trigger">The trigger to add.</param>
		void AddTrigger(IDataLoaderTrigger trigger);

		/// <summary>
		/// Removes a trigger from <see cref="Triggers"/>.
		/// </summary>
		/// <param name="trigger">The trigger to remove.</param>
		void RemoveTrigger(IDataLoaderTrigger trigger);
	}

	/// <summary>
	/// The typed version of <see cref="IDataLoader"/>.
	/// </summary>
	/// <typeparam name="TData">The type of data.</typeparam>
	public interface IDataLoader<TData> : IDataLoader
	{
		/// <inheritdoc cref="IDataLoader.State"/>
		new IDataLoaderState<TData> State { get; }

		/// <inheritdoc cref="IDataLoader.Load(CancellationToken, IDataLoaderContext)"/>
		new Task<TData> Load(CancellationToken ct, IDataLoaderContext context = null);
	}

	/// <summary>
	/// The event handler for the <see cref="IDataLoader.StateChanged"/> event.
	/// </summary>
	/// <param name="dataLoader">The data loader that raises the event.</param>
	/// <param name="newState">The new state the data loader is in.</param>
	public delegate void StateChangedEventHandler(IDataLoader dataLoader, IDataLoaderState newState);

	/// <summary>
	/// The load delegate used by <see cref="IDataLoaderBuilder.LoadMethod"/> to define the load method of a <see cref="IDataLoader"/>.
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	/// <param name="request">The data loader request.</param>
	/// <returns>A task of object, where the object is the data loaded by the data loader.</returns>
	public delegate Task<object> DataLoaderDelegate(CancellationToken ct, IDataLoaderRequest request);

	/// <inheritdoc cref="DataLoaderDelegate"/>
	public delegate Task<TData> DataLoaderDelegate<TData>(CancellationToken ct, IDataLoaderRequest request);

	/// <summary>
	/// Data loader concurrency mode.
	/// </summary>
	public enum DataLoaderConcurrentMode
	{
		/// <summary>
		/// When there's a new request while a previous one is already loading, the previous request gets cancelled.
		/// </summary>
		CancelPrevious,

		/// <summary>
		/// When there's a new request while a previous one is already loading, the new request gets discarded.
		/// </summary>
		DiscardNew,
	}
}
