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
	public class WithTriggerTests
	{
		[Fact]
		public async Task WithTrigger_should_type_the_DataLoader_parameter_correctly()
		{
			var addedTrigger = false;
			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName("Test")
				.WithLoadMethod(async (ct, request) => "data")
				.WithTrigger(typedDataLoader =>
				{
					typedDataLoader.Should().NotBeNull();
					typedDataLoader.Should().BeAssignableTo<IDataLoader<string>>();
					addedTrigger = true;
					return new ManualDataLoaderTrigger();
				})
				.Build();

			addedTrigger.Should().BeTrue();
		}
	}
}
