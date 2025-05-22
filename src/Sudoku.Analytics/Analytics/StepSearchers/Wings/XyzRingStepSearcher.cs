namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with an <b>XYZ-Ring</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Non-Nice XYZ-Ring</item>
/// <item>Nice XYZ-Ring</item>
/// <item>Grouped Non-Nice XYZ-Ring</item>
/// <item>Grouped Nice XYZ-Ring</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_XyzRingStepSearcher",
	Technique.XyzLoop, Technique.XyzNiceLoop, Technique.GroupedXyzLoop, Technique.GroupedXyzNiceLoop,
	Technique.SiameseXyzLoop, Technique.SiameseXyzNiceLoop, Technique.SiameseGroupedXyzLoop, Technique.SiameseGroupedXyzNiceLoop)]
public sealed partial class XyzRingStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the step searcher allows searching for Siamese XYZ-Rings.
	/// </summary>
	[SettingItemName(SettingItemNames.AllowSiameseXyzRing)]
	public bool AllowSiamese { get; set; }


	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// The pattern is formed by an XYZ-Wing and a conjugate pair.
	/// <code><![CDATA[
	/// abc.  .  | ab .  .
	/// .  .  .  | .  .  .
	/// .  .  .  | .  .  .
	/// ---------+--------
	/// ac .  .  | .  .  .
	/// .  .  .  | .  .  .
	/// .  .  a--+-a  .  .
	/// ]]></code>
	/// Or
	/// <code><![CDATA[
	/// abc.  .  | ab .  .
	/// .  .  .  | .  .  .
	/// .  .  .  | .  .  .
	/// ---------+--------
	/// ac .  .  | .  .  .
	/// .  .  .  | .  .  .
	/// a--------+-a  .  .
	/// ]]></code>
	/// </remarks>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var accumulator = new HashSet<XyzRingStep>();
		CollectCore(accumulator, grid, context);

		if (accumulator.Count == 0)
		{
			return null;
		}

		// Check for Siamese XYZ-Rings.
		var siameses = AllowSiamese ? Siamese.XyzRing.GetSiamese(accumulator, grid) : [];
		if (context.OnlyFindOne)
		{
			return siameses is [var siamese, ..] ? siamese : accumulator.FirstOrDefault() is { } normal ? normal : null;
		}

		foreach (var step in siameses)
		{
			context.Accumulator.Add(step);
		}
		if (accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;
	}

	/// <summary>
	/// The core method to collect steps.
	/// </summary>
	/// <param name="accumulator">The accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	private void CollectCore(HashSet<XyzRingStep> accumulator, in Grid grid, in StepAnalysisContext context)
	{
		// The pattern starts with a tri-value cell, so check for it.
		var trivalueCells = CellMap.Empty;
		foreach (var cell in EmptyCells & ~BivalueCells)
		{
			if (BitOperations.PopCount(grid.GetCandidates(cell)) == 3)
			{
				trivalueCells.Add(cell);
			}
		}

		// Iterate on patterns found.
		foreach (var (pivot, leafCell1, leafCell2, house1, house2, unionDigitsMask, zDigit) in new XyzWingPatternSearcher().Search(grid))
		{
			var digitsMaskPivot = grid.GetCandidates(pivot);
			var digitsMask1 = grid.GetCandidates(leafCell1);
			var digitsMask2 = grid.GetCandidates(leafCell2);
			var theOtherTwoDigitsMask = (Mask)(unionDigitsMask & ~(1 << zDigit));
			var theOtherDigit1 = BitOperations.TrailingZeroCount(theOtherTwoDigitsMask);
			var theOtherDigit2 = theOtherTwoDigitsMask.GetNextSet(theOtherDigit1);
			var coveringHouseForDigit1 = (digitsMask1 >> theOtherDigit1 & 1) != 0 ? house1 : house2;
			var coveringHouseForDigit2 = house1 == coveringHouseForDigit1 ? house2 : house1;

			// Iterate on all houses, determining whether two leaf cells can make the house
			// out of filling with intersected digit.
			for (var conflictedHouse = 0; conflictedHouse < 27; conflictedHouse++)
			{
				var cellsShouldBeCovered = HousesMap[conflictedHouse] & CandidatesMap[zDigit];
				if (!cellsShouldBeCovered || !!(cellsShouldBeCovered & [pivot, leafCell1, leafCell2]))
				{
					continue;
				}

				if (((leafCell1.AsCellMap() + leafCell2).ExpandedPeers & cellsShouldBeCovered) != cellsShouldBeCovered)
				{
					continue;
				}

				// An XYZ-Ring is formed. Now check for eliminations.
				var patternCells = cellsShouldBeCovered + pivot + leafCell1 + leafCell2;
				var conclusions = new List<Conclusion>();

				// Elimination zone 1: Sharing house for pivot and leaf cell 1 -> eliminate digit they both hold
				// (digits not intersect).
				foreach (var cell in
					HousesMap[coveringHouseForDigit1] & CandidatesMap[theOtherDigit1] & ~patternCells & ~cellsShouldBeCovered)
				{
					conclusions.Add(new(Elimination, cell, theOtherDigit1));
				}

				// Elimination zone 2: Sharing house for pivot and leaf cell 2 -> eliminate digit they both hold
				// (digits not intersect).
				foreach (var cell in
					HousesMap[coveringHouseForDigit2] & CandidatesMap[theOtherDigit2] & ~patternCells & ~cellsShouldBeCovered)
				{
					conclusions.Add(new(Elimination, cell, theOtherDigit2));
				}

				var isNice = false;
				foreach (var (leaf, theOtherLeaf) in ((leafCell1, leafCell2), (leafCell2, leafCell1)))
				{
					var linkCellsIntersect = cellsShouldBeCovered & PeersMap[leaf];
					foreach (var linkCellHouse in linkCellsIntersect.SharedHouses)
					{
						foreach (var leafCellHouse in (pivot.AsCellMap() + leaf).SharedHouses)
						{
							if (linkCellHouse.HouseType == HouseType.Block ^ leafCellHouse.HouseType == HouseType.Block
								&& (HousesMap[linkCellHouse] & HousesMap[leafCellHouse]) is var i)
							{
								// Elimination zone 3: Intersected cell for the leaf and one grouped node of cells
								// in (grouped) strong link that they share in a same mini-line -> eliminate intersected digit.
								foreach (var cell in i & CandidatesMap[zDigit] & ~patternCells & ~cellsShouldBeCovered)
								{
									conclusions.Add(new(Elimination, cell, zDigit));
								}
							}

							// Check whether loop is nice.
							if ((HousesMap[leafCellHouse] & linkCellsIntersect) == linkCellsIntersect)
							{
								isNice = true;

								// Elimination zone 4 and 5: shared houses for leaf and (grouped) strong link nodes.
								foreach (var cell in
									((HousesMap[leafCellHouse] & CandidatesMap[zDigit]) - pivot & ~cellsShouldBeCovered) - leaf)
								{
									conclusions.Add(new(Elimination, cell, zDigit));
								}

								var lastCellsToCheck = (cellsShouldBeCovered & ~linkCellsIntersect) + theOtherLeaf;
								foreach (var house in lastCellsToCheck.SharedHouses)
								{
									foreach (var cell in HousesMap[house] & CandidatesMap[zDigit] & ~lastCellsToCheck)
									{
										conclusions.Add(new(Elimination, cell, zDigit));
									}
								}
							}
						}
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				accumulator.Add(
					new(
						conclusions.AsMemory(),
						[
							[
								..
								from digit in digitsMaskPivot
								let colorIdentifier = digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal
								select new CandidateViewNode(colorIdentifier, pivot * 9 + digit),
								..
								from digit in digitsMask1
								let colorIdentifier = digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal
								select new CandidateViewNode(colorIdentifier, leafCell1 * 9 + digit),
								..
								from digit in digitsMask2
								let colorIdentifier = digit == zDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal
								select new CandidateViewNode(colorIdentifier, leafCell2 * 9 + digit),
								..
								from cell in cellsShouldBeCovered
								select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + zDigit),
							]
						],
						context.Options,
						zDigit,
						pivot,
						leafCell1,
						leafCell2,
						grid[[pivot, leafCell1, leafCell2]],
						1 << conflictedHouse,
						isNice,
						cellsShouldBeCovered.Count > 2
					)
				);
			}
		}
	}
}
