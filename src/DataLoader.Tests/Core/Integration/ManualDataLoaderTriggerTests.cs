using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Chinook.DataLoader.Tests.Core.Integration
{
	public class ManualDataLoaderTriggerTests
	{
		[Fact]
		public async Task It_loads_when_triggered()
		{
			var expectedResult = "data";
			var loadCount = 0;
			var trigger = new ManualDataLoaderTrigger();

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads_when_triggered))
				.WithLoadMethod(async (ct, request) =>
				{
					loadCount++;

					return expectedResult;
				})
				.TriggerManually(trigger)
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken);

			loadCount.Should().Be(1);
			result.Should().Be(expectedResult);

			trigger.Trigger();

			loadCount.Should().Be(2);
			result.Should().Be(expectedResult);

			trigger.Trigger();

			loadCount.Should().Be(3);
			result.Should().Be(expectedResult);
		}

		[Fact]
		public async Task It_loads_when_triggered_with_context()
		{
			var expectedResult = "data";
			var loadCount = 0;
			var trigger = new ManualDataLoaderTrigger();

			var context = default(IDataLoaderContext);

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads_when_triggered_with_context))
				.WithLoadMethod(async (ct, request) =>
				{
					context = request.Context;

					loadCount++;

					return expectedResult;
				})
				.TriggerManually(trigger)
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken);

			loadCount.Should().Be(1);
			result.Should().Be(expectedResult);

			var expectedContext1 = new DataLoaderContext();
			trigger.Trigger(expectedContext1);

			loadCount.Should().Be(2);
			result.Should().Be(expectedResult);
			context.Should().BeEquivalentTo(expectedContext1);

			var expectedContext2 = new DataLoaderContext();
			trigger.Trigger(expectedContext2);

			loadCount.Should().Be(3);
			result.Should().Be(expectedResult);
			context.Should().BeEquivalentTo(expectedContext2);
		}
	}
}
