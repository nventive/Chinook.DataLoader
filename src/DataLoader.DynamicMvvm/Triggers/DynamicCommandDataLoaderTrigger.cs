using System;
using System.Collections.Generic;
using System.Text;
using Chinook.DynamicMvvm;

namespace Chinook.DataLoader
{
	/// <summary>
	/// ­Represents the options for the <see cref="DynamicCommandDataLoaderTrigger"/>.
	/// </summary>
	[Flags]
	public enum CommandTriggerOptions
	{
		/// <summary>
		/// The trigger requests a load before just before the command executes. 
		/// </summary>
		TriggerBeforeCommandExecution = 0b01,

		/// <summary>
		/// The trigger requests a load before just after the command executed. 
		/// </summary>
		TriggerAfterCommandExecution = 0b10
	}

	public static class DynamicCommandDataLoaderExtensions
	{
		/// <summary>
		/// Adds a <see cref="IDynamicCommand"/> trigger.
		/// </summary>
		/// <remarks>
		/// The name of the trigger is the name of the <paramref name="command"/>.
		/// </remarks>
		/// <typeparam name="TBuilder">The type of builder.</typeparam>
		/// <param name="dataLoaderBuilder">This builder.</param>
		/// <param name="command">The dynamic command causing the triggers.</param>
		/// <param name="triggerExecution">The trigger options.</param>
		/// <returns><see cref="IDataLoaderBuilder"/></returns>
		public static TBuilder TriggerOnCommandExecution<TBuilder>(
			this TBuilder dataLoaderBuilder,
			IDynamicCommand command,
			CommandTriggerOptions triggerExecution = CommandTriggerOptions.TriggerBeforeCommandExecution)
			where TBuilder : IDataLoaderBuilder
			=> dataLoaderBuilder.WithTrigger(new DynamicCommandDataLoaderTrigger(command, triggerExecution));
	}

	/// <summary>
	/// This <see cref="IDataLoaderTrigger"/> will request a load
	/// when the command becomes executing.
	/// </summary>
	public class DynamicCommandDataLoaderTrigger : DataLoaderTriggerBase
	{
		private readonly IDynamicCommand _command;
		private readonly CommandTriggerOptions _triggerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCommandDataLoaderTrigger"/> class.
		/// </summary>
		/// <param name="command">The dynamic command causing the triggers.</param>
		/// <param name="triggerOptions">The trigger options.</param>
		public DynamicCommandDataLoaderTrigger(IDynamicCommand command, CommandTriggerOptions triggerOptions)
			: base(command?.Name)
		{
			_command = command ?? throw new ArgumentNullException(nameof(command));
			_triggerOptions = triggerOptions;
			_command.IsExecutingChanged += OnIsExecutingChanged;
		}

		private void OnIsExecutingChanged(object sender, EventArgs e)
		{
			if ((_triggerOptions.HasFlag(CommandTriggerOptions.TriggerBeforeCommandExecution) && _command.IsExecuting)
				|| (_triggerOptions.HasFlag(CommandTriggerOptions.TriggerAfterCommandExecution) && !_command.IsExecuting))
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
