namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a candidate argument parser.
/// </summary>
internal sealed class CandidateArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (Unsafe.IsNullRef(in grid))
		{
			goto ParseAsDefault;
		}

		if (tryParseAsCellMap(arguments, coordinateParser, out var mapResult))
		{
			var result = CandidateMap.Empty;
			foreach (var cell in mapResult)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					result.Add(cell * 9 + digit);
				}
			}
			return from candidate in result select new CandidateViewNode(colorIdentifier, candidate);
		}

	ParseAsDefault:
		return
			from candidate in new CandidateMap(arguments, coordinateParser)
			select new CandidateViewNode(colorIdentifier, candidate);


		static bool tryParseAsCellMap(ReadOnlySpan<string> arguments, CoordinateParser parser, out CellMap result)
		{
			result = CellMap.Empty;
			foreach (var argument in arguments)
			{
				try
				{
					// Try parse it as a candidate map.
					// If the target digits are not found, then parse it as cells.
					if (parser.CandidateParser(argument))
					{
						result = CellMap.Empty;
						return false;
					}
				}
				catch (FormatException)
				{
				}

				result |= parser.CellParser(argument);
			}
			return true;
		}
	}
}
