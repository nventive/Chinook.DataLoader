using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader.SubscribeToData
{
	public class SubscribeToDataTrigger : DataLoaderTriggerBase, IDataLoaderSetter
	{
		public const string DataContextKey = "SubscribeToDataTrigger.Data";
		public const string ErrorContextKey = "SubscribeToDataTrigger.Error";

		public SubscribeToDataTrigger(string name) : base(name)
		{
		}

		public void SetData(object data)
		{
			var context = new DataLoaderContext(new Dictionary<string, object>
			{
				{ DataContextKey, data },
			});

			RaiseLoadRequested(context);
		}

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
