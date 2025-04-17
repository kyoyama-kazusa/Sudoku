namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Represents a <see cref="GridFormatInfo{TGrid}"/> type that supports inline Susser grid formatting.
/// </summary>
public sealed partial class InlineSusserGridFormatInfo : GridFormatInfo<Grid>
{
	/// <summary>
	/// Indicates the plus token that describes for modifiable values.
	/// </summary>
	private const string PlusToken = "+";

	/// <summary>
	/// Indicates the empty string.
	/// </summary>
	private const string EmptyString = "";


	[GeneratedRegex("""(\+?[\d\.]|\[[1-9]{1,9}\])""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridSusserPattern { get; }


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override IFormatProvider? GetFormat(Type? formatType) => formatType == typeof(GridFormatInfo<Grid>) ? this : null;

	/// <inheritdoc/>
	public override InlineSusserGridFormatInfo Clone() => new() { NegateEliminationsTripletRule = NegateEliminationsTripletRule };

	/// <inheritdoc/>
	protected internal override string FormatCore(in Grid grid)
	{
		var sb = new StringBuilder();
		for (var cell = 0; cell < 81; cell++)
		{
			switch (grid.GetState(cell))
			{
				case CellState.Empty:
				{
					var digitsMask = NegateEliminationsTripletRule ? (Mask)0 : grid.GetCandidates(cell);
					sb.Append(digitsMask == 0 ? "0" : $"[{new(from digit in digitsMask.AllSets select (char)(digit + '1'))}]");
					break;
				}
				case CellState.Modifiable:
				{
					sb.Append($"+{grid.GetDigit(cell) + 1}");
					break;
				}
				case CellState.Given:
				{
					sb.Append(grid.GetDigit(cell) + 1);
					break;
				}
				default:
				{
					throw new FormatException();
				}
			}
		}
		return sb.ToString();
	}

	/// <inheritdoc/>
	protected internal override Grid ParseCore(string str)
	{
		var match = GridSusserPattern.Matches(str);
		if (match is not { Count: 81 } captures)
		{
			return Grid.Undefined;
		}

		var result = Grid.Empty;
		for (var cell = 0; cell < 81; cell++)
		{
			switch (captures[cell].Value)
			{
				case [.. var token, var digitChar and >= '1' and <= '9']:
				{
					var state = token switch { PlusToken => CellState.Modifiable, EmptyString => CellState.Given, _ => default };
					if (state is not (CellState.Given or CellState.Modifiable))
					{
						return Grid.Undefined;
					}

					var digit = digitChar - '1';
					result.SetDigit(cell, digit);
					result.SetState(cell, state);
					break;
				}
				case ['0' or '.']:
				{
					continue;
				}
				case ['[', .. { Length: <= 9 } digitsStr, ']']:
				{
					var digits = MaskOperations.Create(from c in digitsStr select c - '1');
					if (!NegateEliminationsTripletRule)
					{
						// This applies for normal rule - removing candidates marked.
						foreach (var digit in digits)
						{
							// Set the candidate with false to eliminate the candidate.
							result.SetExistence(cell, digit, false);
						}
					}
					else
					{
						// If negate candidates, we should remove all possible candidates from all empty cells, making the grid invalid firstly.
						// Then we should add candidates onto the grid to make the grid valid.
						result[cell] = (Mask)(Grid.EmptyMask | digits);
					}
					break;
				}
				default:
				{
					return Grid.Undefined;
				}
			}
		}
		return result;
	}
}
