namespace Sudoku.CommandLine.Options;

/// <summary>
/// Represents a file path option.
/// </summary>
internal sealed class OutputFilePathOption : Option<string>, IOption<string>
{
	/// <summary>
	/// Initializes a <see cref="OutputFilePathOption"/> instance.
	/// </summary>
	public OutputFilePathOption() : base(["--path", "-p"], "Specifies the output file path")
	{
		Arity = ArgumentArity.ExactlyOne;
		IsRequired = false;
		SetDefaultValue(null);
	}


	/// <inheritdoc/>
	static string IMySymbol<string>.ParseArgument(ArgumentResult result) => throw new NotImplementedException();
}
