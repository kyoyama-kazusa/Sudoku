namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Bowman's Bingo</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Bowman's Bingo</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_BowmanBingoStepSearcher", Technique.BowmanBingo)]
public sealed partial class BowmanBingoStepSearcher : StepSearcher
{
	/// <summary>
	/// The singles searcher.
	/// </summary>
	private static readonly SingleStepSearcher SinglesSearcher = new() { EnableFullHouse = true, EnableLastDigit = true };


	/// <summary>
	/// All temporary conclusions.
	/// </summary>
	private readonly List<Conclusion> _tempConclusions = [];


	/// <summary>
	/// Indicates the maximum length of the bowman bingo you want to search for. The maximum possible value is 64 (81 - 17).
	/// The default value is 32.
	/// </summary>
	[SettingItemName(SettingItemNames.BowmanBingoMaxLength)]
	public int MaxLength { get; set; } = 32;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var tempAccumulator = new List<BowmanBingoStep>();
		ref readonly var grid = ref context.Grid;
		var tempGrid = grid;
		for (var digit = 0; digit < 9; digit++)
		{
			foreach (var cell in CandidatesMap[digit])
			{
				_tempConclusions.Add(new(Assignment, cell, digit));
				var pair = RecordUndoInfo(tempGrid, cell, digit);
				ref readonly var map = ref pair.Candidates;
				var mask = pair.Mask;

				// Try to fill this cell.
				tempGrid.SetDigit(cell, digit);
				var startCandidate = cell * 9 + digit;

				if (IsValidGrid(grid, cell))
				{
					Collect(tempAccumulator, ref tempGrid, ref context, startCandidate, MaxLength - 1);
				}
				else
				{
					tempAccumulator.Add(
						new(
							new SingletonArray<Conclusion>(new(Elimination, startCandidate)),
							[
								[
									.. from p in _tempConclusions select new CandidateViewNode(ColorIdentifier.Normal, p.Candidate),
									.. GetLinks()
								]
							],
							context.Options,
							[.. _tempConclusions]
						)
					);
				}

				// Undo the operation.
				_tempConclusions.RemoveAt(^1);
				UndoGrid(ref tempGrid, map, cell, mask);
			}
		}

		if (!context.OnlyFindOne)
		{
			context.Accumulator.AddRange(
				from info in tempAccumulator.AsSpan()
				orderby info.ContradictionLinks.Length, info.ContradictionLinks[0]
				select (Step)info
			);
		}
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="result">The accumulator.</param>
	/// <param name="grid">The sudoku grid to be checked.</param>
	/// <param name="context">The context.</param>
	/// <param name="startCand">The start candidate to be assumed.</param>
	/// <param name="length">The whole length to be searched.</param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private BowmanBingoStep? Collect(
		List<BowmanBingoStep> result,
		ref Grid grid,
		ref StepAnalysisContext context,
		Candidate startCand,
		int length
	)
	{
		var context2 = new StepAnalysisContext(grid) { OnlyFindOne = true, Options = context.Options };
		if (length == 0 || SinglesSearcher.Collect(ref context2) is not SingleStep { Conclusions.Span: [{ Cell: var c, Digit: var d } conclusion, ..] })
		{
			// Two cases we don't need to go on.
			// Case 1: The variable 'length' is 0.
			// Case 2: The searcher can't get any new steps, which means the expression
			// always returns the value null. Therefore, this case (grid[cell] = digit) is a bad try.
			goto ReturnNull;
		}

		// Try to fill.
		_tempConclusions.Add(conclusion);
		var pair = RecordUndoInfo(grid, c, d);
		ref readonly var map = ref pair.Candidates;
		var mask = pair.Mask;

		grid.SetDigit(c, d);
		if (IsValidGrid(grid, c))
		{
			// Sounds good.
			if (Collect(result, ref grid, ref context2, startCand, length - 1) is { } nestedStep)
			{
				return nestedStep;
			}
		}
		else
		{
			var candidateOffsets = new CandidateViewNode[_tempConclusions.Count];
			var i = 0;
			foreach (var (_, candidate) in _tempConclusions)
			{
				candidateOffsets[i++] = new(ColorIdentifier.Normal, candidate);
			}

			var step = new BowmanBingoStep(
				new SingletonArray<Conclusion>(new(Elimination, startCand)),
				[[.. candidateOffsets, .. GetLinks()]],
				context.Options,
				[.. _tempConclusions]
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			result.Add(step);
		}

		// Undo grid.
		_tempConclusions.RemoveAt(_tempConclusions.Count - 1);
		UndoGrid(ref grid, map, c, mask);

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Get links.
	/// </summary>
	/// <returns>The links.</returns>
	private List<ChainLinkViewNode> GetLinks()
	{
		var result = new List<ChainLinkViewNode>();
		for (var i = 0; i < _tempConclusions.Count - 1; i++)
		{
			var c1 = _tempConclusions[i].Candidate;
			var c2 = _tempConclusions[i + 1].Candidate;
			result.Add(new(ColorIdentifier.Normal, c1.AsCandidateMap(), c2.AsCandidateMap(), false));
		}
		return result;
	}


	/// <summary>
	/// Record all information to be used in undo grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>The result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (CandidateMap Candidates, Mask Mask) RecordUndoInfo(in Grid grid, Cell cell, Digit digit)
		=> ((from c in PeersMap[cell] & CandidatesMap[digit] select c * 9 + digit).AsCandidateMap(), grid[cell]);

	/// <summary>
	/// Undo the grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="list">The list.</param>
	/// <param name="cell">The cell.</param>
	/// <param name="mask">The mask.</param>
	private static void UndoGrid(ref Grid grid, in CandidateMap list, Cell cell, Mask mask)
	{
		foreach (var candidate in list)
		{
			grid.SetExistence(candidate / 9, candidate % 9, true);
		}
		grid.SetMask(cell, mask);
	}

	/// <summary>
	/// To check the specified cell has a same digit filled in a cell
	/// which is same house with the current one.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The result.</returns>
	private static bool IsValidGrid(in Grid grid, Cell cell)
	{
		var result = true;
		foreach (var peerCell in PeersMap[cell])
		{
			var state = grid.GetState(peerCell);
			if ((state != CellState.Empty && grid.GetDigit(peerCell) != grid.GetDigit(cell) || state == CellState.Empty)
				&& grid.GetCandidates(peerCell) != 0)
			{
				continue;
			}

			result = false;
			break;
		}
		return result;
	}
}
