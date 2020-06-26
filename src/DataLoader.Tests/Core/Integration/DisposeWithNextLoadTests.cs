using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader;
using Xunit;

namespace Tests.Core.Integration
{
	public class DisposeWithNextLoadTests
	{
		[Fact]
		public async Task DisposeWithNextLoad_works_with_1_item()
		{
			var list = new List<TestDisposable>();
			var dataLoader = GetDataLoader(Load);

			await dataLoader.Load(CancellationToken.None);

			Assert.Single(list);
			Assert.False(list[0].IsDisposed);

			await dataLoader.Load(CancellationToken.None);

			Assert.Equal(2, list.Count);
			Assert.True(list[0].IsDisposed);
			Assert.False(list[1].IsDisposed);

			async Task<string> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				var disposable = new TestDisposable()
					.DisposeWithNextLoad(request);

				list.Add((TestDisposable)disposable);

				return string.Empty;
			}
		}

		[Fact]
		public async Task DisposeWithNextLoad_works_with_multiple_items()
		{
			var list = new List<TestDisposable>();
			var dataLoader = GetDataLoader(Load);

			await dataLoader.Load(CancellationToken.None);

			Assert.Equal(2, list.Count);
			Assert.False(list[0].IsDisposed);
			Assert.False(list[1].IsDisposed);

			await dataLoader.Load(CancellationToken.None);

			Assert.Equal(4, list.Count);
			Assert.True(list[0].IsDisposed);
			Assert.True(list[1].IsDisposed);
			Assert.False(list[2].IsDisposed);
			Assert.False(list[3].IsDisposed);

			async Task<string> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				list.Add((TestDisposable)new TestDisposable().DisposeWithNextLoad(request));
				list.Add((TestDisposable)new TestDisposable().DisposeWithNextLoad(request));

				return string.Empty;
			}
		}

		[Fact]
		public async Task Items_are_disposed_with_the_dataloader()
		{
			var list = new List<TestDisposable>();
			var dataLoader = GetDataLoader(Load);

			await dataLoader.Load(CancellationToken.None);

			Assert.Equal(2, list.Count);
			Assert.False(list[0].IsDisposed);
			Assert.False(list[1].IsDisposed);

			dataLoader.Dispose();

			Assert.True(list[0].IsDisposed);
			Assert.True(list[1].IsDisposed);

			async Task<string> Load(CancellationToken ct, IDataLoaderRequest request)
			{
				list.Add((TestDisposable)new TestDisposable().DisposeWithNextLoad(request));
				list.Add((TestDisposable)new TestDisposable().DisposeWithNextLoad(request));

				return string.Empty;
			}
		}

		private IDataLoader<string> GetDataLoader(DataLoaderDelegate<string> loadMethod)
		{
			return new DataLoaderBuilder<string>()
				.WithName("Loader")
				.WithLoadMethod(loadMethod)
				.Build();
		}

		private class TestDisposable : IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}
	}
}
