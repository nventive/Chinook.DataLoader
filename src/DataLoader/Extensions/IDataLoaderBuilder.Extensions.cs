using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Extensions on <see cref="IDataLoaderBuilder"/>.
	/// </summary>
	public static class DataLoaderBuilderExtensions2
	{
		/// <summary>
		/// Adds a <see cref="IDataLoaderTrigger"/> in <see cref="IDataLoaderBuilder.TriggerProviders"/>.
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="builder">The <see cref="IDataLoaderBuilder"/> reference.</param>
		/// <param name="triggerProvider">The new <see cref="IDataLoaderTrigger"/> to add.</param>
		public static IDataLoaderBuilder<TData> WithTrigger<TData>(this IDataLoaderBuilder<TData> builder, Func<IDataLoader<TData>, IDataLoaderTrigger> triggerProvider)
		{
			builder.TriggerProviders.Add(GetTriggerProvider);
			return builder;

			IDataLoaderTrigger GetTriggerProvider(IDataLoader dataLoader)
			{				
				return triggerProvider(dataLoader.AsOf<TData>());				
			}
		}
	}
}
