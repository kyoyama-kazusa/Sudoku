namespace Sudoku.Behaviors.Diff;

/// <summary>
/// Provides a way to analyze difference between two grids.
/// </summary>
public static class DiffAnalysis
{
	/// <summary>
	/// Try to analyze difference between two grids if they have a same initial grid (with same digits from given cells).
	/// </summary>
	/// <param name="left">Indicates the first grid to be checked.</param>
	/// <param name="right">Indicates the second grid to be checked.</param>
	/// <param name="result">A <see cref="DiffResult"/> result indicating the difference result.</param>
	/// <returns>A <see cref="bool"/> result indicating whether two grids have a same initial grid.</returns>
	/// <remarks>
	/// It's not necessary to check validity of two grids <paramref name="left"/> and <paramref name="right"/> in this method,
	/// but it might be failed to check in some difference cases; no exceptions will be thrown in this method,
	/// except for some severe errors should be reported.
	/// </remarks>
	public static bool TryAnalyzeDiff(in Grid left, in Grid right, [NotNullWhen(true)] out DiffResult? result)
	{
		// Determine whether they are totally same.
		if (Unsafe.AreSame(in left, in right) || left == right)
		{
			result = new NothingChangedDiffResult();
			return true;
		}

		// If not, determine whether the second grid ('right') can revert to the first grid ('left') by just resetting all digits.
		var (lr, rr) = (Grid.Parse(left.ToString()), Grid.Parse(right.ToString()));
		if (lr == right)
		{
			result = new ResetDiffResult();
			return true;
		}

		// If not, determine whether they are different in reset grid state.
		// Check whether they don't have a same reset grid. If so, we should check given cells or make a fast fail.
		if (lr != rr)
		{
			// Check changing status on given digits from two grids.
			var (leftResetGridGivens, rightResetGridGivens) = (left.GivenCells, right.GivenCells);
			if ((rightResetGridGivens & leftResetGridGivens) == leftResetGridGivens && leftResetGridGivens != rightResetGridGivens)
			{
				// The second grid ('right') hold all possible given digits with the first one ('left'),
				// with new givens added into the grid.
				var givenCandidates = CandidateMap.Empty;
				foreach (var cell in rightResetGridGivens & ~leftResetGridGivens)
				{
					givenCandidates.Add(cell * 9 + rr.GetDigit(cell));
				}

				// Check validity of the second puzzle.
				result = new AddGivenDiffResult(givenCandidates, right.Uniqueness != Uniqueness.Bad);
				return true;
			}
			else if ((leftResetGridGivens & rightResetGridGivens) == rightResetGridGivens && leftResetGridGivens != rightResetGridGivens)
			{
				// The first grid ('left') hold all possible given digits with the second one ('right'), with new givens.
				var givenCandidates = CandidateMap.Empty;
				foreach (var cell in leftResetGridGivens & ~rightResetGridGivens)
				{
					givenCandidates.Add(cell * 9 + lr.GetDigit(cell));
				}

				result = new RemoveGivenDiffResult(givenCandidates);
				return true;
			}
			else if (leftResetGridGivens == rightResetGridGivens)
			{
				var changedCandidates = CandidateMap.Empty;
				foreach (var cell in leftResetGridGivens)
				{
					if (left.GetDigit(cell) != right.GetDigit(cell))
					{
						changedCandidates.Add(cell * 9 + right.GetDigit(cell));
					}
				}

				result = new ChangedGivenDiffResult(changedCandidates);
				return true;
			}

			// Fast fail.
			goto ReturnFalse;
		}

		// If not, they have a same initial grid.
		// Now determine whether the second grid ('right') adds several modifiable digits from the first grid ('left').
		var (leftModifiables, rightModifiables) = (left.ModifiableCells, right.ModifiableCells);
		if ((rightModifiables & leftModifiables) == leftModifiables && leftModifiables != rightModifiables)
		{
			var modifiableCandidates = CandidateMap.Empty;
			foreach (var cell in rightModifiables & ~leftModifiables)
			{
				modifiableCandidates.Add(cell * 9 + right.GetDigit(cell));
			}

			// Check validity of the second puzzle.
			result = new AddModifiableDiffResult(modifiableCandidates, right.Uniqueness != Uniqueness.Bad);
			return true;
		}
		else if ((leftModifiables & rightModifiables) == rightModifiables && leftModifiables != rightModifiables)
		{
			var modifiableCandidates = CandidateMap.Empty;
			foreach (var cell in leftModifiables & ~rightModifiables)
			{
				modifiableCandidates.Add(cell * 9 + left.GetDigit(cell));
			}

			result = new RemoveModifiableDiffResult(modifiableCandidates);
			return true;
		}
		else if (leftModifiables == rightModifiables && !!leftModifiables)
		{
			var changedCandidates = CandidateMap.Empty;
			foreach (var cell in leftModifiables)
			{
				if (left.GetDigit(cell) != right.GetDigit(cell))
				{
					changedCandidates.Add(cell * 9 + right.GetDigit(cell));
				}
			}

			result = new ChangedModifiableDiffResult(changedCandidates);
			return true;
		}

		// If not, determine whether the second grid ('right') adds several candidates from the first grid ('left').
		// This requires two grids hold a same list of empty cells.
		var (leftEmptyCells, rightEmptyCells) = (left.EmptyCells, right.EmptyCells);
		if (leftEmptyCells == rightEmptyCells)
		{
			// Check candidate state.
			var addedCandidates = CandidateMap.Empty;
			var removedCandidates = CandidateMap.Empty;
			var changedCandidates = CandidateMap.Empty;
			foreach (var cell in leftEmptyCells)
			{
				var leftCandidates = left.GetCandidates(cell);
				var rightCandidates = right.GetCandidates(cell);
				if (leftCandidates == rightCandidates)
				{
					continue;
				}

				if ((rightCandidates & leftCandidates) == leftCandidates)
				{
					foreach (var digit in (Mask)(rightCandidates & ~leftCandidates))
					{
						addedCandidates.Add(cell * 9 + digit);
					}
				}
				else if ((leftCandidates & rightCandidates) == rightCandidates)
				{
					foreach (var digit in (Mask)(leftCandidates & ~rightCandidates))
					{
						removedCandidates.Add(cell * 9 + digit);
					}
				}
				else
				{
					foreach (var digit in rightCandidates)
					{
						changedCandidates.Add(cell * 9 + digit);
					}
				}
			}

			if (addedCandidates && ~removedCandidates && ~changedCandidates)
			{
				result = new AddCandidateDiffResult(addedCandidates, right.Uniqueness != Uniqueness.Bad);
				return true;
			}
			else if (~addedCandidates && removedCandidates && ~changedCandidates)
			{
				result = new RemoveCandidateDiffResult(removedCandidates);
				return true;
			}
			else if (changedCandidates)
			{
				result = new ChangedCandidateDiffResult(changedCandidates);
				return true;
			}
			else
			{
				goto ReturnFalse;
			}
		}

	ReturnFalse:
		// All previous branches are failed to be checked.
		result = null;
		return false;
	}
}
