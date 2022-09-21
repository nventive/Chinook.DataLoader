using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace Chinook.DataLoader
{
	public partial class DataLoaderView
	{
		/// <summary>
		/// The <see cref="IDataLoader"/> driving the <see cref="DataLoaderView"/>.
		/// </summary>
		public IDataLoader Source
		{
			get => (IDataLoader)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(IDataLoader), typeof(DataLoaderView), new PropertyMetadata(null, OnSourceChanged));

		private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataLoaderView)d).OnSourceChanged((IDataLoader)e.NewValue);
		}

		/// <summary>
		/// The binding-friendly state of the <see cref="DataLoaderView"/>.
		/// </summary>
		public DataLoaderViewState State
		{
			get => (DataLoaderViewState)GetValue(StateProperty);
			private set => SetValue(StateProperty, value);
		}

		public static readonly DependencyProperty StateProperty =
			DependencyProperty.Register("State", typeof(DataLoaderViewState), typeof(DataLoaderView), new PropertyMetadata(null));

		/// <summary>
		/// The template to display the main content of the <see cref="DataLoaderView"/>.
		/// This would typically use the <see cref="DataLoaderViewState.Data"/> to display the data loaded by the <see cref="IDataLoader"/>.
		/// </summary>
		public DataTemplate ContentTemplate
		{
			get => (DataTemplate)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		public static readonly DependencyProperty ContentTemplateProperty =
			DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

		/// <summary>
		/// The template to display when the state is considered empty.
		/// This directly links to <see cref="IDataLoaderState.IsEmpty"/>.
		/// </summary>
		public DataTemplate EmptyTemplate
		{
			get => (DataTemplate)GetValue(EmptyTemplateProperty);
			set => SetValue(EmptyTemplateProperty, value);
		}

		public static readonly DependencyProperty EmptyTemplateProperty =
			DependencyProperty.Register("EmptyTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

		/// <summary>
		/// The template to display when the state contains an error.
		/// Typically this would be use when the state contains an error and no data.
		/// </summary>
		public DataTemplate ErrorTemplate
		{
			get => (DataTemplate)GetValue(ErrorTemplateProperty);
			set => SetValue(ErrorTemplateProperty, value);
		}

		public static readonly DependencyProperty ErrorTemplateProperty =
			DependencyProperty.Register("ErrorTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

		/// <summary>
		/// The template to display when the state contains both an error and data.
		/// Your style could overlay this template over the <see cref="ContentTemplate"/>.
		/// </summary>
		public DataTemplate ErrorNotificationTemplate
		{
			get => (DataTemplate)this.GetValue(ErrorNotificationTemplateProperty);
			set => this.SetValue(ErrorNotificationTemplateProperty, value);
		}

		public static readonly DependencyProperty ErrorNotificationTemplateProperty =
			DependencyProperty.Register("ErrorNotificationTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(default(DataTemplate)));

		/// <summary>
		/// This is the <see cref="ICommand"/> used to bind a refresh button.
		/// </summary>
		public ICommand RefreshCommand
		{
			get => (ICommand)GetValue(RefreshCommandProperty);
			set => SetValue(RefreshCommandProperty, value);
		}

		public static readonly DependencyProperty RefreshCommandProperty =
			DependencyProperty.Register("RefreshCommand", typeof(ICommand), typeof(DataLoaderView), new PropertyMetadata(null));

		/// <summary>
		/// This is effectively the minimum visual state duration.
		/// Setting this to 1 second means you'll see each visual state at least 1 second.
		/// You should use this to avoid visual flickers.
		/// </summary>
		public TimeSpan StateMinimumDuration
		{
			get => (TimeSpan)GetValue(StateMinimumDurationProperty);
			set => SetValue(StateMinimumDurationProperty, value);
		}

		public static readonly DependencyProperty StateMinimumDurationProperty =
			DependencyProperty.Register("StateMinimumDuration", typeof(TimeSpan), typeof(DataLoaderView), new PropertyMetadata(TimeSpan.Zero, OnStateMinimumDurationChanged));

		private static void OnStateMinimumDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = (DataLoaderView)d;
			that._controller.StateMinimumDuration = e.NewValue as TimeSpan? ?? TimeSpan.Zero;
		}

		/// <summary>
		/// This is delay that the <see cref="DataLoaderView"/> waits before changing state.
		/// This is useful to avoid displaying a loading state when the loading is very quick.
		/// You should keep this value low (around 100-200ms) to avoid slowing down your user experience.
		/// E.g. If you set this to 200ms and your loading typically lasts 100ms, it means that you'll typically never see any loading state. 
		/// </summary>
		public TimeSpan StateChangingThrottleDelay
		{
			get { return (TimeSpan)GetValue(StateChangingThrottleDelayProperty); }
			set { SetValue(StateChangingThrottleDelayProperty, value); }
		}

		public static readonly DependencyProperty StateChangingThrottleDelayProperty =
			DependencyProperty.Register("StateChangingThrottleDelay", typeof(TimeSpan), typeof(DataLoaderView), new PropertyMetadata(TimeSpan.Zero, OnStateChangingThrottleDelay));

		private static void OnStateChangingThrottleDelay(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = (DataLoaderView)d;
			that._controller.StateChangingThrottleDelay = e.NewValue as TimeSpan? ?? TimeSpan.Zero;
		}

		/// <summary>
		/// Gets or sets whether the <see cref="DataLoaderView"/> automatically loads its <see cref="IDataLoader"/>.
		/// This defaults to true.
		/// </summary>
		public bool AutoLoad
		{
			get => (bool)this.GetValue(AutoLoadProperty);
			set => this.SetValue(AutoLoadProperty, value);
		}

		public static readonly DependencyProperty AutoLoadProperty =
			DependencyProperty.Register("AutoLoad", typeof(bool), typeof(DataLoaderView), new PropertyMetadata(true));
	}
}
