#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.DataLoader
{
	/// <summary>
	/// This attributes allows to preserve code when using Xamarin.
	/// See https://docs.microsoft.com/en-us/xamarin/ios/deploy-test/linker?tabs=windows#preserving-code for more info.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class PreserveAttribute : Attribute
	{
		public bool AllMembers;
	}
}
#pragma warning restore IDE1006 // Naming Styles
