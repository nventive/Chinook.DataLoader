using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chinook.DataLoader
{
	public static class ObservableDataLoaderExtensions
	{
		/// <summary>
		/// Adds an observable trigger.
		/// </summary>
		/// <typeparam name="TBuilder">Builder type</typeparam>
		/// <typeparam name="T">Observable type</typeparam>
		/// <param name="dataLoaderBuilder"><see cref="IDataLoaderBuilder"/></param>
		/// <param name="observable">Observable</param>
		/// <param name="name">Trigger name</param>
		/// <param name="contextSelector">Optional selector to populate the <see cref="IDataLoaderContext"/> from the observable value.</param>
		/// <returns><see cref="IDataLoaderBuilder"/></returns>
		public static TBuilder TriggerFromObservable<TBuilder, T>(this TBuilder dataLoaderBuilder, IObservable<T> observable, string name = null, Func<T, IDataLoaderContext> contextSelector = null) where TBuilder : IDataLoaderBuilder
			=> dataLoaderBuilder.WithTrigger(new ObservableDataLoaderTrigger<T>(observable, contextSelector, name));
	}

	/// <summary>
	/// A <see cref="IDataLoaderTrigger"/> that will request a load
	/// when the observer receives a new value in OnNext.
	/// </summary>
	/// <typeparam name="T">Type of observable</typeparam>
	public class ObservableDataLoaderTrigger<T> : DataLoaderTriggerBase
	{
		private readonly IDisposable _subscription;
		private readonly Func<T, IDataLoaderContext> _contextSelector;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableDataLoaderTrigger{T}"/> class.
		/// </summary>
		/// <param name="observable">Observable</param>
		/// <param name="contextSelector">Context selector</param>
		/// <param name="name">Name</param>
		public ObservableDataLoaderTrigger(IObservable<T> observable, Func<T, IDataLoaderContext> contextSelector = null, string name = null)
			: base(name)
		{
			_contextSelector = contextSelector ?? DefaultContextSelector;
			_subscription = observable.Subscribe(new DataLoaderTriggerObserver(OnNext));
		}

		private void OnNext(T observableValue)
		{
			RaiseLoadRequested(_contextSelector(observableValue));
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			base.Dispose();

			_subscription.Dispose();
		}

		private class DataLoaderTriggerObserver : IObserver<T>
		{
			private readonly Action<T> _onNext;

			public DataLoaderTriggerObserver(Action<T> onNext)
			{
				_onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
			}

			public void OnCompleted() { }

			public void OnError(Exception error) { }

			public void OnNext(T value) => _onNext(value);
		}

		private static IDataLoaderContext DefaultContextSelector(T observableValue)
		{
			return null;
		}
	}
}
