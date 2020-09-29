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
	public class DataLoaderTests
	{
		[Fact]
		public async Task It_loads()
		{
			var expectedResult = "data";

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads))
				.WithLoadMethod(async (ct, request) => expectedResult)
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken);

			result.Should().Be(expectedResult);
		}

		[Fact]
		public async Task It_loads_with_context()
		{
			var expectedResult = "data";
			var expectedContext = new DataLoaderContext();
			expectedContext["key1"] = "value1";

			var context = default(IDataLoaderContext);

			var dataLoader = DataLoaderBuilder<string>
				.Empty
				.WithName(nameof(It_loads_with_context))
				.WithLoadMethod(async (ct, request) =>
				{
					context = request.Context;

					return expectedResult;
				})
				.Build();

			var cancellationToken = CancellationToken.None;

			var result = await dataLoader.Load(cancellationToken, expectedContext);

			result.Should().Be(expectedResult);
			context.Should().BeEquivalentTo(expectedContext);
		}
	}
}
