namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with an <b>Almost Locked Candidates</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Basic types:
/// <list type="bullet">
/// <item>Almost Locked Pair</item>
/// <item>Almost Locked Triple</item>
/// </list>
/// </item>
/// <item>
/// Extended types (contains value cells):
/// <list type="bullet">
/// <item>Extended Almost Locked Triple</item>
/// <item>Extended Almost Locked Quadruple</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_AlmostLockedCandidatesStepSearcher",
	Technique.AlmostLockedPair, Technique.AlmostLockedTriple, Technique.AlmostLockedQuadruple,
	Technique.AlmostLockedTripleValueType, Technique.AlmostLockedQuadrupleValueType)]
public sealed partial class AlmostLockedCandidatesStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the searcher checks the almost locked quadruple.
	/// </summary>
	[SettingItemName(SettingItemNames.CheckAlmostLockedQuadruple)]
	public bool CheckAlmostLockedQuadruple { get; set; }

	/// <summary>
	/// Indicates whether the searcher checks for value types.
	/// </summary>
	[SettingItemName(SettingItemNames.AlmostLockedCandidatesCheckValueTypes)]
	public bool CheckValueTypes { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		foreach (var checkValueCells in (false, true))
		{
			if (CheckValueTypes ^ checkValueCells)
			{
				// They should hold a same state.
				continue;
			}

			for (var size = 2; size <= (CheckAlmostLockedQuadruple && checkValueCells ? 4 : 3); size++)
			{
				foreach (var ((baseSet, coverSet), (a, b, c, _)) in Miniline.Map)
				{
					if (c && EmptyCells)
					{
						if (Collect(ref context, size, baseSet, coverSet, a, b, c, checkValueCells) is { } step1)
						{
							return step1;
						}
						if (Collect(ref context, size, coverSet, baseSet, b, a, c, checkValueCells) is { } step2)
						{
							return step2;
						}
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// <inheritdoc cref="Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="size">The size.</param>
	/// <param name="baseSet">The base set.</param>
	/// <param name="coverSet">The cover set.</param>
	/// <param name="a">The left grid map.</param>
	/// <param name="b">The right grid map.</param>
	/// <param name="c">The intersection.</param>
	/// <param name="checkValueCells">Indicates whether the method checks for value cells.</param>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// <para>
	/// The diagrams:
	/// <code><![CDATA[
	/// ALP:
	/// abx aby | ab
	/// abz     |
	///
	/// ALT:
	/// abcw abcx | abc abc
	/// abcy abcz |
	/// ]]></code>
	/// </para>
	/// <para>Algorithm:</para>
	/// <para>
	/// If the cell <c>ab</c> (in ALP) or <c>abc</c> (in ALT) is filled with the digit <c>p</c>,
	/// then the cells <c>abx</c> and <c>aby</c> (in ALP) and <c>abcw</c> and <c>abcx</c> (in ALT) can't
	/// fill the digit <c>p</c>. Therefore the digit <c>p</c> can only be filled into the left-side block.
	/// </para>
	/// <para>
	/// If the block only contains those cells that can contain the digit <c>p</c>, the ALP or ALT will be formed,
	/// and the elimination is <c>z</c> (in ALP) and <c>y</c> and <c>z</c> (in ALT).
	/// </para>
	/// </remarks>
	private static AlmostLockedCandidatesStep? Collect(
		ref StepAnalysisContext context,
		int size,
		House baseSet,
		House coverSet,
		in CellMap a,
		in CellMap b,
		in CellMap c,
		bool checkValueCells
	)
	{
		var characters = context.Options.BabaGroupInitialLetter.GetSequence(context.Options.BabaGroupLetterCasing);

		ref readonly var grid = ref context.Grid;

		// Iterate on each cell combination.
		foreach (ref readonly var alsCells in (checkValueCells ? a : a & EmptyCells) & size - 1)
		{
			// Collect the mask. The cell combination must contain the specified number of digits.
			var mask = grid[alsCells, checkValueCells];
			if (BitOperations.PopCount(mask) != size)
			{
				continue;
			}

			// Check whether overlapped.
			var isOverlapped = false;
			foreach (var digit in mask)
			{
				if (ValuesMap[digit] & HousesMap[coverSet])
				{
					isOverlapped = true;
					break;
				}
			}
			if (isOverlapped)
			{
				continue;
			}

			// Then check whether the another house (left-side block in those diagrams)
			// forms an AHS (i.e. those digits must appear in the specified cells).
			var ahsMask = (Mask)0;
			foreach (var digit in mask)
			{
				ahsMask |= (HousesMap[coverSet] & CandidatesMap[digit] & b) / coverSet;
			}
			if (BitOperations.PopCount(ahsMask) != size - 1)
			{
				continue;
			}

			// Gather the AHS cells.
			var ahsCells = CellMap.Empty;
			foreach (var pos in ahsMask)
			{
				ahsCells.Add(HousesCells[coverSet][pos]);
			}

			// Value cells checker.
			var valueCells = (alsCells | ahsCells) & ~EmptyCells;
			if (!checkValueCells && !!valueCells)
			{
				// Value cells must be empty if we don't check value cells.
				// However, we don't check anything if the existence of value cells is allowed.
				continue;
			}

			// Collect all eliminations.
			var conclusions = new List<Conclusion>();
			foreach (var aCell in a)
			{
				if (!alsCells.Contains(aCell))
				{
					foreach (var digit in (Mask)(mask & grid.GetCandidates(aCell)))
					{
						conclusions.Add(new(Elimination, aCell, digit));
					}
				}
			}
			foreach (var digit in (Mask)(Grid.MaxCandidatesMask & ~mask))
			{
				foreach (var ahsCell in ahsCells & CandidatesMap[digit])
				{
					conclusions.Add(new(Elimination, ahsCell, digit));
				}
			}

			// Check whether any eliminations exists.
			if (conclusions.Count == 0)
			{
				continue;
			}

			// Gather highlight candidates.
			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var digit in mask)
			{
				foreach (var cell in alsCells & CandidatesMap[digit])
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
				}
			}
			foreach (var cell in c)
			{
				foreach (var digit in (Mask)(mask & grid.GetCandidates(cell)))
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
				}
			}
			foreach (var cell in ahsCells)
			{
				foreach (var digit in (Mask)(mask & grid.GetCandidates(cell)))
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
				}
			}

			var babaGroupingNodes = new List<BabaGroupViewNode>(alsCells.Count + ahsCells.Count);
			var (characterIndexAhs, characterIndexAls) = (0, 0);
			foreach (var cell in alsCells)
			{
				babaGroupingNodes.Add(new(cell, characters[characterIndexAhs++], grid.GetCandidates(cell)));
			}
			foreach (var cell in ahsCells)
			{
				babaGroupingNodes.Add(new(cell, characters[characterIndexAls++], grid.GetCandidates(cell)));
			}
			foreach (var cell in c & EmptyCells)
			{
				babaGroupingNodes.Add(new(cell, characters[size - 1], grid.GetCandidates(cell)));
			}

			var valueCellNodes = from cell in valueCells select new CellViewNode(ColorIdentifier.Normal, cell);
			var step = new AlmostLockedCandidatesStep(
				conclusions.AsMemory(),
				[
					[
						.. valueCellNodes,
						.. candidateOffsets,
						new HouseViewNode(ColorIdentifier.Normal, baseSet),
						new HouseViewNode(ColorIdentifier.Auxiliary2, coverSet)
					],
					[
						.. valueCellNodes,
						.. babaGroupingNodes,
						new HouseViewNode(ColorIdentifier.Normal, baseSet),
						new HouseViewNode(ColorIdentifier.Auxiliary2, coverSet)
					]
				],
				context.Options,
				mask,
				alsCells,
				ahsCells,
				valueCellNodes.Length != 0
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
