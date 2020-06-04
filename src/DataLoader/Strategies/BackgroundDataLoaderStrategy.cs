using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	public static partial class BackgroundDataLoaderStrategyExtensions
	{
		/// <summary>
		/// Loads the data on a background thread.
		/// </summary>
		/// <param name="dataLoaderBuilder"><see cref="IDataLoaderBuilder"/></param>
		/// <returns><see cref="IDataLoaderBuilder"/></returns>
		public static TBuilder OnBackgroundThread<TBuilder>(this TBuilder dataLoaderBuilder) where TBuilder : IDataLoaderBuilder
			=> dataLoaderBuilder.WithStrategy(new BackgroundDataLoaderStrategy());
	}

	/// <summary>
	/// This <see cref="IDataLoaderStrategy"/> loads the data on a background thread.
	/// <see cref="Task.Run(Func{Task}, CancellationToken)"/>
	/// </summary>
	public class BackgroundDataLoaderStrategy : DelegatingDataLoaderStrategy
	{
		/// <inheritdoc />
		public override Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
		{
			return Task.Run(InnerLoad);

			Task<object> InnerLoad()
			{
				return base.Load(ct, request);
			}
		}
	}
}
