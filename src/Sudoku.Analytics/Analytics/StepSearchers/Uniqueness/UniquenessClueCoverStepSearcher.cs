namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Uniqueness Clue Cover</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Uniqueness Clue Cover</item>
/// </list>
/// </summary>
/// <seealso href="http://sudopedia.enjoysudoku.com/Uniqueness_Clue_Cover.html">Uniqueness Clue Cover</seealso>
/// <seealso href="http://forum.enjoysudoku.com/uniqueness-clue-cover-t40814.html#p342974">Serg's Description to UCC</seealso>
[StepSearcher(
	"StepSearcherName_UniquenessClueCoverStepSearcher",
	Technique.UniquenessClueCover,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class UniquenessClueCoverStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// The technique is very complex that I cannot enumerate all possible cases.
	/// See <see href="http://sudopedia.enjoysudoku.com/Uniqueness_Clue_Cover.html">this link</see> (Uniqueness Clue Cover - Sudopedia Mirror)
	/// and <see href="http://forum.enjoysudoku.com/uniqueness-clue-cover-t40814.html#p342974">this link</see> (Serg's Description to UCC)
	/// to learn about this technique.
	/// </remarks>
	/// <example>
	/// To-do list:
	/// <code><![CDATA[
	/// ..+1.....+9.....+9..1.+9...1..2+349.5..2..+1632.9+4552.+9.43..1+84+79...39+6....7.8.5.8.6.9+4:711 716 816 721 323 723 731 333 733 435 635 835 538 638 838 647 765 865 668 768
	/// ]]></code>
	/// </example>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		foreach (var (chuteIndex, isRow, _) in Chutes)
		{
			ref readonly var chute = ref ChuteMaps[chuteIndex];
			if ((chute & ~EmptyCells) is not [var c1, var c2] valueCells)
			{
				// The number of the value cells must be 2 for this type.
				continue;
			}

			if (PeersMap[c1].Contains(c2) || PeersMap[c2].Contains(c1))
			{
				// The two value cells cannot lie in a same house.
				continue;
			}

			var (c1Digit, c2Digit) = (grid.GetDigit(c1), grid.GetDigit(c2));
			if (c1Digit == c2Digit)
			{
				// The two value cells cannot be filled with a same digit.
				continue;
			}

			var digitsMask = (Mask)(1 << c1Digit | 1 << c2Digit);
			var elimHouseType = isRow ? HouseType.Column : HouseType.Row;
			var excludedHouseType = isRow ? HouseType.Row : HouseType.Column;
			var excludedLines = HousesMap[c1.ToHouse(excludedHouseType)] | HousesMap[c2.ToHouse(excludedHouseType)];
			var conclusions = new List<Conclusion>(2);
			foreach (var elimCell in chute & ~excludedLines & (HousesMap[c1.ToHouse(elimHouseType)] | HousesMap[c2.ToHouse(elimHouseType)]))
			{
				var correspondingValueCell = (HousesMap[elimCell.ToHouse(elimHouseType)] & chute & valueCells)[0];
				var elimDigit = BitOperations.TrailingZeroCount((Mask)(digitsMask & ~(1 << grid.GetDigit(correspondingValueCell))));
				if (CandidatesMap[elimDigit].Contains(elimCell))
				{
					conclusions.Add(new(Assignment, elimCell, elimDigit));
				}
			}
			if (conclusions.Count == 0)
			{
				// No eliminations can be found.
				continue;
			}

			var step = new UniquenessClueCoverStep(
				conclusions.AsMemory(),
				[
					[
						new ChuteViewNode(ColorIdentifier.Normal, chuteIndex),
						new CellViewNode(ColorIdentifier.Normal, c1),
						new CellViewNode(ColorIdentifier.Normal, c2)
					]
				],
				context.Options,
				c1.AsCellMap() + c2,
				(Mask)(1 << c1Digit | 1 << c2Digit),
				chuteIndex
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}
}
