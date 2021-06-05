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
		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderViewState"/>.
		/// </summary>
		public DataLoaderViewState()
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderViewState"/> copying all propreties from the provided <paramref name="dataLoaderViewState"/>.
		/// </summary>
		/// <remarks>
		/// This copy constructor can be useful if you want to use x:Bind inside a DataLoader's ContentTemplate.
		/// </remarks>
		/// <param name="dataLoaderViewState">The copy reference.</param>
		public DataLoaderViewState(DataLoaderViewState dataLoaderViewState)
		{
			View = dataLoaderViewState.View;
			Parent = dataLoaderViewState.Parent;
			Source = dataLoaderViewState.Source;
			Request = dataLoaderViewState.Request;
			Data = dataLoaderViewState.Data;
			Error = dataLoaderViewState.Error;
			IsLoading = dataLoaderViewState.IsLoading;
			IsInitial = dataLoaderViewState.IsInitial;
			IsEmpty = dataLoaderViewState.IsEmpty;
		}

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
