using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.DataLoader.SubscribeToData;
using Microsoft.Extensions.Logging;

namespace Chinook.DataLoader
{
	public static class SubscribeToDataExtensions
	{
		/// <summary>
		/// Automatically re-evaluates <see cref="IDataLoaderState.IsEmpty"/> when <see cref="INotifyCollectionChanged.CollectionChanged"/> is raised.
		/// </summary>
		/// <typeparam name="TData">Any type implementing <see cref="INotifyCollectionChanged"/>.</typeparam>
		/// <param name="builder">The builder.</param>
		/// <returns>The original builder.</returns>
		public static IDataLoaderBuilder<TData> UpdateOnCollectionChanged<TData>(
			this IDataLoaderBuilder<TData> builder)
			where TData : INotifyCollectionChanged
		{
			return builder.SubscribeToData<TData>(Subscribe);

			IDisposable Subscribe(TData data, IDataLoaderSetter setter)
			{
				return data.SubscribeToUpdateDataLoader(setter);
			}
		}

		/// <summary>
		/// Tries to subscribe to the data produced by the <see cref="IDataLoader"/> in order to update its state without reloading the <see cref="IDataLoader"/>.
		/// This makes sense when <typeparamref name="TData"/> is something observable such as <see cref="INotifyCollectionChanged"/>.
		/// You should use this method with <see cref="DataLoaderBuilderFactory.DataLoaderBuilderFactory(Func{IDataLoaderBuilder, IDataLoaderBuilder})"/> so that all DataLoaders of <typeparamref name="TData"/> subscribe to their data automatically. 
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="builder">The builder.</param>
		/// <param name="subscribeToData">
		/// The method subscribing to the data.
		/// Use that method to subscribe to events available on <typeparamref name="TData"/> and return an IDisposable that unsubscribes from events.
		/// Your event handlers should leverage <see cref="IDataLoaderSetter.SetData(object)"/> and <see cref="IDataLoaderSetter.SetError(Exception)"/>.
		/// </param>
		/// <returns>The original builder.</returns>
		public static IDataLoaderBuilder TrySubscribeToData<TData>(
			this IDataLoaderBuilder builder,
			Func<TData, IDataLoaderSetter, IDisposable> subscribeToData)
		{
			// Early-out 1: The SubscribeToData delegating strategy is already present in the builder.
			if (builder.DelegatingStrategies.Any(s => s is SubscribeToDataStrategy))
			{
				return builder;
			}

			// Early-out 2: The builder isn't typed.
			if (builder.DataType == null)
			{
				if (typeof(SubscribeToDataExtensions).Log().IsEnabled(LogLevel.Error))
				{
					typeof(SubscribeToDataExtensions).Log().LogError("Can't validate type. Make sure the 'IDataLoaderBuilder.DataType' property is set before calling 'TrySubscribeToData'.");
				}

				return builder;
			}

			var dataType = typeof(TData);

			// Early-out 3: The builder type doesn't match TData.
			if (!dataType.IsAssignableFrom(builder.DataType))
			{
				// If TData doesn't match the builder type, we don't do anything.
				return builder;
			}

			var namePrefix = builder.Name == null ? string.Empty : builder.Name + ".";
			var trigger = new SubscribeToDataTrigger(namePrefix + nameof(SubscribeToDataTrigger));
			var strategy = new SubscribeToDataStrategy(data => subscribeToData((TData)data, trigger));

			return builder
				.WithTrigger(trigger)
				.WithStrategy(strategy);
		}

		/// <summary>
		/// Subscribes to the data produced by the <see cref="IDataLoader"/> in order to update its state without reloading the <see cref="IDataLoader"/>.
		/// This makes sense when <typeparamref name="TData"/> is something observable such as <see cref="INotifyCollectionChanged"/>.
		/// </summary>
		/// <typeparam name="TData">The type of data.</typeparam>
		/// <param name="builder">The builder.</param>
		/// <param name="subscribeToData">
		/// The method subscribing to the data.
		/// Use that method to subscribe to events available on <typeparamref name="TData"/> and return an IDisposable that unsubscribes from events.
		/// Your event handlers should leverage <see cref="IDataLoaderSetter.SetData(object)"/> and <see cref="IDataLoaderSetter.SetError(Exception)"/>.
		/// </param>
		/// <returns>The original <paramref name="builder"/>.</returns>
		public static IDataLoaderBuilder<TData> SubscribeToData<TData>(
			this IDataLoaderBuilder<TData> builder,
			Func<TData, IDataLoaderSetter, IDisposable> subscribeToData)
		{
			var namePrefix = builder.Name == null ? string.Empty : builder.Name + ".";
			var trigger = new SubscribeToDataTrigger(namePrefix + nameof(SubscribeToDataTrigger));
			var strategy = new SubscribeToDataStrategy(data => subscribeToData((TData)data, trigger));

			return builder
				.WithTrigger(trigger)
				.WithStrategy(strategy);
		}
	}
}
