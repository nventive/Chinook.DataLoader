using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This is a default implementation of <see cref="IDataLoaderRequest"/>.
	/// </summary>
	public class DataLoaderRequest : IDataLoaderRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderRequest"/> class.
		/// </summary>
		/// <param name="sequenceId">The sequence Id of this request.</param>
		/// <param name="sourceTrigger"><see cref="IDataLoaderTrigger"/></param>
		/// <param name="triggerContext"><see cref="IDataLoaderContext"/></param>
		/// <param name="dataLoaderContextValues"></param>
		public DataLoaderRequest(int sequenceId, IDataLoaderTrigger sourceTrigger, IDataLoaderContext triggerContext, IDictionary<string, object> dataLoaderContextValues)
		{
			SequenceId = sequenceId;
			SourceTrigger = sourceTrigger ?? throw new ArgumentNullException(nameof(sourceTrigger));
			Context = new MergedDataLoaderContext(triggerContext, dataLoaderContextValues);
		}

		/// <inheritdoc/>
		public int SequenceId { get; }

		/// <inheritdoc />
		public IDataLoaderTrigger SourceTrigger { get; }

		/// <inheritdoc />
		public IDataLoaderContext Context { get; }

		/// <inheritdoc />
		public override string ToString()
		{
			return "Request #" + SequenceId;
		}
	}
}
