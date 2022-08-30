using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chinook.DataLoader;
using Chinook.DynamicMvvm;
using FluentAssertions;
using Xunit;

namespace Tests.DynamicMvvm.Unit
{
	public class DynamicPropertyDataLoaderTriggerTests
	{
		[Fact]
		public void Triggers_when_Value_changes()
		{
			// Arrange
			var wasTriggered = false;
			var property = new TestProperty();
			var trigger = new DynamicPropertyDataLoaderTrigger(property);

			trigger.LoadRequested += Trigger_LoadRequested;

			// Act
			property.Value = "test";

			// Assert
			wasTriggered.Should().BeTrue();

			void Trigger_LoadRequested(IDataLoaderTrigger dlt, IDataLoaderContext context)
			{
				dlt.Should().BeSameAs(trigger);
				wasTriggered = true;
			}
		}

		private class TestProperty : IDynamicProperty
		{
			private object _value;

			public string Name => "TestProperty";

			public object Value
			{
				get => _value;
				set
				{
					_value = value;
					ValueChanged?.Invoke(this);
				}
			}

			public event DynamicPropertyChangedEventHandler ValueChanged;

			public void Dispose()
			{
				ValueChanged = null;
			}
		}
	}
}
