using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// Base implementation of <see cref="IDataLoaderTrigger"/>.
	/// </summary>
	public abstract class DataLoaderTriggerBase : IDataLoaderTrigger
	{
		/// <summary>
		/// Creates a new instance of <see cref="DataLoaderTriggerBase"/>.
		/// </summary>
		/// <param name="name">The name of this trigger.</param>
		public DataLoaderTriggerBase(string name)
		{
			Name = name ?? GetType().Name;
		}

		/// <inheritdoc cref="IDataLoaderTrigger.Name" />
		public string Name { get; }

		/// <inheritdoc cref="IDataLoaderTrigger.LoadRequested" />
		public event LoadRequestedEventHandler LoadRequested;

		/// <summary>
		/// Raises the <see cref="LoadRequested"/> event.
		/// </summary>
		/// <param name="context">The optional context for the trigger.</param>
		protected void RaiseLoadRequested(IDataLoaderContext context = null)
		{
			LoadRequested?.Invoke(this, context);
		}

		/// <inheritdoc/>
		public virtual void Dispose()
		{
			// Clear event subscriptions.
			LoadRequested = null;
		}

		/// <inheritdoc/>
		public override string ToString() => $"{Name} ({GetType().Name})";
	}
}
