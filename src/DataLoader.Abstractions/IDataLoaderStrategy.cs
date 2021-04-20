using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	/// <summary>
	/// A <see cref="IDataLoaderStrategy"/> is an execution strategy to load the data.
	/// </summary>
	public interface IDataLoaderStrategy : IDisposable
	{
		/// <summary>
		/// Loads data based on a given <see cref="IDataLoaderRequest"/> for a <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="ct">The <see cref="CancellationToken"/>.</param>
		/// <param name="request">The <see cref="IDataLoaderRequest"/>.</param>
		/// <returns>The data loaded by the strategy, for the <see cref="IDataLoader"/>.</returns>
		Task<object> Load(CancellationToken ct, IDataLoaderRequest request);		
	}
}
