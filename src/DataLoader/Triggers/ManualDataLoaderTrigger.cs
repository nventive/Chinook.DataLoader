using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	public static class ManualDataLoaderExtensions
	{
		/// <summary>
		/// Adds a manual trigger.
		/// </summary>
		/// <param name="dataLoaderBuilder"><see cref="IDataLoaderBuilder"/></param>
		/// <param name="trigger"><see cref="ManualDataLoaderTrigger"/></param>
		/// <returns><see cref="IDataLoaderBuilder" /></returns>
		public static TBuilder TriggerManually<TBuilder>(this TBuilder dataLoaderBuilder, ManualDataLoaderTrigger trigger) where TBuilder: IDataLoaderBuilder
			=> dataLoaderBuilder.WithTrigger(trigger);
	}

	/// <summary>
	/// A <see cref="IDataLoaderTrigger"/> that will request a load
	/// when the method <see cref="Trigger"/> is called.
	/// </summary>
	public class ManualDataLoaderTrigger : DataLoaderTriggerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ManualDataLoaderTrigger"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		public ManualDataLoaderTrigger(string name = null)
			: base(name)
		{
		}

		/// <summary>
		/// Will trigger a request.
		/// </summary>
		/// <param name="context"><see cref="IDataLoaderContext"/></param>
		public void Trigger(IDataLoaderContext context = null) => RaiseLoadRequested(context);
	}
}
