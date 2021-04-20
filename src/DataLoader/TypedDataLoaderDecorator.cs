using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This class decorates an <see cref="IDataLoader"/> with the <see cref="IDataLoader{TData}"/> interface.
	/// </summary>
	/// <typeparam name="TData">Type of data</typeparam>
	public class TypedDataLoaderDecorator<TData> : IDataLoader<TData>
	{
		// This class is a decorator and shouldn't have any logic.
		// It must forward execution to its inner IDataLoader.

		public TypedDataLoaderDecorator(IDataLoader innerDataLoader)
		{
			InnerDataLoader = innerDataLoader;
		}

		public IDataLoader InnerDataLoader { get; }

		/// <inheritdoc cref="IDataLoader{TData}.State" />
		IDataLoaderState<TData> IDataLoader<TData>.State => InnerDataLoader.State.AsOf<TData>();

		/// <inheritdoc cref="IDataLoader{TData}.Load(CancellationToken, IDataLoaderContext)" />
		async Task<TData> IDataLoader<TData>.Load(CancellationToken ct, IDataLoaderContext context)
		{
			var result = await InnerDataLoader.Load(ct, context);
			return (TData)result;
		}

		#region Non-Generic Decoration

		/// <inheritdoc cref="IDataLoader.Name" />
		public string Name => InnerDataLoader.Name;

		/// <inheritdoc cref="IDataLoader.State" />
		public IDataLoaderState State => InnerDataLoader.State;

		/// <inheritdoc cref="IDataLoader.Triggers" />
		public IEnumerable<IDataLoaderTrigger> Triggers => InnerDataLoader.Triggers;

		/// <inheritdoc cref="IDataLoader.StateChanged" />
		public event StateChangedEventHandler StateChanged
		{
			add => InnerDataLoader.StateChanged += value;
			remove => InnerDataLoader.StateChanged -= value;
		}

		/// <inheritdoc cref="IDataLoader.AddTrigger(IDataLoaderTrigger)" />
		public void AddTrigger(IDataLoaderTrigger trigger) => InnerDataLoader.AddTrigger(trigger);

		/// <inheritdoc />
		public void Dispose() => InnerDataLoader.Dispose();

		/// <inheritdoc cref="IDataLoader.Load(CancellationToken, IDataLoaderContext)" />
		public Task<object> Load(CancellationToken ct, IDataLoaderContext context = null) => InnerDataLoader.Load(ct, context);

		/// <inheritdoc cref="IDataLoader.RemoveTrigger(IDataLoaderTrigger)" />
		public void RemoveTrigger(IDataLoaderTrigger trigger) => InnerDataLoader.RemoveTrigger(trigger);
		#endregion
	}
}
