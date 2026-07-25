namespace Sudoku.CommandLine;

/// <summary>
/// Represents a command. The type provides the basic operation on implementation of handling and parent commands adding / removing.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="description">The description.</param>
public closed class CommandBase(string name, string description) : Command(name, description)
{
	/// <summary>
	/// Indicates whether the command has sub-commands.
	/// </summary>
	public virtual bool HasSubcommands => false;

	/// <summary>
	/// Indicates the options.
	/// </summary>
	public virtual SymbolList<Option> OptionsCore => [];

	/// <summary>
	/// Indicates the global options of the current command, applying to the current command and its sub-commands.
	/// </summary>
	public virtual SymbolList<Option> GlobalOptionsCore => [];

	/// <summary>
	/// Indicates the arguments.
	/// </summary>
	public virtual SymbolList<Argument> ArgumentsCore => [];

	/// <summary>
	/// Indicates the parent command. The value can be <see langword="null"/> if the command has no parent.
	/// </summary>
	public CommandBase? Parent { get; init; }


	/// <summary>
	/// <para>Provides a way to handle the command.</para>
	/// <para>
	/// This method should be implemented if the constructor calls method
	/// <see cref="Handler.SetHandler(Command, Action{InvocationContext})"/>.
	/// </para>
	/// </summary>
	/// <param name="context">
	/// The context. Use property <see cref="InvocationContext.ParseResult"/> to retrieve the target option result.
	/// </param>
	/// <seealso cref="Handler.SetHandler(Command, Action{InvocationContext})"/>
	/// <seealso cref="InvocationContext.ParseResult"/>
	protected virtual void HandleCore(InvocationContext context)
	{
	}
}
