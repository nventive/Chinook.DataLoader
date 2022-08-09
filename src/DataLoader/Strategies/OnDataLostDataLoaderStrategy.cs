using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This class is a <see cref="DelegatingDataLoaderStrategy"/> that offers a callback when the previous data reference is lost when getting new data.
	/// </summary>
	public class OnDataLostDataLoaderStrategy : DelegatingDataLoaderStrategy
	{
		private readonly Action<object> _onPreviousDataLost;
		private object _data;
		private int _lastRequestId;

		/// <summary>
		/// Creates a new instance of <see cref="OnDataLostDataLoaderStrategy"/>.
		/// </summary>
		/// <param name="onPreviousDataLost">The action to invoke when the previous data is lost.</param>
		public OnDataLostDataLoaderStrategy(Action<object> onPreviousDataLost)
		{
			_onPreviousDataLost = onPreviousDataLost;
		}

		/// <inheritdoc/>
		public override async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request), "OnDataLostDataLoaderStrategy requires a non-null IDataLoaderRequest parameter in order to read its SequenceId.");
			}

			_lastRequestId = request.SequenceId;

			var result = await base.Load(ct, request);

			// Check whether the result we get is the latest one.
			if (request.SequenceId != _lastRequestId)
			{
				// If the result isn't the latest, we consider it lost immediately.
				// This can happen when the DataLoader loads 2 requests concurrently in a way that the second request finishes before the first request.
				_onPreviousDataLost(result);
				return _data;
			}

			// We should not lose the previous data if we load the same instance.
			if (_data != null && !ReferenceEquals(_data, result))
			{
				_onPreviousDataLost(_data);
			}
			_data = result;

			return result;
		}
	}

	/// <summary>
	/// This class exposes extensions methods related to <see cref="OnDataLostDataLoaderStrategy"/>.
	/// </summary>
	public static class OnDataLostDataLoaderStrategyExtensions
	{
		/// <summary>
		/// Adds a <see cref="OnDataLostDataLoaderStrategy"/> to this builder that will dispose the previous data when new data is received.
		/// </summary>
		/// <typeparam name="TBuilder">The type of the builder.</typeparam>
		/// <param name="builder">The builder.</param>
		/// <returns>The original builder.</returns>
		public static TBuilder DisposePreviousData<TBuilder>(this TBuilder builder)
			where TBuilder : IDataLoaderBuilder
		{
			return builder.WithStrategy(new OnDataLostDataLoaderStrategy(DisposeData));

			void DisposeData(object data)
			{
				if (data is IDisposable disposable)
				{
					disposable.Dispose();
				}
				else if (data is IEnumerable enumerable)
				{
					foreach (var item in enumerable)
					{
						if (item is IDisposable disposableItem)
						{
							disposableItem.Dispose();
						}
					}
				}
			}
		}
	}
}
