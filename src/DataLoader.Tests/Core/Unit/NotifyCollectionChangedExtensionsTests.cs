using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chinook.DataLoader.SubscribeToData;
using FluentAssertions;
using Xunit;

namespace Tests.Core.Unit
{
	public class NotifyCollectionChangedExtensionsTests
	{
		[Fact]
		public void Invokes_SetData_when_collection_changes()
		{
			// Arrange
			var collection = new ObservableCollection<int>();
			var setter = new TestSetter();
			NotifyCollectionChangedExtensions.SubscribeToCollectionChanged(collection, setter);

			setter.Data.Should().BeNull();

			// Act
			collection.Add(1);

			// Assert
			setter.Data.Should().BeSameAs(collection);
			setter.DataSetCount.Should().Be(1);
		}

		[Fact]
		public void Stops_invoking_SetData_when_subscription_is_disposed()
		{
			// Arrange
			var collection = new ObservableCollection<int>();
			var setter = new TestSetter();
			var subscription = NotifyCollectionChangedExtensions.SubscribeToCollectionChanged(collection, setter);
			collection.Add(1);

			// Act
			subscription.Dispose();
			collection.Add(1);

			// Assert
			setter.DataSetCount.Should().Be(1);
		}

		private class TestSetter : IDataLoaderSetter
		{
			public int DataSetCount { get; private set; }

			public object Data { get; private set; }

			public void SetData(object data)
			{
				Data = data;
				++DataSetCount;
			}

			public void SetError(Exception error)
			{
				throw new NotImplementedException();
			}
		}
	}
}
