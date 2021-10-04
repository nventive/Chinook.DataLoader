using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This interface is used to interact with <see cref="DataLoaderViewController"/>.
	/// </summary>
	public interface IDataLoaderViewDelegate
	{
		/// <summary>
		/// Gets the control on which the <see cref="DataLoaderViewController"/> will apply visual states.
		/// The value will be published to <see cref="DataLoaderViewState.View"/>.
		/// </summary>
		/// <remarks>
		/// Implementors can return null to skip visual states computations.
		/// </remarks>
		/// <returns>The view associated to this <see cref="IDataLoaderViewDelegate"/>, if any.</returns>
		Control GetView();

		/// <summary>
		/// Gets the DataContext associated with the implementor.
		/// The value will be published to <see cref="DataLoaderViewState.Parent"/>.
		/// </summary>
		/// <returns>The DataContext.</returns>
		object GetDataContext();

		/// <summary>
		/// Sets the <see cref="DataLoaderViewState"/> to the implementor from the <see cref="DataLoaderViewController"/>.
		/// The implementor can then react in the way it sees fit.
		/// </summary>
		/// <param name="viewState">The <see cref="DataLoaderViewState"/> computed by the <see cref="DataLoaderViewController"/>.</param>
		void SetState(DataLoaderViewState viewState);
	}
}
