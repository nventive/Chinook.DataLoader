using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader.SubscribeToData
{
	/// <summary>
	/// In the context of SubscribeToData, this interface allows you to directly change either the data or the error of the <see cref="IDataLoader.State"/>.
	/// </summary>
	public interface IDataLoaderSetter
	{
		/// <summary>
		/// Sets the <see cref="IDataLoaderState.Data"/> on a <see cref="IDataLoader"/>.
		/// This also re-evaluates the <see cref="IDataLoaderState.IsEmpty"/> property.
		/// </summary>
		/// <param name="data">The data to set.</param>
		void SetData(object data);

		/// <summary>
		/// Sets the <see cref="IDataLoaderState.Error"/> on a <see cref="IDataLoader"/>.
		/// </summary>
		/// <param name="error">The exception to set.</param>
		void SetError(Exception error);
	}
}
