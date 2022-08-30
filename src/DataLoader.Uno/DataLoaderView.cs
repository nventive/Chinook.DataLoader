using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This control has visual states associated to the <see cref="IDataLoader"/> states.
	/// It can be used to display a loading indicator while the <see cref="IDataLoader"/> is loading data.
	/// </summary>
	[TemplateVisualState(GroupName = DataLoaderViewController.DataVisualGroup, Name = DataLoaderViewController.InitialVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.DataVisualGroup, Name = DataLoaderViewController.DataVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.DataVisualGroup, Name = DataLoaderViewController.EmptyVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.LoadingVisualGroup, Name = DataLoaderViewController.LoadingVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.LoadingVisualGroup, Name = DataLoaderViewController.NotLoadingVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.ErrorVisualGroup, Name = DataLoaderViewController.ErrorVisualStateName)]
	[TemplateVisualState(GroupName = DataLoaderViewController.ErrorVisualGroup, Name = DataLoaderViewController.NoErrorVisualStateName)]
	[ContentProperty(Name = "ContentTemplate")]
	public partial class DataLoaderView : Control, IDataLoaderViewDelegate
	{
		private readonly DataLoaderViewController _controller;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataLoaderView"/> class.
		/// </summary>
		public DataLoaderView()
		{
			this.DefaultStyleKey = typeof(DataLoaderView);

			_controller = new DataLoaderViewController(this, Dispatcher);

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			_controller.OnViewLoaded();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			_controller.OnViewUnloaded();
		}

		private void OnSourceChanged(IDataLoader dataLoader)
		{
			_controller.SetDataLoader(dataLoader);

			if (dataLoader != null && AutoLoad)
			{
				var manualDataLoaderTrigger = new ManualDataLoaderTrigger($"{nameof(DataLoaderView)}.{nameof(AutoLoad)}");

				dataLoader.AddTrigger(manualDataLoaderTrigger);

				manualDataLoaderTrigger.Trigger();				
			}
		}

		/// <summary>
		/// Sets the <see cref="State"/>.
		/// </summary>
		/// <remarks>
		/// This method is virtual to allow creation of more strongly typed version of DataLoaderView to allow usage of x:Bind inside the ContentTemplate.
		/// </remarks>
		/// <param name="state">The <see cref="DataLoaderViewState"/> to set as the current state.</param>
		protected virtual void SetState(DataLoaderViewState state)
		{
			State = state;
		}

		Control IDataLoaderViewDelegate.GetView()
		{
			return this;
		}

		void IDataLoaderViewDelegate.SetState(DataLoaderViewState viewState)
		{
			SetState(viewState);
		}

		object IDataLoaderViewDelegate.GetDataContext()
		{
			return DataContext;
		}
	}
}
