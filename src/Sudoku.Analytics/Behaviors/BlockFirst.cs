namespace Sudoku.Behaviors;

/// <summary>
/// Represents a way to finish puzzle with block-first rule.
/// </summary>
public sealed class BlockFirst : IBehaviorMetric
{
	/// <inheritdoc/>
	public static UserBehavior MeasurableBehavior => UserBehavior.BlockFirst;


	/// <inheritdoc/>
	public static ReadOnlySpan<int> GetDistanceArray(
		Collector collector,
		in Grid grid,
		out ReadOnlySpan<KeyValuePair<SingleStep, Grid>> steps,
		out ReadOnlySpan<KeyValuePair<Step, Grid>> stepsAll
	)
	{
		var (playground, solution, result) = (grid, grid.GetSolutionGrid(), (List<int>)[]);
		var (resultSteps, lastStep) = ((List<KeyValuePair<SingleStep, Grid>>)[], default(SingleStep)!);
		var resultStepsAll = (List<KeyValuePair<Step, Grid>>)[];
		while (!playground.IsSolved)
		{
			var possibleSteps = collector.Collect(playground);
			if (possibleSteps.Length == 0)
			{
				// The puzzle cannot be solved.
				throw new InvalidOperationException(SR.ExceptionMessage("PuzzleCannotBeSolved"));
			}

			if (possibleSteps.Any(static step => step is SingleStep))
			{
				// The puzzle can be solved with single steps.
				var validSteps =
					from step in possibleSteps
					let castedStep = step as SingleStep
					where castedStep is not null
					select castedStep into step
					group step by step.Code into stepGroup
					orderby stepGroup.Key
					select stepGroup;
				var (z, s) = findNearestStep(grid, playground, lastStep, validSteps);

				lastStep = z;
				var kvp = KeyValuePair.Create(lastStep, playground);
				result.Add(s);
				resultStepsAll.Add(kvp.Cast<SingleStep, Grid, Step, Grid>());
				resultSteps.Add(kvp);
				playground.Apply(new(Assignment, lastStep.Cell, solution.GetDigit(lastStep.Cell)));
				continue;
			}

			// If the puzzle cannot be solved here, we should apply with indirect techniques again and again.
			var foundStep = possibleSteps[0];
			resultStepsAll.Add(KeyValuePair.Create(foundStep, playground));
			playground.Apply(foundStep);
		}

		steps = resultSteps.AsSpan();
		stepsAll = resultStepsAll.AsSpan();
		return result.AsSpan();


		(SingleStep Step, int Score) findNearestStep(
			in Grid grid,
			in Grid playground,
			SingleStep lastStep,
			SpanOrderedEnumerable<SpanGrouping<SingleStep, Technique>> stepGroups
		)
		{
			var (minScore, minStep) = (int.MaxValue, default(SingleStep)!);
			foreach (var step in stepGroups[0])
			{
				if (lastStep is { Cell: var lastCell, Digit: var lastDigit }
					&& step.Cell.ToHouse(HouseType.Block) == lastCell.ToHouse(HouseType.Block)
					&& lastDigit > step.Digit
					&& lastCell.ToHouse(HouseType.Block) is var block
					&& (HousesMap[block] & playground.EmptyCells).Count != 1)
				{
					// Special rule:
					// If a user has found a new step that will revert filling rule,
					// it will be allowed if and only if the step is a full house.
					continue;
				}

				var newScore = getScore(grid, playground, lastStep?.Cell ?? -1, step.Cell);
				if (newScore <= minScore)
				{
					minScore = newScore;
					minStep = step;
				}
			}

			// Last resort: If a 'minStep' is not found, it must be treated as a new loop to find a new step.
			if (minStep is null)
			{
				minScore = int.MaxValue;
				minStep = default!;
				foreach (var step in stepGroups[0])
				{
					var s = getBottomingScore(step, playground);
					if (s <= minScore)
					{
						minScore = s;
						minStep = step;
					}
				}
				return (minStep, minScore);


				int getBottomingScore(SingleStep step, in Grid playground)
				{
					var lastDigit = solution.GetDigit(lastStep!.Cell);
					var currentDigit = solution.GetDigit(step.Cell);
					var currentCellBlock = step.Cell.ToHouse(HouseType.Block);
					var baseScore = 0;
					for (var (currentBlock, i) = (currentCellBlock, 0); i < 9; currentBlock = (currentBlock + 1) % 9, i++)
					{
						baseScore += (playground.EmptyCells & HousesMap[currentBlock]).Count;
					}

					var mask = (Mask)0;
					for (var d = 0; d < currentDigit; d++)
					{
						if (!!(playground.CandidatesMap[d] & HousesMap[currentCellBlock]))
						{
							mask |= (Mask)(1 << d);
						}
					}
					return baseScore + BitOperations.PopCount(mask);
				}
			}
			return (minStep, minScore);
		}

		static int getScore(in Grid grid, in Grid playground, Cell lastCell, Cell currentCell)
		{
			var solution = grid.GetSolutionGrid();

			// Create index table. e.g. If block 1 is missing digit 2, 5 and 7, we should make them in queue.
			var indexedList = new List<Cell>();
			var emptyCells = playground.EmptyCells;
			var startBlock = lastCell == -1 ? 0 : lastCell.ToHouse(HouseType.Block);
			for (var (block, i) = (startBlock, 0); i < 9; block = (block + 1) % 9, i++)
			{
				var emptyCellsInHouse = emptyCells & HousesMap[block];
				if (!emptyCellsInHouse)
				{
					continue;
				}

				foreach (var digit in playground[emptyCellsInHouse])
				{
					foreach (var cell in emptyCellsInHouse)
					{
						if (solution.GetDigit(cell) == digit && !indexedList.Contains(cell))
						{
							indexedList.Add(cell);
							break;
						}
					}
				}
			}

			// If a step is from the middle of the digit, we should check whether the previous step uses a different digit,
			// and remove them to find the number of digits in the removed set.
			var mask = playground[emptyCells & HousesMap[startBlock]];
			if (lastCell != -1)
			{
				var maskCopied = mask;
				foreach (var digit in maskCopied)
				{
					if (digit > solution.GetDigit(lastCell))
					{
						mask &= (Mask)~(1 << digit);
					}
				}
			}

			if (lastCell != -1
				&& BitOperations.IsPow2((lastCell.AsCellMap() + currentCell).BlockMask)
				&& solution.GetDigit(lastCell) is var lastDigit
				&& solution.GetDigit(currentCell) is var currentDigit
				&& currentDigit < lastDigit
				&& (emptyCells & HousesMap[lastCell.ToHouse(HouseType.Block)]).Count == 1)
			{
				// Directly measure the distance, ignoring whether two digits are filled with having reverted or not.
				return 1;
			}

			var index = indexedList.FindIndex(cell => cell == currentCell);
			return lastCell == -1 ? index + 1 : index + 1 - BitOperations.PopCount(mask);
		}
	}
}
