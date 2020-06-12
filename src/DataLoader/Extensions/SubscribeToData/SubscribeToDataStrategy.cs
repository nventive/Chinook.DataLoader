using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader.SubscribeToData
{
	public class SubscribeToDataStrategy : DelegatingDataLoaderStrategy
	{
		private readonly Func<object, IDisposable> _subscribeToData;

		private IDisposable _subscription;

		public SubscribeToDataStrategy(Func<object, IDisposable> subscribeToData)
		{
			_subscribeToData = subscribeToData;
		}

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

		public override void Dispose()
		{
			base.Dispose();

			_subscription?.Dispose();
		}
	}
}
