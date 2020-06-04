using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This implementation of <see cref="IDataLoaderStrategy"/> simply forwards the load execution to a provided <see cref="DataLoaderDelegate"/>.
	/// </summary>
	public class DelegatedDataLoaderStrategy : IDataLoaderStrategy
	{
		private readonly DataLoaderDelegate _dataLoaderDelegate;

		/// <summary>
		/// Creates a new instance of <see cref="DelegatedDataLoaderStrategy"/>.
		/// </summary>
		/// <param name="dataLoaderDelegate">The delegate to use for the <see cref="Load(CancellationToken, IDataLoaderRequest)"/> method.</param>
		public DelegatedDataLoaderStrategy(DataLoaderDelegate dataLoaderDelegate)
		{
			_dataLoaderDelegate = dataLoaderDelegate;
		}

		/// <inheritdoc/>
		public Task<object> Load(CancellationToken ct, IDataLoaderRequest request)
		{
			return _dataLoaderDelegate(ct, request);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
		}
	}
}
