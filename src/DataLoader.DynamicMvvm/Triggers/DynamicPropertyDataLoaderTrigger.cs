using System;
using System.Collections.Generic;
using System.Text;
using Chinook.DynamicMvvm;

namespace Chinook.DataLoader
{
	public static class DynamicPropertyDataLoaderExtensions
	{
		/// <summary>
		/// Adds a <see cref="IDynamicProperty"/> trigger.
		/// </summary>
		/// <param name="dataLoaderBuilder"><see cref="IDataLoaderBuilder"/></param>
		/// <param name="property"><see cref="IDynamicProperty"/></param>
		/// <returns><see cref="IDataLoaderBuilder"/></returns>
		public static IDataLoaderBuilder TriggerOnValueChanged(this IDataLoaderBuilder dataLoaderBuilder, IDynamicProperty property)
			=> dataLoaderBuilder.WithTrigger(new DynamicPropertyDataLoaderTrigger(property));
	}

	/// <summary>
	/// A <see cref="IDataLoaderTrigger"/> that will request a load
	/// when the value of a <see cref="IDynamicProperty"/> has changed.
	/// </summary>
	public class DynamicPropertyDataLoaderTrigger : DataLoaderTriggerBase
	{
		private readonly IDynamicProperty _property;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicPropertyDataLoaderTrigger"/> class.
		/// </summary>
		/// <param name="property"></param>
		public DynamicPropertyDataLoaderTrigger(IDynamicProperty property)
			: base(property.Name)
		{
			_property = property ?? throw new ArgumentNullException(nameof(property));
			_property.ValueChanged += OnValueChanged;
		}

		private void OnValueChanged(IDynamicProperty property)
		{
			RaiseLoadRequested();
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			base.Dispose();

			_property.ValueChanged -= OnValueChanged;
		}
	}
}
