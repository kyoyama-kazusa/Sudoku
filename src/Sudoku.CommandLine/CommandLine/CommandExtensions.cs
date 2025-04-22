namespace System.CommandLine;

/// <summary>
/// Provides with extension methods on <see cref="Command"/>.
/// </summary>
/// <seealso cref="Command"/>
public static class CommandExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Command"/>.
	/// </summary>
	extension(Command @this)
	{
		/// <summary>
		/// Adds a list of <see cref="Option"/> instances into the command.
		/// </summary>
		/// <param name="options">The options.</param>
		public void AddRange(params SymbolList<Option> options)
		{
			foreach (var option in options)
			{
				@this.Add(option);
			}
		}

		/// <summary>
		/// Adds a list of <see cref="Argument"/> instances into the command.
		/// </summary>
		/// <param name="arguments">The arguments.</param>
		public void AddRange(params SymbolList<Argument> arguments)
		{
			foreach (var argument in arguments)
			{
				@this.Add(argument);
			}
		}

		/// <summary>
		/// Adds a list of <see cref="Command"/> instances into the command.
		/// </summary>
		/// <param name="subcommands">The subcommands.</param>
		public void AddRange(params SymbolList<Command> subcommands)
		{
			foreach (var subcommand in subcommands)
			{
				@this.Add(subcommand);
			}
		}

		/// <summary>
		/// Adds a list of <see cref="Option"/> instances into the command,
		/// as global ones applying to the current command and its sub-commands.
		/// </summary>
		/// <param name="options">The options.</param>
		public void AddRangeGlobal(params SymbolList<Option> options)
		{
			foreach (var option in options)
			{
				@this.AddGlobalOption(option);
			}
		}
	}
}
