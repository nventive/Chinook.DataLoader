using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Chinook.DataLoader;
using Chinook.DataLoader.SubscribeToData;

namespace System.Collections.Specialized
{
	public static class NotifyCollectionChangedExtensions
	{
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
