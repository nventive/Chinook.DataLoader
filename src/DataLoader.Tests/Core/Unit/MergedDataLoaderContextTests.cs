using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chinook.DataLoader;
using FluentAssertions;
using Xunit;

namespace Tests.Core.Unit
{
	public class MergedDataLoaderContextTests
	{
		[Fact]
		public void Contains_pairs_from_both_sources()
		{
			// Arrange
			var triggerValues = new Dictionary<string, object>()
			{
				{ "K1", "V1"},
				{ "K2", "V2"},
			};
			var loaderValues = new Dictionary<string, object>()
			{
				{ "K3", "V3"},
				{ "K4", "V4"},
			};
			var context = new MergedDataLoaderContext(triggerValues, loaderValues);

			context.Count.Should().Be(4);
			context.Keys.Should().BeEquivalentTo(new[] { "K1", "K2", "K3", "K4" });
			context.Values.Should().BeEquivalentTo(new[] { "V1", "V2", "V3", "V4" });
		}

		[Fact]
		public void Clear_should_only_remove_items_from_loaderValues()
		{
			// Arrange
			var triggerValues = new Dictionary<string, object>()
			{
				{ "K1", "V1"},
				{ "K2", "V2"},
			};
			var loaderValues = new Dictionary<string, object>()
			{
				{ "K3", "V3"},
				{ "K4", "V4"},
			};
			var context = new MergedDataLoaderContext(triggerValues, loaderValues);

			// Act
			context.Clear();

			// Assert
			context.Count.Should().Be(2);
			context.Keys.Should().BeEquivalentTo(new[] { "K1", "K2" });
			context.Values.Should().BeEquivalentTo(new[] { "V1", "V2" });
			loaderValues.Should().BeEmpty();
		}

		[Fact]
		public void Remove_should_only_remove_items_from_loaderValues()
		{
			// Arrange
			var triggerValues = new Dictionary<string, object>()
			{
				{ "K1", "V1"},
				{ "K2", "V2"},
			};
			var loaderValues = new Dictionary<string, object>()
			{
				{ "K3", "V3"},
				{ "K4", "V4"},
			};
			var context = new MergedDataLoaderContext(triggerValues, loaderValues);

			// Act
			context.Remove("K1");
			context.Remove("K3");

			// Assert
			context.Count.Should().Be(3);
			context.Keys.Should().BeEquivalentTo(new[] { "K1", "K2", "K4" });
			context.Values.Should().BeEquivalentTo(new[] { "V1", "V2", "V4" });
			loaderValues.Should().NotContainKey("K3");
		}

		[Fact]
		public void Set_should_change_loaderValues()
		{
			// Arrange
			var triggerValues = new Dictionary<string, object>()
			{
				{ "K1", "V1"},
				{ "K2", "V2"},
			};
			var loaderValues = new Dictionary<string, object>()
			{
				{ "K3", "V3"},
				{ "K4", "V4"},
			};
			var context = new MergedDataLoaderContext(triggerValues, loaderValues);

			// Act
			context["K5"] = "V5";

			// Assert
			context.Count.Should().Be(5);
			context.Keys.Should().BeEquivalentTo(new[] { "K1", "K2", "K3", "K4", "K5" });
			context.Values.Should().BeEquivalentTo(new[] { "V1", "V2", "V3", "V4", "V5" });
			loaderValues.Should().Contain(new KeyValuePair<string, object>("K5", "V5"));
		}
	}
}
