using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	internal static class WeakReference
	{
		public static T GetTargetOrDefault<T>(this WeakReference<T> weakReference)
			where T : class
		{
			return weakReference.TryGetTarget(out var value) ? value : default(T);
		}
	}
}
