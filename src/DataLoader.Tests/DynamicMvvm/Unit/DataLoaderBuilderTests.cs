using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chinook.DataLoader;
using Chinook.DynamicMvvm;
using Xunit;

namespace Tests.DynamicMvvm.Unit
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

			builder = builder.TriggerOnValueChanged(new DynamicProperty("MyProperty"));
			builder = builder.TriggerOnCommandExecution(new DynamicCommand("MyCommand", new ActionCommandStrategy(() => { })));

			Assert.NotNull(builder);
		}

		/// <summary>
		/// This is a "compilation test" that ensures that extensions on <see cref="IDataLoaderBuilder{TData}"/> returns an object of type <see cref="IDataLoaderBuilder{TData}"/>.
		/// </summary>
		[Fact]
		public void DynamicMvvm_extensions_work_for_typed_version()
		{
			IDataLoaderBuilder<string> builder = new DataLoaderBuilder<string>();

			builder = builder.TriggerOnValueChanged(new DynamicProperty("MyProperty"));
			builder = builder.TriggerOnCommandExecution(new DynamicCommand("MyCommand", new ActionCommandStrategy(() => { })));

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
				.TriggerOnValueChanged(new DynamicProperty("MyProperty"))
				.TriggerOnCommandExecution(new DynamicCommand("MyCommand", new ActionCommandStrategy(() => { })));

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
				.TriggerOnValueChanged(new DynamicProperty("MyProperty"))
				.TriggerOnCommandExecution(new DynamicCommand("MyCommand", new ActionCommandStrategy(() => { })));

			Assert.NotNull(builder);
		}
	}
}
