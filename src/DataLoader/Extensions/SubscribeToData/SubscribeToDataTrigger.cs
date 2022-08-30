using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader.SubscribeToData
{
	/// <summary>
	/// This is <see cref="IDataLoaderTrigger"/> implementation for the SubscribeToData recipe.
	/// It works in pair with <see cref="SubscribeToDataStrategy"/>.
	/// </summary>
	public class SubscribeToDataTrigger : DataLoaderTriggerBase, IDataLoaderSetter
	{
		public const string DataContextKey = "SubscribeToDataTrigger.Data";
		public const string ErrorContextKey = "SubscribeToDataTrigger.Error";

		/// <summary>
		/// Creates a new instance of <see cref="SubscribeToDataTrigger"/>.
		/// </summary>
		/// <param name="name">The name of this trigger.</param>
		public SubscribeToDataTrigger(string name) : base(name)
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// By calling this, a Load is requested and intercepted by <see cref="SubscribeToDataStrategy"/> which directly returns the <paramref name="data"/>.
		/// </remarks>
		public void SetData(object data)
		{
			var context = new DataLoaderContext(new Dictionary<string, object>
			{
				{ DataContextKey, data },
			});

			RaiseLoadRequested(context);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// By calling this, a Load is requested and intercepted by <see cref="SubscribeToDataStrategy"/> which directly throws the <paramref name="error"/>.
		/// </remarks>
		public void SetError(Exception error)
		{
			var context = new DataLoaderContext(new Dictionary<string, object>
			{
				{ ErrorContextKey, error },
			});

			RaiseLoadRequested(context);
		}
	}
}
