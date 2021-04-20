using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader.DisposeWithNextLoad
{
	/// <summary>
	/// This class is a container of <see cref="IDisposable"/> objects that automatically disposes its items when necessary.
	/// It associates each disposable object with the <see cref="IDataLoaderRequest"/> that created them and keeps track of that request.
	/// When items from a newer request are added, the items associated to a previous <see cref="IDataLoaderRequest"/> are disposed.
	/// </summary>
	/// <remarks>
	/// This class is not thread-safe on its own, but <see cref="IDataLoader"/> doesn't allow concurrent loads so it's safe to use in that context.
	/// </remarks>
	public class SequenceDisposableGroup : IDisposable
	{
		private readonly List<IDisposable> _disposables = new List<IDisposable>();

		private int _sequenceId;

		/// <summary>
		/// Adds a disposable and disposes all disposables associated with a previous request, when applicable.
		/// </summary>
		/// <param name="sequenceId">The <see cref="IDataLoaderRequest.SequenceId"/> of the request associated with <paramref name="disposable"/>.</param>
		/// <param name="disposable">The disposable object to add.</param>
		public void AddOrClear(int sequenceId, IDisposable disposable)
		{
			if (sequenceId > _sequenceId)
			{
				ClearDisposables();
				_sequenceId = sequenceId;
			}

			_disposables.Add(disposable);
		}

		private void ClearDisposables()
		{
			foreach (var existingDisposable in _disposables)
			{
				existingDisposable.Dispose();
			}
			_disposables.Clear();
		}

		public void Dispose()
		{
			ClearDisposables();
		}
	}
}
