using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Chinook.DataLoader;
using Chinook.DynamicMvvm;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;
using Xunit.Sdk;
using FluentAssertions;

namespace Tests.DynamicMvvm.Integration
{
	public class ViewModelTests
	{
		private readonly IServiceProvider _serviceProvider;

		public ViewModelTests()
		{
			var serviceCollection = new ServiceCollection();

			serviceCollection.AddSingleton<IDataLoaderBuilderFactory, DataLoaderBuilderFactory>();
			serviceCollection.AddSingleton<IDynamicCommandBuilderFactory, DynamicCommandBuilderFactory>();

			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		[Fact]
		public void Names_are_mapped_correctly()
		{
			var vm = new MyViewModel(_serviceProvider);

			Assert.Equal("MySuperTitles", vm.Titles.Name);
			Assert.Equal(nameof(MyViewModel.Subtitles), vm.Subtitles.Name);
		}

		[Fact]
		public void Extensions_return_null_when_VM_is_disposed()
		{
			var vm = new MyViewModel(_serviceProvider);

			vm.Titles.Should().NotBeNull();
			vm.RefreshTitles.Should().NotBeNull();

			vm.Dispose();

			vm.Titles.Should().BeNull();
			vm.RefreshTitles.Should().BeNull();
		}

		private class MyViewModel : ViewModelBase
		{
			public MyViewModel(IServiceProvider serviceProvider) : base(serviceProvider: serviceProvider)
			{
			}

			public IDataLoader Titles => this.GetDataLoader(GetTitles, b => b
				.OnBackgroundThread()
				.WithName("MySuperTitles")
				.WithEmptySelector(d => !(d.Data?.Any() ?? false))
			);

			private async Task<string[]> GetTitles(CancellationToken ct)
			{
				return new string[]
				{
					"Hello",
					"World"
				};
			}

			public IDynamicCommand RefreshTitles => this.GetCommandFromDataLoaderRefresh(Titles);

			public IDataLoader Subtitles => this.GetDataLoader(GetSubtitles);

			private async Task<string[]> GetSubtitles(CancellationToken ct)
			{
				return null;
			}
		}
	}
}
