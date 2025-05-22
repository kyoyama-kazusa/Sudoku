namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Locked Candidates</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Pointing</item>
/// <item>Claiming</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_LockedCandidatesStepSearcher", Technique.Pointing, Technique.Claiming, IsCachingSafe = true)]
public sealed partial class LockedCandidatesStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes" />
	/// <para>
	/// The main idea of this searching operation:
	/// <code><![CDATA[
	/// .-------.-------.-------.
	/// | C C C | A A A | A A A |
	/// | B B B | . . . | . . . |
	/// | B B B | . . . | . . . |
	/// '-------'-------'-------'
	/// ]]></code>
	/// For example, if the cells C form a locked candidates, there'll be two cases:
	/// <list type="number">
	/// <item><b>Pointing (Type 1)</b>: Cells A contains the digit, but cells B doesn't.</item>
	/// <item><b>Claiming (Type 2)</b>: Cells B contains the digit, but cells A doesn't.</item>
	/// </list>
	/// </para>
	/// <para>Therefore, the algorithm is:</para>
	/// <para>
	/// Use <see cref="Mask"/>.<see langword="operator"/> |(<see cref="Mask"/>, <see cref="Mask"/>)
	/// to merge digits mask from cells A, cells B and cells C,
	/// and suppose the notation <c>a</c> is the mask result for cells A, <c>b</c> is the mask result for cells B,
	/// and <c>c</c> is the mask result for cells C.
	/// If the equation <c><![CDATA[(c & (a ^ b)) != 0]]></c> is correct, the locked candidates exists,
	/// and the result of the expression <c><![CDATA[c & (a ^ b)]]></c> is a mask that holds the digits of the locked candidates.
	/// </para>
	/// <para>
	/// Why this expression? <c>a ^ b</c> means the digit can only appear in either cells A or cells B.
	/// If both or neither, the digit won't contain the locked candidates pattern.
	/// Because of the optimization of the performance, we use the predefined table to iterate on
	/// all possible location where may form a locked candidates.
	/// </para>
	/// </remarks>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var emptyCellsForGrid = grid.EmptyCells;
		var candidatesMapForGrid = grid.CandidatesMap;
		foreach (var ((baseSet, coverSet), (a, b, c, _)) in Miniline.Map)
		{
			if (!LockedCandidates.IsLockedCandidates(grid, a, b, c, emptyCellsForGrid, out var m))
			{
				continue;
			}

			// Now iterate on the mask to get all digits.
			foreach (var digit in m)
			{
				ref readonly var candidatesMap = ref candidatesMapForGrid[digit];

				// Check whether the digit contains any eliminations.
				var (housesMask, elimMap) = a & candidatesMap
					? ((Mask)(coverSet << 8 | baseSet), a & candidatesMap)
					: ((Mask)(baseSet << 8 | coverSet), b & candidatesMap);
				if (!elimMap)
				{
					continue;
				}

				// Okay, now put the current step into the collection.
				var (realBaseSet, realCoverSet, intersection) = (housesMask >> 8 & 127, housesMask & 127, c & candidatesMap);
				if (c.Count < 2)
				{
					continue;
				}

				var step = new LockedCandidatesStep(
					(from cell in elimMap select new Conclusion(Elimination, cell, digit)).ToArray(),
					[
						[
							.. from cell in intersection select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
							new HouseViewNode(ColorIdentifier.Normal, realBaseSet),
							new HouseViewNode(ColorIdentifier.Auxiliary1, realCoverSet),
							.. Excluder.GetLockedCandidatesExcluders(grid, digit, realBaseSet, intersection)
						]
					],
					context.Options,
					digit,
					realBaseSet,
					realCoverSet
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}

		return null;
	}
}
