using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Extensions on <see cref="IDataLoaderContext"/>.
	/// </summary>
	public static class DataLoaderContextExtensions
	{
		public const string DataLoaderNameKey = "DataLoaderName";

		/// <summary>
		/// Gets the name of the <see cref="IDataLoader"/> associated to this <see cref="IDataLoaderContext"/>.
		/// </summary>
		/// <param name="context">The desired <see cref="IDataLoaderContext"/>.</param>
		/// <returns>The name of the associated <see cref="IDataLoader"/> when available; null otherwise.</returns>
		public static string GetDataLoaderName(this IDataLoaderContext context)
		{
			return context[DataLoaderNameKey] as string;
		}
	}
}
