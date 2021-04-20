using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Chinook.DataLoader;
using Chinook.DataLoader.SubscribeToData;

namespace System.Collections.Specialized
{
	/// <summary>
	/// This class offers extensions on <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	public static class NotifyCollectionChangedExtensions
	{
		/// <summary>
		/// Subscribes to CollectionChanged to automatically set a <see cref="IDataLoader"/>'s data.
		/// </summary>
		/// <param name="collection">The <see cref="INotifyCollectionChanged"/> data.</param>
		/// <param name="setter">The <see cref="IDataLoader"/> to be set.</param>
		/// <returns>The <see cref="IDisposable"/> object.</returns>
		public static IDisposable SubscribeToCollectionChanged(this INotifyCollectionChanged collection, IDataLoaderSetter setter)
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
