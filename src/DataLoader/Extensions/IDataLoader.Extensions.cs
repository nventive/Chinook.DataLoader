using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Extensions on <see cref="IDataLoader"/>.
	/// </summary>
	public static class DataLoaderExtensions
	{
		/// <summary>
		/// Returns a typed version of <see cref="IDataLoader"/>.
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="dataLoader">The <see cref="IDataLoader"/>.</param>
		/// <returns><see cref="IDataLoaderState{TData}"/>.</returns>
		public static IDataLoader<TData> AsOf<TData>(this IDataLoader dataLoader)
		{
			if (dataLoader is IDataLoader<TData> typedDataLoader)
			{
				return typedDataLoader;
			}
			else
			{
				return new TypedDataLoaderDecorator<TData>(dataLoader);
			}
		}
	}
}
