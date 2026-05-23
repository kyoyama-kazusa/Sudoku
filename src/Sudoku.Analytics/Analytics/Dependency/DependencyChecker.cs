namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents a checker that analyzes dependency of a grid,
/// to know how to conclude assignment <c>b</c> from the specified assignment <c>a</c>.
/// </summary>
public static class DependencyChecker
{
	/// <summary>
	/// Try to analyze dependency spaces (truth and link) in the grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="previous">The previous assignment.</param>
	/// <param name="current">The current assignment.</param>
	/// <param name="truth">Indicates the truth inferred.</param>
	/// <param name="link">Indicates the link inferred.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool TryGetDependencySpaces(
		in Grid grid,
		in DependencyAssignment previous,
		in DependencyAssignment current,
		out Space truth,
		out Space link
	)
	{
		// Just check for such cases, inferring them by checking satisfiability of such patterns. Here're the cases:
		// .-------------.-------------------------------.-------------------------------.-----------------------------.
		// |     \\Cases | Case 1:                       | Case 2:                       | Case 3:                     |
		// | Types\\     |   Cell link + House truth     |   House link + House truth    |   House link + Cell truth   |
		// :-------------+-------------------------------+-------------------------------+-----------------------------:
		// |             | r1c1 = 1                      | r1c1 = 1                      | r1c1 = 1                    |
		// | Non-grouped |   => r1c1 <> 2 (cell link)    |   => r4c1 <> 1 (house link)   |   => r1c2 <> 1 (house link) |
		// |             |   => r1c2 = 2 (house truth)   |   => r4c4 = 1 (house truth)   |   => r1c2 = 2 (cell truth)  |
		// :-------------+-------------------------------+-------------------------------+-----------------------------:
		// |             | r1c1 = 1                      | r1c123 = 1                    | r1c123 = 1                  |
		// | Grouped     |   => r1c1 <> 2 (cell link)    |   => r123c2 <> 1 (house link) |   => r1c7 <> 1 (house link) |
		// |             |   => r1c789 = 2 (house truth) |   => r789c2 = 1 (house truth) |   => r1c7 = 2 (cell truth)  |
		// '-------------'-------------------------------'-------------------------------'-----------------------------'

		var (emptyCells, (previousDigit, previousCells), (currentDigit, currentCells)) = (grid.EmptyCells, previous, current);

		// Check shared houses for current assignment, trying to check which house covers previous assignment.
		// If does, a hidden single can be inferred.
		foreach (var house in currentCells.SharedHouses)
		{
			// All previously-assigned cells should be inside the shared house.
			if ((HousesMap[house] & previousCells) == previousCells) { goto Case1; } else { goto Case2; }

		Case1:
			// May be case 1.
			if (previousDigit == currentDigit)
			{
				// Same digit, same house covered. No valid types available.
				continue;
			}

			// Check previous assignment. The previous assignment cannot be grouped one in case 1.
			if (previousCells is not [var previousCell])
			{
				continue;
			}

			// Check whether the grid contains 'current.Digit' in cell 'previousCell'.
			if ((grid.GetCandidates(previousCell) >> currentDigit & 1) == 0)
			{
				continue;
			}

			// Case 1 check passed.
			truth = house switch
			{
				< 9 => Space.BlockDigit(house, currentDigit),
				< 18 => Space.RowDigit(house - 9, currentDigit),
				_ => Space.ColumnDigit(house - 18, currentDigit)
			};
			link = Space.RowColumn(previousCell / 9, previousCell % 9);
			return true;

		Case2:
			// Otherwise, may be case 2.
			if (previousDigit != currentDigit)
			{
				// Different digits, with different house covered. No valid types available.
				continue;
			}

			// Collect cells that contains the current digit.
			var cellsIncludingCurrentDigit = CellMap.Empty;
			foreach (var cell in HousesMap[house] & emptyCells)
			{
				if ((grid.GetCandidates(cell) >> currentDigit & 1) != 0)
				{
					cellsIncludingCurrentDigit += cell;
				}
			}

			// Determine whether all the last cells (excluding 'currentCells') can be seen by previous cells or not.
			var lastCells = cellsIncludingCurrentDigit & ~currentCells;
			if (!lastCells)
			{
				// There's no cell to be included in link.
				continue;
			}
			if ((previousCells.PeerIntersection & lastCells) != lastCells)
			{
				continue;
			}

			// Okay. Now use shared house to describe links.
			var linkHouse = previousCells.FirstSharedHouse;
			Debug.Assert(linkHouse != FallbackConstants.@int);

			truth = house switch
			{
				< 9 => Space.BlockDigit(house, currentDigit),
				< 18 => Space.RowDigit(house - 9, currentDigit),
				_ => Space.ColumnDigit(house - 18, currentDigit)
			};
			link = linkHouse switch
			{
				< 9 => Space.BlockDigit(linkHouse, currentDigit),
				< 18 => Space.RowDigit(linkHouse - 9, currentDigit),
				_ => Space.ColumnDigit(linkHouse - 18, currentDigit)
			};
			return true;
		}

		// If here, case 3 should be checked now.
		// In case 3, current assignment can only hold one cell in order to construct cell truth.
		if (currentCells is not [var currentCell])
		{
			goto ReturnFalse;
		}

		// Check whether the target cell contains 'previousDigit'.
		if ((grid.GetCandidates(currentCell) >> previousDigit & 1) == 0)
		{
			goto ReturnFalse;
		}

		// Check whether the previous cell has a house that includes target cell 'currentCell'.
		// Iterate on each shared house.
		foreach (var house in previousCells.SharedHouses)
		{
			// Check whether including or not.
			if (!HousesMap[house].Contains(currentCell))
			{
				continue;
			}

			truth = Space.RowColumn(currentCell / 9, currentCell % 9);
			link = house switch
			{
				< 9 => Space.BlockDigit(house, previousDigit),
				< 18 => Space.RowDigit(house - 9, previousDigit),
				_ => Space.ColumnDigit(house - 18, previousDigit)
			};
			return true;
		}

	ReturnFalse:
		// Otherwise, we cannot infer a valid truth and link that can transfer assignments from 'previous' to 'current'.
		truth = link = default;
		return false;
	}
}
