﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Chinook.DataLoader;

namespace Chinook.DynamicMvvm
{
	/// <summary>
	/// Extensions on <see cref="IViewModel"/> to attach <see cref="IDataLoader"/>.
	/// </summary>
	public static partial class IViewModelExtensions
	{
		/// <summary>
		/// Gets or creates a <see cref="IDataLoader"/> from a task.
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <param name="source">The task source.</param>
		/// <param name="configure">The optional configuration.</param>
		/// <param name="name">The optional name of the <see cref="IDataLoader"/>.</param>
		/// <returns>The <see cref="IDataLoader"/>.</returns>
		public static IDataLoader<TData> GetDataLoader<TData>(
			this IViewModel viewModel,
			Func<CancellationToken, Task<TData>> source,
			Func<IDataLoaderBuilder<TData>, IDataLoaderBuilder<TData>> configure = null,
			[CallerMemberName] string name = null
		) => viewModel.GetDataLoader((ct, _) => source(ct), configure, name);

		/// <summary>
		/// Gets or creates a <see cref="IDataLoader"/> from a task.
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <param name="source">The task source.</param>
		/// <param name="configure">The optional configuration.</param>
		/// <param name="name">The optional name of the <see cref="IDataLoader"/>.</param>
		/// <returns>The <see cref="IDataLoader"/>. Null is returned if the <see cref="IViewModel"/> is disposed.</returns>
		public static IDataLoader<TData> GetDataLoader<TData>(
			this IViewModel viewModel,
			DataLoaderDelegate<TData> source,
			Func<IDataLoaderBuilder<TData>, IDataLoaderBuilder<TData>> configure = null,
			[CallerMemberName] string name = null
		)
		{
			if (viewModel.IsDisposed)
			{
				return null;
			}

			return (IDataLoader<TData>)viewModel.GetOrCreateDataLoader(name, n =>
			{
				var builder = viewModel.GetDataLoaderBuilderFactory()
					.CreateTyped<TData>()
					.WithName(name)
					.WithLoadMethod(source);

				if (configure != null)
				{
					builder = configure(builder);
				}

				return builder.Build();
			});
		}

		/// <summary>
		/// Gets or creates a <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <param name="key">The key holding the <see cref="IDataLoader"/> in the containing <see cref="IViewModel"/>. This also serves as the default name of the <see cref="IDataLoader"/>.</param>
		/// <param name="factory">The <see cref="IDataLoader"/> Factory.</param>
		/// <returns>The <see cref="IDataLoader"/>. Null is returned if the <see cref="IViewModel"/> is disposed.</returns>
		public static IDataLoader GetOrCreateDataLoader(
			this IViewModel viewModel,
			string key,
			Func<string, IDataLoader> factory
		)
		{
			if (viewModel.IsDisposed)
			{
				return null;
			}

			if (!viewModel.TryGetDisposable<IDataLoader>(key, out var dataLoader))
			{
				dataLoader = factory(key);

				viewModel.AddDisposable(key, dataLoader);
			}

			return dataLoader;
		}

		/// <summary>
		/// Gets the <see cref="IDataLoaderBuilderFactory"/> for the <paramref name="viewModel"/>.
		/// </summary>
		/// <param name="viewModel">The <see cref="IViewModel"/>.</param>
		/// <returns>The <see cref="IDataLoaderBuilderFactory"/>.</returns>
		private static IDataLoaderBuilderFactory GetDataLoaderBuilderFactory(this IViewModel viewModel)
			=> viewModel.ServiceProvider.GetRequiredService<IDataLoaderBuilderFactory>();
	}
}
