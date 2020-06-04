using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This class allows to delegate the load strategy to an inner strategy.
	/// This class is an homologue to <see cref="System.Net.Http.DelegatingHandler"/>.
	/// </summary>
	public abstract class DelegatingDataLoaderStrategy : IDataLoaderStrategy
	{
		/// <summary>
		/// Inner <see cref="IDataLoaderStrategy"/>
		/// </summary>
		public IDataLoaderStrategy InnerStrategy { get; set; }

		/// <inheritdoc cref="IDataLoaderStrategy.Load(CancellationToken, IDataLoaderRequest)"/>
		public virtual Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
		{
			return InnerStrategy.Load(ct, request);
		}

		/// <summary>
		/// Disposes the <see cref="InnerStrategy"/>.
		/// </summary>
		public virtual void Dispose()
		{
			InnerStrategy.Dispose();
		}
	}
}
