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
		// Here we use WeakReference on all reference types because UWP creates "Dependent Handles" on DataLoaderViewState instances, preventing GC.
		// There isn't much information about how to investigate dependent handles so this solution might not be the one.

		// Note that even though we use WeakReferences, we shouldn't get any unwanted GC.
		// View is strongly held by its UI tree parent. Once the DataLoaderView lost, the DataLoaderViewState shouldn't be used anyways.
		// Parent (DataLoaderView's DataContext) is strongly held by the DataLoaderView.
		// Source is also strongly held by the DataLoaderView.
		// Request, Data, and Error are all strongly held by the DataLoader's State (strongly held in DataLoaderView's Source).

		private WeakReference<DataLoaderView> _view = new WeakReference<DataLoaderView>(null);
		private WeakReference<object> _parent = new WeakReference<object>(null);
		private WeakReference<IDataLoader> _source = new WeakReference<IDataLoader>(null);
		private WeakReference<IDataLoaderRequest> _request = new WeakReference<IDataLoaderRequest>(null);
		private WeakReference<object> _data = new WeakReference<object>(null);
		private WeakReference<Exception> _error = new WeakReference<Exception>(null);

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

		public DataLoaderView View
		{
			get => _view.GetTargetOrDefault();
			set => _view.SetTarget(value);
		}

		public object Parent
		{
			get => _parent.GetTargetOrDefault();
			set => _parent.SetTarget(value);
		}

		public IDataLoader Source
		{
			get => _source.GetTargetOrDefault();
			set => _source.SetTarget(value);
		}

		public IDataLoaderRequest Request
		{
			get => _request.GetTargetOrDefault();
			set => _data.SetTarget(value);
		}

		public object Data
		{
			get => _data.GetTargetOrDefault();
			set => _data.SetTarget(value);
		}

		public Exception Error
		{
			get => _error.GetTargetOrDefault();
			set => _error.SetTarget(value);
		}

		public bool IsLoading { get; set; }

		public bool IsInitial { get; set; }

		public bool IsEmpty { get; set; }
	}
}
