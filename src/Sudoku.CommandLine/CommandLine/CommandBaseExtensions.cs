namespace Sudoku.CommandLine;

/// <summary>
/// Provides with extension methods on <see cref="CommandBase"/>.
/// </summary>
/// <seealso cref="CommandBase"/>
public static class CommandBaseExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TCommand"/>,
	/// where <typeparamref name="TCommand"/> satisfies <see cref="CommandBase"/> constraint.
	/// </summary>
	extension<TCommand>(TCommand @this) where TCommand : CommandBase
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out SymbolList<Option> options, out SymbolList<Argument> arguments)
		{
			options = @this.OptionsCore;
			arguments = @this.ArgumentsCore;
		}

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(
			out SymbolList<Option> options,
			out SymbolList<Argument> arguments,
			out SymbolList<Option> globalOptions
		)
			=> ((options, arguments), globalOptions) = (@this, @this.Parent?.GlobalOptionsCore ?? []);
	}
}
