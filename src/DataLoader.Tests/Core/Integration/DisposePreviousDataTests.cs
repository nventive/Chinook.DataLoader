using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader;
using FluentAssertions;
using Xunit;

namespace Tests.Core.Integration
{
	public class DisposePreviousDataTests
	{
		[Fact]
		public async Task Doesnt_dispose_if_same_instance()
		{
			// Arrange
			var disposable = new TestDisposable();
			var dataLoader = GetDataLoader(Load);

			await dataLoader.Load(CancellationToken.None);

			disposable.IsDisposed.Should().BeFalse();

			// Act			
			await dataLoader.Load(CancellationToken.None);

			// Assert
			disposable.IsDisposed.Should().BeFalse();

			async Task<TestDisposable> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return disposable;
			}
		}

		[Fact]
		public async Task Disposes_when_type_implements_IDisposable()
		{
			// Arrange
			var dataLoader = GetDataLoader(Load).AsOf<TestDisposable>();
			var result1 = await dataLoader.Load(CancellationToken.None);

			result1.IsDisposed.Should().BeFalse();

			// Act			
			var result2 = await dataLoader.Load(CancellationToken.None);

			// Assert
			result1.IsDisposed.Should().BeTrue();
			result2.IsDisposed.Should().BeFalse();

			async Task<TestDisposable> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return new TestDisposable();
			}
		}

		[Fact]
		public async Task Disposes_when_type_is_enumerable_of_objects_implementing_IDisposable()
		{
			// Arrange
			var dataLoader = GetDataLoader(Load).AsOf<TestDisposable[]>();
			var result1 = await dataLoader.Load(CancellationToken.None);

			result1.Should().AllSatisfy(x => x.IsDisposed.Should().BeFalse());

			// Act			
			var result2 = await dataLoader.Load(CancellationToken.None);

			// Assert
			result1.Should().AllSatisfy(x => x.IsDisposed.Should().BeTrue());
			result2.Should().AllSatisfy(x => x.IsDisposed.Should().BeFalse());

			async Task<TestDisposable[]> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				return new[] { new TestDisposable(), new TestDisposable() };
			}
		}

		private IDataLoader GetDataLoader<T>(DataLoaderDelegate<T> loadMethod)
		{
			return new DataLoaderBuilder<T>()
				.WithName("Loader")
				.WithLoadMethod(loadMethod)
				.DisposePreviousData()
				.Build();
		}		
	}
}
