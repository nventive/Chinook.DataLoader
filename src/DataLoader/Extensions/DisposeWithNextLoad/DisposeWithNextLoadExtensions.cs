using System;
using System.Collections.Generic;
using System.Text;
using Chinook.DataLoader.DisposeWithNextLoad;

namespace Chinook.DataLoader
{
	public static class DisposeWithNextLoadExtensions
	{
		/// <summary>
		/// The key used to get or add the <see cref="SequenceDisposableGroup"/> in a <see cref="IDataLoaderContext"/>.
		/// </summary>
		public const string DisposeWithNextLoadKey = "DisposeWithNextLoad";

		/// <summary>
		/// Adds a <paramref name="disposable"/> to this <paramref name="request"/>'s <see cref="SequenceDisposableGroup"/>.
		/// </summary>
		/// <param name="request">The request that generated the <paramref name="disposable"/>.</param>
		/// <param name="disposable">The disposable to add.</param>
		public static void AddToSequenceDisposableGroup(this IDataLoaderRequest request, IDisposable disposable)
		{
			var contextLifetime = request.Context.GetSequenceDisposableGroup();
			contextLifetime.AddOrClear(request.SequenceId, disposable);
		}

		/// <summary>
		/// Gets the <see cref="SequenceDisposableGroup"/> of this <paramref name="context"/>.
		/// If none exists, it's automatically created and added.
		/// </summary>
		/// <param name="context">The context from which to retrieve the <see cref="SequenceDisposableGroup"/> object.</param>
		/// <returns>The <see cref="SequenceDisposableGroup"/> of this <paramref name="context"/>.</returns>
		public static SequenceDisposableGroup GetSequenceDisposableGroup(this IDataLoaderContext context)
		{
			if (context.TryGetValue(DisposeWithNextLoadKey, out var lifetime))
			{
				return (SequenceDisposableGroup)lifetime;
			}
			else
			{
				var contextLifetime = new SequenceDisposableGroup();
				context.Add(DisposeWithNextLoadKey, contextLifetime);
				return contextLifetime;
			}
		}

		/// <summary>
		/// Adds this <paramref name="disposable"/> to the <see cref="IDataLoaderRequest"/>'s <see cref="SequenceDisposableGroup"/>.
		/// </summary>
		/// <param name="disposable">The disposable to add.</param>
		/// <param name="request">The request that generated the <paramref name="disposable"/>.</param>
		/// <returns>The original <paramref name="disposable"/>.</returns>
		public static IDisposable DisposeWithNextLoad(this IDisposable disposable, IDataLoaderRequest request)
		{
			request.AddToSequenceDisposableGroup(disposable);
			return disposable;
		}
	}
}
