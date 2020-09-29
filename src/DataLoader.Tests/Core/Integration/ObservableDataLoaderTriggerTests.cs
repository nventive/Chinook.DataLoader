using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Chinook.DataLoader.Tests.Core.Integration
{
	public class ObservableDataLoaderTriggerTests
	{
		[Fact]
		public async Task It_loads_when_triggered()
		{
			var expectedResult = "data";
			var loadCount = 0;
			var observable = new Subject<object>();

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads_when_triggered))
				.WithLoadMethod(async (ct, request) =>
				{
					loadCount++;

					return expectedResult;
				})
				.TriggerFromObservable(observable)
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken);

			loadCount.Should().Be(1);
			result.Should().Be(expectedResult);

			observable.OnNext(new object());

			loadCount.Should().Be(2);
			result.Should().Be(expectedResult);

			observable.OnNext(new object());

			loadCount.Should().Be(3);
			result.Should().Be(expectedResult);
		}

		[Fact]
		public async Task It_loads_when_triggered_with_context()
		{
			var expectedResult = "data";
			var loadCount = 0;
			var observable = new Subject<int>();

			var context = default(IDataLoaderContext);

			var expectedContext1 = new DataLoaderContext();
			var expectedContext2 = new DataLoaderContext();

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads_when_triggered_with_context))
				.WithLoadMethod(async (ct, request) =>
				{
					context = request.Context;

					loadCount++;

					return expectedResult;
				})
				.TriggerFromObservable(observable, contextSelector: i => i == 100 ? expectedContext1 : expectedContext2)
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken);

			loadCount.Should().Be(1);
			result.Should().Be(expectedResult);

			observable.OnNext(100);

			loadCount.Should().Be(2);
			result.Should().Be(expectedResult);
			context.Should().BeEquivalentTo(expectedContext1);

			observable.OnNext(200);

			loadCount.Should().Be(3);
			result.Should().Be(expectedResult);
			context.Should().BeEquivalentTo(expectedContext2);
		}
	}
}
