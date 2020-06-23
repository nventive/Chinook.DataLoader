using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader.SubscribeToData;
using Xunit;

namespace Chinook.DataLoader.Tests.Core.Integration
{
	public class SubscribeToDataTests
	{
		[Fact]
		public async Task TrySubscribeToData_works_when_using_matching_types()
		{ 
			var factory = new DataLoaderBuilderFactory(b => b
				.TrySubscribeToData<ITestData>((data, setter) => data.Subscribe(s => setter.SetData(data)))
			);

			var builder = factory
				.CreateTyped<MyData>()
				.WithName("Loader")
				.WithLoadMethod(Load);

			var dataLoader = builder.Build();

			// The recipe is applicable when a SubscribeToDataTrigger has been added to the DataLoader.
			Assert.Contains(dataLoader.Triggers, t => t is SubscribeToDataTrigger);

			var data = await dataLoader.Load(CancellationToken.None);

			Assert.True(data.IsSubscribed);

			async Task<MyData> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return new MyData();
			}
		}

		[Fact]
		public async Task TrySubscribeToData_doesnt_work_when_using_non_matching_types()
		{
			var factory = new DataLoaderBuilderFactory(b => b
				.TrySubscribeToData<ITestData>((data, setter) => data.Subscribe(s => setter.SetData(data)))
			);

			var builder = factory
				.CreateTyped<string>()
				.WithName("Loader")
				.WithLoadMethod(Load);

			var dataLoader = builder.Build();

			// The triggers must not contain the SubscribeToDataTrigger because the recipe doesn't apply for a DataLoader of string.
			Assert.DoesNotContain(dataLoader.Triggers, t => t is SubscribeToDataTrigger);

			async Task<string> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return string.Empty;
			}
		}

		[Fact]
		public void TrySubscribeToData_doesnt_work_on_untyped_builders()
		{
			var factory = new DataLoaderBuilderFactory(b => b
				.TrySubscribeToData<ITestData>((data, setter) => data.Subscribe(s => setter.SetData(data)))
			);

			// Using Create() instead of CreateTyped() must log an error and not apply the recipe.
			var builder = factory
				.Create()
				.WithName("Loader")
				.WithLoadMethod(Load);

			Assert.DoesNotContain(builder.DelegatingStrategies, s => s is SubscribeToDataStrategy);

			async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return new MyData();
			}
		}

		[Fact]
		public async Task Subscription_is_disposed_on_new_loads()
		{
			var dataLoader = GetHappyPathDataLoader();

			var data = await dataLoader.Load(CancellationToken.None);

			Assert.False(data.IsSubscriptionDisposed);

			// Loading new data must dispose the subscription.
			var data2 = await dataLoader.Load(CancellationToken.None);

			Assert.True(data.IsSubscriptionDisposed);
		}

		[Fact]
		public async Task Subscription_is_disposed_with_dataloader()
		{
			var dataLoader = GetHappyPathDataLoader();

			var data = await dataLoader.Load(CancellationToken.None);

			Assert.False(data.IsSubscriptionDisposed);

			// Disposing the DataLoader must dispose the subscription.
			dataLoader.Dispose();

			Assert.True(data.IsSubscriptionDisposed);
		}

		[Fact]
		public async Task IsEmpty_is_updated_when_data_changes()
		{
			var dataLoader = GetHappyPathDataLoader();

			var data = await dataLoader.Load(CancellationToken.None);

			Assert.True(dataLoader.State.IsEmpty);

			// Updating the message will go through the data subscription and re-evaluate the IsEmpty property.
			data.SetMessage("not empty");

			Assert.False(dataLoader.State.IsEmpty);

			data.SetMessage(null);

			Assert.True(dataLoader.State.IsEmpty);
		}

		private static IDataLoader<MyData> GetHappyPathDataLoader()
		{
			var factory = new DataLoaderBuilderFactory(b => b
				.TrySubscribeToData<ITestData>((data, setter) => data.Subscribe(s => setter.SetData(data)))
			);

			var builder = factory
				.CreateTyped<MyData>()
				.WithEmptySelector(state => state.Data?.Message == null)
				.WithName("Loader")
				.WithLoadMethod(Load);

			var dataLoader = builder.Build();
			return dataLoader;

			async Task<MyData> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return new MyData();
			}
		}

		private interface ITestData
		{
			string Message { get; }

			IDisposable Subscribe(Action<string> onDataChanged);
		}

		private class MyData : ITestData
		{
			private Action<string> _onMessageChanged;

			public bool IsSubscribed { get; private set; }

			public bool IsSubscriptionDisposed { get; private set; }

			public string Message { get; private set; }

			public void SetMessage(string message)
			{
				Message = message;
				_onMessageChanged(message);
			}

			public IDisposable Subscribe(Action<string> onMessageChanged)
			{
				_onMessageChanged = onMessageChanged;

				IsSubscribed = true;

				return new ActionDisposable(() => { IsSubscriptionDisposed = true; });
			}
		}
	}
}
