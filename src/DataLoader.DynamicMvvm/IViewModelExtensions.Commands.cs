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
	public static partial class DataLoaderViewModelExtensions
	{
		/// <summary>
		/// Gets or creates a <see cref="IDynamicCommand"/> that will refresh
		/// the specified <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <param name="dataLoader">The <see cref="IDataLoader"/> to refresh.</param>
		/// <param name="name">The command name.</param>
		/// <param name="configure">The optional func to configure the command builder.</param>
		/// <returns>The <see cref="IDynamicCommand"/>.</returns>
		public static IDynamicCommand GetCommandFromDataLoaderRefresh(
			this IViewModel viewModel,
			IDataLoader dataLoader,
			Func<IDynamicCommandBuilder, IDynamicCommandBuilder> configure = null,
			[CallerMemberName] string name = null
		) => viewModel.GetOrCreateCommand(name, n => viewModel.GetDynamicCommandBuilderFactory().CreateFromTask(n, ct => RefreshDataLoader(ct, dataLoader), viewModel), configure);

		private static async Task RefreshDataLoader(CancellationToken ct, IDataLoader dataLoader)
		{
			var context = new DataLoaderContext();

			context["IsForceRefreshing"] = true;

			await dataLoader.Load(ct, context);
		}

		/// <summary>
		/// Gets the <see cref="IDynamicCommandBuilderFactory"/> for the <paramref name="viewModel"/>.
		/// </summary>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <returns>The <see cref="IDynamicCommandBuilderFactory"/>.</returns>
		private static IDynamicCommandBuilderFactory GetDynamicCommandBuilderFactory(this IViewModel viewModel)
			=> viewModel.ServiceProvider.GetRequiredService<IDynamicCommandBuilderFactory>();
	}
}
