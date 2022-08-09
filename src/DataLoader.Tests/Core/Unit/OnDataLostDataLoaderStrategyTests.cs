using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader;
using FluentAssertions;
using Xunit;

namespace Tests.Core.Unit
{
	public class OnDataLostDataLoaderStrategyTests
	{
		[Fact]
		public async Task NotDisposePreviousData_When_FirstLoad()
		{
			// Arrange
			var methodCount = 0;
			Action<object> mockAction = data => methodCount++;

			var sut = new OnDataLostDataLoaderStrategy(mockAction);
			sut.InnerStrategy = new MockDelegatingDataLoaderStrategy(() => Task.FromResult(new object()));

			// Act
			await sut.Load(CancellationToken.None, new TestRequest(0));

			// Assert
			methodCount.Should().Be(0);
		}

		[Fact]
		public async Task Throw_When_RequestIsNull()
		{
			// Arrange
			var sut = new OnDataLostDataLoaderStrategy(_ => { });
			sut.InnerStrategy = new MockDelegatingDataLoaderStrategy(() => Task.FromResult(new object()));

			// Act & Assert
			await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Load(CancellationToken.None, request: null));
		}

		[Theory]
		[ClassData(typeof(OnDataLostDataLoaderStrategyTestData))]
		public async Task DisposePreviousData_If_LoadReturnsNewReference(object firstLoad, object secondLoad, int expectedMethodCount)
		{
			// Arrange
			var methodCount = 0;
			var isFirstLoad = true;
			Action<object> mockAction = data => methodCount++;

			var sut = new OnDataLostDataLoaderStrategy(mockAction);
			sut.InnerStrategy = new MockDelegatingDataLoaderStrategy(() =>
			{
				var result = isFirstLoad ? firstLoad : secondLoad;
				return Task.FromResult(result);
			});
			await sut.Load(CancellationToken.None, new TestRequest(0));
			isFirstLoad = false;

			// Act
			await sut.Load(CancellationToken.None, new TestRequest(1));

			// Assert
			methodCount.Should().Be(expectedMethodCount);
		}

		[Fact]
		public async Task DisposeLatestData_When_Request2FinishesBeforeRequest1()
		{
			// Arrange
			var lostItem = default(object);
			var isFirstLoad = true;
			Action<object> mockAction = data => lostItem = data;
			var tcs = new TaskCompletionSource<bool>();

			var sut = new OnDataLostDataLoaderStrategy(mockAction);
			sut.InnerStrategy = new MockDelegatingDataLoaderStrategy(async () =>
			{
				var result = isFirstLoad ? 1 : 2;
				if (isFirstLoad)
				{
					await tcs.Task;
				}
				return result;
			});

			// Act

			// Start 2 concurrent loads.
			var task1 = sut.Load(CancellationToken.None, new TestRequest(0));
			isFirstLoad = false;
			var task2 = sut.Load(CancellationToken.None, new TestRequest(1));

			// Make it so that the first load finishes after the second load.
			await task2;
			tcs.SetResult(true);
			await task1;

			// Assert

			// The lost item needs to be 1 (the value associated with the first load) even if it finished last (because the second load deprecates the first one).
			lostItem.Should().Be(1);
		}

		public class MockDelegatingDataLoaderStrategy : DelegatingDataLoaderStrategy
		{
			private Func<Task<object>> _innerFunc;

			public MockDelegatingDataLoaderStrategy(Func<Task<object>> innerFunc)
			{
				_innerFunc = innerFunc;
			}

			public override async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return await _innerFunc();
			}
		}

		public class OnDataLostDataLoaderStrategyTestData : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				var list = new List<object[]>();

				var value = new Random().Next(int.MaxValue);
				var instance1 = new object();
				var instance2 = new object();

				list.Add(new object[] { instance1, instance2, 1 }); // Different reference, will call dispose.
				list.Add(new object[] { instance1, instance1, 0 }); // Same reference, won't call dispose.
				list.Add(new object[] { value, value, 1 }); // Same value, but will still call dispose.

				return list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public class TestRequest : IDataLoaderRequest
		{
			public TestRequest(int sequenceId)
			{
				SequenceId = sequenceId;
			}

			public int SequenceId { get; }

			public IDataLoaderTrigger SourceTrigger { get; }

			public IDataLoaderContext Context { get; }
		}
	}
}
