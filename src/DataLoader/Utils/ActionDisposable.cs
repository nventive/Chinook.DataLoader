using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	internal class ActionDisposable : IDisposable
	{
		private readonly Action _dispose;

		private bool _isDisposed = false;

		public ActionDisposable(Action dispose)
		{
			_dispose = dispose;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_dispose();
				_isDisposed = true;
			}
		}
	}
}
