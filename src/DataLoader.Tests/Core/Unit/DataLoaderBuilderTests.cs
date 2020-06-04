using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chinook.DataLoader.Tests.Core.Unit
{
	public class DataLoaderBuilderTests
	{
		/// <summary>
		/// This is a "compilation test" that ensures that extensions on <see cref="IDataLoaderBuilder"/> returns an object of type <see cref="IDataLoaderBuilder"/>.
		/// </summary>
		[Fact]
		public void Default_extensions_work_for_untyped_version()
		{
			IDataLoaderBuilder builder = new DataLoaderBuilder();

			builder = builder.WithName("name");
			builder = builder.WithLoadMethod(null);
			builder = builder.WithEmptySelector(null);
			builder = builder.WithConcurrentMode(DataLoaderConcurrentMode.CancelPrevious);
			builder = builder.WithStrategy(null);
			builder = builder.WithTrigger(d => new ManualDataLoaderTrigger());
			builder = builder.WithTrigger(new ManualDataLoaderTrigger());

			Assert.NotNull(builder);
		}

		/// <summary>
		/// This is a "compilation test" that ensures that extensions on <see cref="IDataLoaderBuilder{TData}"/> returns an object of type <see cref="IDataLoaderBuilder{TData}"/>.
		/// </summary>
		[Fact]
		public void Default_extensions_work_for_typed_version()
		{
			IDataLoaderBuilder<string> builder = new DataLoaderBuilder<string>();

			builder = builder.WithName("name");
			builder = builder.WithLoadMethod(null);
			builder = builder.WithEmptySelector<string>(null);
			builder = builder.WithConcurrentMode(DataLoaderConcurrentMode.CancelPrevious);
			builder = builder.WithStrategy(null);
			builder = builder.WithTrigger(d => new ManualDataLoaderTrigger());
			builder = builder.WithTrigger(new ManualDataLoaderTrigger());

			Assert.NotNull(builder);
		}

		/// <summary>
		/// This is a "compilation test" that ensures that extensions on <see cref="IDataLoaderBuilder"/> returns an object of type <see cref="IDataLoaderBuilder"/>.
		/// </summary>
		[Fact]
		public void We_can_create_a_builder_fluently_untyped()
		{
			var builder = DataLoaderBuilder
				.Empty
				.WithName("name")
				.WithLoadMethod(async (ct, request) => "data")
				.WithEmptySelector(state => false)
				.WithConcurrentMode(DataLoaderConcurrentMode.CancelPrevious)
				.WithStrategy(null)
				.WithTrigger(d => new ManualDataLoaderTrigger())
				.WithTrigger(new ManualDataLoaderTrigger());

			Assert.NotNull(builder);
		}

		/// <summary>
		/// This is a "compilation test" that ensures that extensions on <see cref="IDataLoaderBuilder{TData}"/> returns an object of type <see cref="IDataLoaderBuilder{TData}"/>.
		/// </summary>
		[Fact]
		public void We_can_create_a_builder_fluently_typed()
		{
			var builder = DataLoaderBuilder<string>
				.Empty
				.WithName("name")
				.WithLoadMethod(async (ct, request) => "data")
				.WithEmptySelector(state => false)
				.WithConcurrentMode(DataLoaderConcurrentMode.CancelPrevious)
				.WithStrategy(null)
				.WithTrigger(d => new ManualDataLoaderTrigger())
				.WithTrigger(new ManualDataLoaderTrigger());

			Assert.NotNull(builder);
		}
	}
}
