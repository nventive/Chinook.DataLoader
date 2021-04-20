using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Chinook.DataLoader;
using Chinook.DataLoader.SubscribeToData;

namespace System.Collections.Specialized
{
	/// <summary>
	/// Extensions on <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	public static class NotifyCollectionChangedExtensions
	{
		/// <summary>
		/// Subscribes to the data changed event of a <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="collection">The data source binded to the <see cref="IDataLoader"/>.</param>
		/// <param name="setter">The <see cref="IDataLoader"/> to be set.</param>
		/// <returns>The <see cref="IDisposable"/> object.</returns>
		public static IDisposable SubscribeToUpdateDataLoader(this INotifyCollectionChanged collection, IDataLoaderSetter setter)
		{
			collection.CollectionChanged += OnCollectionChanged;

			return new ActionDisposable(() => collection.CollectionChanged -= OnCollectionChanged);

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				setter.SetData(collection);
			}
		}
	}
}
