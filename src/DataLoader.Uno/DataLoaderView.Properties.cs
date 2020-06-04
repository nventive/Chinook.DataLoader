using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Chinook.DataLoader
{
	public partial class DataLoaderView
	{
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

		public DataLoaderViewState State
		{
			get => (DataLoaderViewState)GetValue(StateProperty);
			private set => SetValue(StateProperty, value);
		}

		public static readonly DependencyProperty StateProperty =
			DependencyProperty.Register("State", typeof(DataLoaderViewState), typeof(DataLoaderView), new PropertyMetadata(null));

		public DataTemplate ContentTemplate
		{
			get => (DataTemplate)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		public static readonly DependencyProperty ContentTemplateProperty =
			DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

		public DataTemplate EmptyTemplate
		{
			get => (DataTemplate)GetValue(EmptyTemplateProperty);
			set => SetValue(EmptyTemplateProperty, value);
		}

		public static readonly DependencyProperty EmptyTemplateProperty =
			DependencyProperty.Register("EmptyTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

		public DataTemplate ErrorTemplate
		{
			get => (DataTemplate)GetValue(ErrorTemplateProperty);
			set => SetValue(ErrorTemplateProperty, value);
		}

		public static readonly DependencyProperty ErrorTemplateProperty =
			DependencyProperty.Register("ErrorTemplate", typeof(DataTemplate), typeof(DataLoaderView), new PropertyMetadata(null));

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
			that._stateMinDuration = e.NewValue as TimeSpan? ?? TimeSpan.Zero;
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
			that._stateChangingThrottleDelay = e.NewValue as TimeSpan? ?? TimeSpan.Zero;
		}

		public bool AutoLoad
		{
			get => (bool)this.GetValue(AutoLoadProperty);
			set => this.SetValue(AutoLoadProperty, value);
		}

		public static readonly DependencyProperty AutoLoadProperty =
			DependencyProperty.Register("AutoLoad", typeof(bool), typeof(DataLoaderView), new PropertyMetadata(true));
	}
}
