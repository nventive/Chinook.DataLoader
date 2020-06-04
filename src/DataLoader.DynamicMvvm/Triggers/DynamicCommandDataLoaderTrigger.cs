using System;
using System.Collections.Generic;
using System.Text;
using Chinook.DynamicMvvm;

namespace Chinook.DataLoader
{
	public static class DynamicCommandDataLoaderExtensions
	{
		/// <summary>
		/// Adds a <see cref="IDynamicCommand"/> trigger.
		/// </summary>
		/// <param name="dataLoaderBuilder"><see cref="IDataLoaderBuilder"/></param>
		/// <param name="command"><see cref="IDynamicCommand"/></param>
		/// <returns><see cref="IDataLoaderBuilder"/></returns>
		public static IDataLoaderBuilder TriggerOnCommandExecution(this IDataLoaderBuilder dataLoaderBuilder, IDynamicCommand command)
			=> dataLoaderBuilder.WithTrigger(new DynamicCommandDataLoaderTrigger(command));
	}

	/// <summary>
	/// This <see cref="IDataLoaderTrigger"/> will request a load
	/// when the command becomes executing.
	/// </summary>
	public class DynamicCommandDataLoaderTrigger : DataLoaderTriggerBase
	{
		private readonly IDynamicCommand _command;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCommandDataLoaderTrigger"/> class.
		/// </summary>
		/// <param name="command"><see cref="IDynamicCommand"/></param>
		public DynamicCommandDataLoaderTrigger(IDynamicCommand command)
			: base(command?.Name)
		{
			_command = command ?? throw new ArgumentNullException(nameof(command));
			_command.IsExecutingChanged += OnIsExecutingChanged;
		}

		private void OnIsExecutingChanged(object sender, EventArgs e)
		{
			if (_command.IsExecuting)
			{
				RaiseLoadRequested();
			}
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			base.Dispose();

			_command.IsExecutingChanged -= OnIsExecutingChanged;
		}
	}
}
