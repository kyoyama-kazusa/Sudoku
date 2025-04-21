namespace Sudoku.CommandLine.Options;

/// <summary>
/// Provides a transforming method option.
/// </summary>
internal sealed class TransformatingMethodOption : Option<TransformType>, IOption<TransformType>
{
	/// <summary>
	/// Initializes a <see cref="TransformatingMethodOption"/> instance.
	/// </summary>
	public TransformatingMethodOption() : base(
		["-m", "--method"],
		ParseArgument,
		false,
		"Specifies the transforming methods; using comma ',' to separate multiple transforming rules"
	)
	{
		Arity = ArgumentArity.OneOrMore;
		IsRequired = true;
	}


	/// <inheritdoc/>
	public static TransformType ParseArgument(ArgumentResult result)
	{
		var typesResult = TransformType.None;
		foreach (var token in from token in result.Tokens select token.Value)
		{
			if (TransformType.TryParse(token, true, out var typeResult))
			{
				typesResult |= typeResult;
			}
			else
			{
				result.ErrorMessage = $"Invalid token to parse - '{token}'.";
				return default;
			}
		}
		return typesResult;
	}
}
