namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Represents a <see cref="GridFormatInfo{TGrid}"/> type that supports Comma-separated-values formatting.
/// </summary>
public sealed class CsvGridFormatInfo : GridFormatInfo<Grid>
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override IFormatProvider? GetFormat(Type? formatType) => formatType == typeof(GridFormatInfo<Grid>) ? this : null;

	/// <inheritdoc/>
	public override CsvGridFormatInfo Clone() => new();

	/// <inheritdoc/>
	protected internal override string FormatCore(in Grid grid)
	{
		var span = grid.ToString("0").Span;
		var sb = new StringBuilder(81 + 72 + 9);
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				if (span[i * 9 + j] - '0' is var digit and not 0)
				{
					sb.Append(digit);
				}
				sb.Append('\t');
			}
			sb.RemoveFrom(^1).AppendLine();
		}
		return sb.RemoveFrom(^Environment.NewLine.Length).ToString();
	}

	/// <inheritdoc/>
	protected internal override Grid ParseCore(string str)
	{
		if (!str.Contains('\t'))
		{
			return Grid.Undefined;
		}

		if (str.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not { Length: 9 } values)
		{
			return Grid.Undefined;
		}

		var sb = new StringBuilder(81);
		foreach (var value in values)
		{
			foreach (var digitString in value.Split('\t'))
			{
				sb.Append(string.IsNullOrEmpty(digitString) ? '.' : digitString[0]);
			}
		}
		return Grid.Parse(sb.ToString());
	}
}
