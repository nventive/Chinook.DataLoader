using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader.SubscribeToData
{
	/// <summary>
	/// This is <see cref="IDataLoaderStrategy"/> implementation for the SubscribeToData recipe.
	/// It works in pair with <see cref="SubscribeToDataTrigger"/>.
	/// </summary>
	public class SubscribeToDataStrategy : DelegatingDataLoaderStrategy
	{
		private readonly Func<object, IDisposable> _subscribeToData;

		private IDisposable _subscription;

		/// <summary>
		/// Creates a new instance of <see cref="SubscribeToDataStrategy"/>.
		/// </summary>
		/// <param name="subscribeToData">The subscription function.</param>
		public SubscribeToDataStrategy(Func<object, IDisposable> subscribeToData)
		{
			_subscribeToData = subscribeToData;
		}

		/// <inheritdoc/>
		public override async Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
		{
			var value = request.Context[SubscribeToDataTrigger.DataContextKey];
			if (value != null)
			{
				return value;
			}
			else if (request.Context[SubscribeToDataTrigger.ErrorContextKey] is Exception error)
			{
				throw error;
			}
			else
			{
				if (_subscription != null)
				{
					_subscription.Dispose();
				}

				var data = await base.Load(ct, request);

				_subscription = _subscribeToData(data);

				return data;
			}
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();

			_subscription?.Dispose();
		}
	}
}
