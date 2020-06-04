using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Chinook.DataLoader
{
	[Preserve(AllMembers = true)]
	[Bindable]
	public class DataLoaderViewState
	{
		public DataLoaderView View { get; set; }

		public object Parent { get; set; }

		public IDataLoader Source { get; set; }

		public IDataLoaderRequest Request { get; set; }

		public object Data { get; set; }

		public Exception Error { get; set; }

		public bool IsLoading { get; set; }

		public bool IsInitial { get; set; }

		public bool IsEmpty { get; set; }
	}
}
