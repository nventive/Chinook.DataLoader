using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Represents the state of the <see cref="DataLoaderView"/>.
	/// It's similar to <see cref="IDataLoaderState"/>, but it includes additional properties (such as <see cref="View"/> and <see cref="Parent"/>) to ease data binding.
	/// </summary>
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

		private readonly WeakReference<Control> _view = new WeakReference<Control>(null);
		private readonly WeakReference<object> _parent = new WeakReference<object>(null);
		private readonly WeakReference<IDataLoader> _source = new WeakReference<IDataLoader>(null);
		private readonly WeakReference<IDataLoaderRequest> _request = new WeakReference<IDataLoaderRequest>(null);
		private readonly WeakReference<object> _data = new WeakReference<object>(null);
		private readonly WeakReference<Exception> _error = new WeakReference<Exception>(null);

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

		/// <summary>
		/// The control that generated this state.
		/// This is typically a <see cref="DataLoaderView"/>, but it could be something else if you implement your own <see cref="IDataLoaderViewDelegate"/>.
		/// </summary>
		public Control View
		{
			get => _view.GetTargetOrDefault();
			set => _view.SetTarget(value);
		}

		/// <summary>
		/// The DataContext of the <see cref="View"/>.
		/// This is really helpful to bind to properties of the ViewModel owning the <see cref="IDataLoader"/>.
		/// </summary>
		public object Parent
		{
			get => _parent.GetTargetOrDefault();
			set => _parent.SetTarget(value);
		}

		/// <summary>
		/// The <see cref="IDataLoader"/>.
		/// </summary>
		public IDataLoader Source
		{
			get => _source.GetTargetOrDefault();
			set => _source.SetTarget(value);
		}

		/// <inheritdoc cref="IDataLoaderState.Request"/>
		public IDataLoaderRequest Request
		{
			get => _request.GetTargetOrDefault();
			set => _request.SetTarget(value);
		}

		/// <inheritdoc cref="IDataLoaderState.Data"/>
		public object Data
		{
			get => _data.GetTargetOrDefault();
			set => _data.SetTarget(value);
		}

		/// <inheritdoc cref="IDataLoaderState.Error"/>
		public Exception Error
		{
			get => _error.GetTargetOrDefault();
			set => _error.SetTarget(value);
		}

		/// <inheritdoc cref="IDataLoaderState.IsLoading"/>
		public bool IsLoading { get; set; }

		/// <inheritdoc cref="IDataLoaderState.IsInitial"/>
		public bool IsInitial { get; set; }

		/// <inheritdoc cref="IDataLoaderState.IsEmpty"/>
		public bool IsEmpty { get; set; }
	}
}
