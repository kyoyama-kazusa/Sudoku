namespace Sudoku.Analytics.StepSearchers.SingleDigitPatterns;

/// <summary>
/// Provides with an <b>Empty Rectangle</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Empty Rectangle</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_EmptyRectangleStepSearcher", Technique.EmptyRectangle)]
public sealed partial class EmptyRectangleStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates all houses iterating on the specified block forming an empty rectangle.
	/// </summary>
	private static readonly House[][] EmptyRectangleLinkIds = [
		[12, 13, 14, 15, 16, 17, 21, 22, 23, 24, 25, 26],
		[12, 13, 14, 15, 16, 17, 18, 19, 20, 24, 25, 26],
		[12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23],
		[9, 10, 11, 15, 16, 17, 21, 22, 23, 24, 25, 26],
		[9, 10, 11, 15, 16, 17, 18, 19, 20, 24, 25, 26],
		[9, 10, 11, 15, 16, 17, 18, 19, 20, 21, 22, 23],
		[9, 10, 11, 12, 13, 14, 21, 22, 23, 24, 25, 26],
		[9, 10, 11, 12, 13, 14, 18, 19, 20, 24, 25, 26],
		[9, 10, 11, 12, 13, 14, 18, 19, 20, 21, 22, 23]
	];


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		for (var digit = 0; digit < 9; digit++)
		{
			for (var block = 0; block < 9; block++)
			{
				// Check the empty rectangle occupies more than 2 cells.
				// and the pattern forms an empty rectangle.
				var erMap = CandidatesMap[digit] & HousesMap[block];
				if (erMap.Count < 2 || !IsEmptyRectangle(erMap, block, out var row, out var column))
				{
					continue;
				}

				// Search for conjugate pair.
				for (var i = 0; i < 12; i++)
				{
					var linkMap = CandidatesMap[digit] & HousesMap[EmptyRectangleLinkIds[block][i]];
					if (linkMap.Count != 2)
					{
						continue;
					}

					var blockMask = linkMap.BlockMask;
					if (BitOperations.IsPow2(blockMask) || i < 6 && !(linkMap & HousesMap[column]) || i >= 6 && !(linkMap & HousesMap[row]))
					{
						continue;
					}

					var t = (linkMap & ~HousesMap[i < 6 ? column : row])[0];
					var elimHouse = i < 6 ? t % 9 + 18 : t / 9 + 9;
					if ((CandidatesMap[digit] & HousesMap[elimHouse] & HousesMap[i < 6 ? row : column]) is not [var elimCell, ..])
					{
						continue;
					}

					if (!CandidatesMap[digit].Contains(elimCell))
					{
						continue;
					}

					// Gather all highlight candidates.
					var candidateOffsets = new List<CandidateViewNode>();
					var cpCells = new List<Cell>(2);
					foreach (var cell in HousesMap[block] & CandidatesMap[digit])
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
					foreach (var cell in linkMap)
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						cpCells.Add(cell);
					}

					var step = new EmptyRectangleStep(
						new SingletonArray<Conclusion>(new(Elimination, elimCell, digit)),
						[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, block)]],
						context.Options,
						digit,
						block,
						new(cpCells[0], cpCells[1], digit)
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}


	/// <summary>
	/// Determine whether the specified cells in the specified block form an empty rectangle.
	/// </summary>
	/// <param name="cells">The cells to be checked.</param>
	/// <param name="block">The block where the cells may form an empty rectangle pattern.</param>
	/// <param name="row">The row that the empty rectangle used.</param>
	/// <param name="column">The column that the empty rectangle used.</param>
	/// <returns>
	/// A <see cref="bool"/> value indicating that. If <see langword="true"/>,
	/// both arguments <paramref name="row"/> and <paramref name="column"/> can be used;
	/// otherwise, both arguments should be discards.
	/// </returns>
	private static bool IsEmptyRectangle(in CellMap cells, House block, out House row, out House column)
	{
		var (r, c) = (block / 3 * 3 + 9, block % 3 * 3 + 18);
		for (var (i, count) = (r, 0); i < r + 3; i++)
		{
			if (!!(cells & HousesMap[i]) || ++count <= 1)
			{
				continue;
			}

			row = column = -1;
			return false;
		}

		for (var (i, count) = (c, 0); i < c + 3; i++)
		{
			if (!!(cells & HousesMap[i]) || ++count <= 1)
			{
				continue;
			}

			row = column = -1;
			return false;
		}

		for (var i = r; i < r + 3; i++)
		{
			for (var j = c; j < c + 3; j++)
			{
				if (cells & ~(HousesMap[i] | HousesMap[j]))
				{
					continue;
				}

				row = i;
				column = j;
				return true;
			}
		}

		row = column = -1;
		return false;
	}
}
