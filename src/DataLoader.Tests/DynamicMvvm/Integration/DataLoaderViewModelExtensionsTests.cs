using Xunit;
using Moq;
using Chinook.DynamicMvvm;
using Chinook.DataLoader;
using FluentAssertions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.DynamicMvvm.Unit
{
	public class DataLoaderViewModelExtensionsTests
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IViewModel _viewModel;

		public DataLoaderViewModelExtensionsTests()
		{
			var serviceCollection = new ServiceCollection();

			serviceCollection.AddSingleton<IDataLoaderBuilderFactory, DataLoaderBuilderFactory>();
			serviceCollection.AddSingleton<IDynamicCommandBuilderFactory, DynamicCommandBuilderFactory>();
			serviceCollection.AddSingleton<IDynamicPropertyFactory, DynamicPropertyFactory>();

			_serviceProvider = serviceCollection.BuildServiceProvider();

			_viewModel = new ViewModelBase("vm", _serviceProvider);
		}

		[Fact]
		public void Should_Create_A_New_DynamicProperty_From_DataLoaderState()
		{
			// Arrange
			var dl = _serviceProvider.GetService<IDataLoaderBuilderFactory>().Create().WithName("dl").Build();

			// Act
			var stateResult = _viewModel.GetFromDataLoaderState(dl, "test");
			var propertyResult = _viewModel.GetProperty("test");

			// Assert
			stateResult.Should().BeAssignableTo<IDataLoaderState>();
			propertyResult.Should().BeAssignableTo<IDynamicProperty<IDataLoaderState>>();
		}

		[Fact]
		public void Should_Create_A_New_DynamicProperty_From_Generic_DataLoaderState()
		{
			// Arrange
			var dl = _serviceProvider.GetService<IDataLoaderBuilderFactory>().CreateTyped<string>().WithName("dl").Build();

			// Act
			var stateResult = _viewModel.GetFromDataLoaderState(dl, "test");
			var propertyResult = _viewModel.GetProperty("test");

			// Assert
			stateResult.Should().BeAssignableTo<IDataLoaderState<string>>();
			propertyResult.Should().BeAssignableTo<IDynamicProperty<IDataLoaderState<string>>>();
		}
	}
}
