using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Chinook.DataLoader;

namespace Chinook.DynamicMvvm
{
	/// <summary>
	/// Extensions on <see cref="IViewModel"/> to create commands for <see cref="IDataLoader"/>.
	/// </summary>
	public static partial class IViewModelExtensions
	{
		/// <summary>
		/// Gets or creates a <see cref="IDynamicCommand"/> that will refresh
		/// the specified <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="viewModel"><see cref="IViewModel"/></param>
		/// <param name="dataLoader"><see cref="IDataLoader"/> to refresh</param>
		/// <param name="name">Command name</param>
		/// <param name="decorator">Command decorator</param>
		/// <returns><see cref="IDynamicCommand"/></returns>
		public static IDynamicCommand GetCommandFromDataLoaderRefresh(
			this IViewModel viewModel,
			IDataLoader dataLoader,
			Func<IDynamicCommandStrategy, IDynamicCommandStrategy> decorator = null,
			[CallerMemberName] string name = null
		) => viewModel.GetOrCreateCommand(name, n => viewModel.GetDynamicCommandFactory().CreateFromTask(n, ct => RefreshDataLoader(ct, dataLoader), new DynamicCommandStrategyDecorator(decorator)));

		private static async Task RefreshDataLoader(CancellationToken ct, IDataLoader dataLoader)
		{
			var context = new DataLoaderContext();

			context["IsForceRefreshing"] = true;

			await dataLoader.Load(ct, context);
		}

		/// <summary>
		/// Gets the <see cref="IDynamicCommandFactory"/> for the <paramref name="viewModel"/>.
		/// </summary>
		/// <param name="viewModel">ViewModel</param>
		/// <returns>IDynamicCommandFactory</returns>
		private static IDynamicCommandFactory GetDynamicCommandFactory(this IViewModel viewModel)
			=> viewModel.ServiceProvider.GetRequiredService<IDynamicCommandFactory>();
	}
}
