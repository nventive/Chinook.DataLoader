using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Extensions on <see cref="IDataLoaderBuilder"/>.
	/// </summary>
	public static class DataLoaderBuilderExtensions
	{
		/// <inheritdoc cref="IDataLoaderBuilder.Name"/>
		public static TBuilder WithName<TBuilder>(this TBuilder builder, string name) where TBuilder : IDataLoaderBuilder
		{
			builder.Name = name;
			return builder;
		}

		/// <inheritdoc cref="IDataLoaderBuilder.LoadMethod"/>
		public static IDataLoaderBuilder WithLoadMethod(this IDataLoaderBuilder builder, DataLoaderDelegate loadMethod)
		{
			builder.LoadMethod = loadMethod;
			return builder;
		}

		/// <inheritdoc cref="IDataLoaderBuilder.LoadMethod"/>
		public static IDataLoaderBuilder<TData> WithLoadMethod<TData>(this IDataLoaderBuilder<TData> builder, DataLoaderDelegate<TData> loadMethod)
		{
			builder.LoadMethod = InnerLoad;
			return builder;

			async Task<object> InnerLoad(CancellationToken ct, IDataLoaderRequest request)
			{
				return await loadMethod(ct, request);
			}
		}

		/// <inheritdoc cref="IDataLoaderBuilder.ConcurrentMode"/>
		public static TBuilder WithConcurrentMode<TBuilder>(this TBuilder builder, DataLoaderConcurrentMode concurrentMode) where TBuilder : IDataLoaderBuilder
		{
			builder.ConcurrentMode = concurrentMode;
			return builder;
		}

		/// <summary>
		/// Adds a <see cref="DelegatingDataLoaderStrategy"/> in <see cref="IDataLoaderBuilder.DelegatingStrategies"/>.
		/// </summary>
		/// <typeparam name="TBuilder">Any type implementing <see cref="IDataLoaderBuilder"/>.</typeparam>
		/// <param name="builder">The <see cref="IDataLoaderBuilder"/> reference.</param>
		/// <param name="delegatingStrategy">The new <see cref="DelegatingDataLoaderStrategy"/> to add.</param>
		public static TBuilder WithStrategy<TBuilder>(this TBuilder builder, DelegatingDataLoaderStrategy delegatingStrategy) where TBuilder : IDataLoaderBuilder
		{
			builder.DelegatingStrategies.Add(delegatingStrategy);
			return builder;
		}

		/// <summary>
		/// Adds a <see cref="IDataLoaderTrigger"/> in <see cref="IDataLoaderBuilder.TriggerProviders"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IDataLoaderBuilder"/> reference.</param>
		/// <param name="triggerProvider">The new <see cref="IDataLoaderTrigger"/> to add.</param>
		public static IDataLoaderBuilder WithTrigger(this IDataLoaderBuilder builder, Func<IDataLoader, IDataLoaderTrigger> triggerProvider)
		{
			builder.TriggerProviders.Add(triggerProvider);
			return builder;
		}

		

		/// <summary>
		/// Adds a <see cref="IDataLoaderTrigger"/> in <see cref="IDataLoaderBuilder.TriggerProviders"/>.
		/// </summary>
		/// <typeparam name="TBuilder">Any type implementing <see cref="IDataLoaderBuilder"/>.</typeparam>
		/// <param name="builder">The <see cref="IDataLoaderBuilder"/> reference.</param>
		/// <param name="trigger">The new <see cref="IDataLoaderTrigger"/> to add.</param>
		public static TBuilder WithTrigger<TBuilder>(this TBuilder builder, IDataLoaderTrigger trigger) where TBuilder : IDataLoaderBuilder
		{
			builder.TriggerProviders.Add(GetTrigger);
			return builder;

			IDataLoaderTrigger GetTrigger(IDataLoader dataLoader)
			{
				return trigger;
			}
		}

		/// <inheritdoc cref="IDataLoaderBuilder.EmptySelector"/>
		public static IDataLoaderBuilder WithEmptySelector(this IDataLoaderBuilder builder, Func<IDataLoaderState, bool> emptySelector)
		{
			builder.EmptySelector = emptySelector;
			return builder;
		}

		/// <inheritdoc cref="IDataLoaderBuilder.EmptySelector"/>
		public static IDataLoaderBuilder<TData> WithEmptySelector<TData>(this IDataLoaderBuilder<TData> builder, Func<IDataLoaderState<TData>, bool> emptySelector)
		{
			builder.EmptySelector = emptySelector;
			return builder;
		}
	}
}
