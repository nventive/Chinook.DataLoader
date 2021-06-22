using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Chinook.DataLoader;
using Chinook.DynamicMvvm;
using FluentAssertions;
using Xunit;

namespace Tests.DynamicMvvm.Unit
{
	public class DynamicCommandDataLoaderTriggerTests
	{
		[Fact]
		private void Triggers_before_execution()
		{
			var command = new TestCommand();
			var sut = new DynamicCommandDataLoaderTrigger(command, CommandTriggerOptions.TriggerBeforeCommandExecution);
			int loadRequests = 0;

			sut.LoadRequested += OnLoadRequested;

			loadRequests.Should().Be(0);
			command.IsExecuting = true;
			loadRequests.Should().Be(1);
			command.IsExecuting = false;
			loadRequests.Should().Be(1);

			void OnLoadRequested(IDataLoaderTrigger trigger, IDataLoaderContext context)
			{
				++loadRequests;
			}
		}

		[Fact]
		private void Triggers_after_execution()
		{
			var command = new TestCommand();
			var sut = new DynamicCommandDataLoaderTrigger(command, CommandTriggerOptions.TriggerAfterCommandExecution);
			int loadRequests = 0;

			sut.LoadRequested += OnLoadRequested;

			loadRequests.Should().Be(0);
			command.IsExecuting = true;
			loadRequests.Should().Be(0);
			command.IsExecuting = false;
			loadRequests.Should().Be(1);

			void OnLoadRequested(IDataLoaderTrigger trigger, IDataLoaderContext context)
			{
				++loadRequests;
			}
		}

		[Fact]
		private void Triggers_before_and_after_execution()
		{
			var command = new TestCommand();
			var sut = new DynamicCommandDataLoaderTrigger(command, CommandTriggerOptions.TriggerBeforeCommandExecution | CommandTriggerOptions.TriggerAfterCommandExecution);
			int loadRequests = 0;

			sut.LoadRequested += OnLoadRequested;

			loadRequests.Should().Be(0);
			command.IsExecuting = true;
			loadRequests.Should().Be(1);
			command.IsExecuting = false;
			loadRequests.Should().Be(2);

			void OnLoadRequested(IDataLoaderTrigger trigger, IDataLoaderContext context)
			{
				++loadRequests;
			}
		}

		private class TestCommand : IDynamicCommand
		{
			public string Name => "Test";

			private bool _isExecuting;
			public bool IsExecuting
			{
				get => _isExecuting;
				set
				{
					_isExecuting = value;
					IsExecutingChanged?.Invoke(this, null);
				}
			}

			public event EventHandler IsExecutingChanged;
			public event EventHandler CanExecuteChanged;
			public event PropertyChangedEventHandler PropertyChanged;

			public bool CanExecute(object parameter)
			{
				throw new NotImplementedException();
			}

			public void Dispose()
			{
				throw new NotImplementedException();
			}

			public Task Execute(object parameter)
			{
				throw new NotImplementedException();
			}

			void ICommand.Execute(object parameter)
			{
				throw new NotImplementedException();
			}
		}
	}
}
