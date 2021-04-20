using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Default implementation of <see cref="IDataLoaderBuilder"/>.
	/// </summary>
	public class DataLoaderBuilder : IDataLoaderBuilder
	{
		/// <summary>
		/// Represents an empty <see cref="DataLoaderBuilder"/>.
		/// </summary>
		public static DataLoaderBuilder Empty => new DataLoaderBuilder();

		/// <inheritdoc cref="IDataLoaderBuilder.Name"/>
		public string Name { get; set; }

		/// <inheritdoc cref="IDataLoaderBuilder.DataType"/>
		public Type DataType { get; set; }

		/// <inheritdoc cref="IDataLoaderBuilder.LoadMethod"/>
		public DataLoaderDelegate LoadMethod { get; set; }

		/// <inheritdoc cref="IDataLoaderBuilder.ConcurrentMode"/>
		public DataLoaderConcurrentMode ConcurrentMode { get; set; }

		/// <inheritdoc cref="IDataLoaderBuilder.DelegatingStrategies"/>
		public IList<DelegatingDataLoaderStrategy> DelegatingStrategies { get; set; } = new List<DelegatingDataLoaderStrategy>();

		/// <inheritdoc cref="IDataLoaderBuilder.TriggerProviders"/>
		public IList<Func<IDataLoader, IDataLoaderTrigger>> TriggerProviders { get; set; } = new List<Func<IDataLoader, IDataLoaderTrigger>>();

		/// <inheritdoc cref="IDataLoaderBuilder.EmptySelector"/>
		public Func<IDataLoaderState, bool> EmptySelector { get; set; } = DefaultEmptySelector;

		/// <inheritdoc cref="IDataLoaderBuilder.Build"/>
		public IDataLoader Build()
		{
			var strategy = GetStrategy(LoadMethod, DelegatingStrategies);

			var dataLoader = new DataLoader(
				Name,
				strategy,
				ConcurrentMode,
				EmptySelector
			);

			foreach (var triggerProvider in TriggerProviders)
			{
				dataLoader.AddTrigger(triggerProvider(dataLoader));
			}

			return dataLoader;
		}

		private static bool DefaultEmptySelector(IDataLoaderState state)
		{
			return false;
		}

		private static IDataLoaderStrategy GetStrategy(DataLoaderDelegate loadMethod, IList<DelegatingDataLoaderStrategy> delegatingStrategies)
		{
			IDataLoaderStrategy strategy = new DelegatedDataLoaderStrategy(loadMethod);
			foreach (var delegatingStrategy in delegatingStrategies)
			{
				// Stitch up all the delegating strategies together.
				delegatingStrategy.InnerStrategy = strategy;
				strategy = delegatingStrategy;
			}

			return strategy;
		}
	}

	/// <summary>
	/// Typed version of <see cref="DataLoaderBuilder"/>.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public class DataLoaderBuilder<TData> : DataLoaderBuilder, IDataLoaderBuilder<TData>
	{
		/// <summary>
		/// Represents an empty <see cref="DataLoaderBuilder{TData}"/>.
		/// </summary>
		public new static DataLoaderBuilder<TData> Empty => new DataLoaderBuilder<TData>();

		public DataLoaderBuilder()
		{
			DataType = typeof(TData);
		}

		/// <inheritdoc cref="IDataLoaderBuilder.EmptySelector"/>
		Func<IDataLoaderState<TData>, bool> IDataLoaderBuilder<TData>.EmptySelector
		{
			get => base.EmptySelector;
			set => base.EmptySelector = state => value(state.AsOf<TData>());
		}

		/// <inheritdoc cref="IDataLoaderBuilder.Build"/>
		IDataLoader<TData> IDataLoaderBuilder<TData>.Build()
		{
			var dataLoader = base.Build();
			return new TypedDataLoaderDecorator<TData>(dataLoader);
		}
	}
}
