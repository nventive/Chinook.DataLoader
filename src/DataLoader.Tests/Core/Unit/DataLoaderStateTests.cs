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
	public class DataLoaderStateTests
	{
		[Fact]
		public void Generic_state_handles_null_data_with_value_types()
		{
			var stateWithNullData = DataLoaderState.Default;

			IDataLoaderState<int> state = new DataLoaderState<int>(stateWithNullData);
			state.Data.Should().Be(default(int));
		}

		[Fact]
		public void Generic_state_handles_null_data_with_nullable_value_types()
		{
			var stateWithNullData = DataLoaderState.Default;

			IDataLoaderState<int?> state = new DataLoaderState<int?>(stateWithNullData);
			state.Data.Should().Be(null);
		}

		[Fact]
		public void Generic_state_handles_null_data_with_object_types()
		{
			var stateWithNullData = DataLoaderState.Default;

			IDataLoaderState<string> state = new DataLoaderState<string>(stateWithNullData);
			state.Data.Should().Be(null);
		}

		[Fact]
		public void Generic_state_handles_non_null_data_with_value_types()
		{
			var stateWithNullData = DataLoaderState.Default.WithData(1);

			IDataLoaderState<int> state = new DataLoaderState<int>(stateWithNullData);
			state.Data.Should().Be(1);
		}

		[Fact]
		public void Generic_state_handles_non_null_data_with_nullable_value_types()
		{
			var stateWithNullData = DataLoaderState.Default.WithData(1);

			IDataLoaderState<int?> state = new DataLoaderState<int?>(stateWithNullData);
			state.Data.Should().Be(1);
		}

		[Fact]
		public void Generic_state_handles_non_null_data_with_object_types()
		{
			var stateWithNullData = DataLoaderState.Default.WithData("hello");

			IDataLoaderState<string> state = new DataLoaderState<string>(stateWithNullData);
			state.Data.Should().Be("hello");
		}
	}
}
