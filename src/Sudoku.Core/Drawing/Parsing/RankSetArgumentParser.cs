namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents rank set argument parser.
/// </summary>
internal closed class RankSetArgumentParser : ArgumentParser
{
	/// <summary>
	/// Indicates whether the parser is truth-based.
	/// </summary>
	public abstract bool IsTruthBased { get; }


	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorDescriptor colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (Unsafe.IsNullRef(in grid))
		{
			throw new FormatException();
		}

		var parser = coordinateParser as RxCyParser ?? new();
		var result = new List<ViewNode>();
		var spaceSets = SpaceSet.Empty;
		foreach (var argument in arguments)
		{
			spaceSets |= parser.SpaceParser(argument);
		}

		foreach (var space in spaceSets)
		{
			foreach (var candidate in space.GetAvailableRange(grid))
			{
				result.Add(new CandidateViewNode(colorIdentifier, candidate));
			}

			result.Add(
				IsTruthBased
					? new TruthSpaceViewNode(colorIdentifier, space)
					: new LinkSpaceViewNode(colorIdentifier, space)
			);
		}
		return result.AsSpan();
	}
}
