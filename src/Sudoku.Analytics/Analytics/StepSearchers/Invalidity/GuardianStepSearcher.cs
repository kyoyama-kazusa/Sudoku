namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Guardian</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Guardian</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_GuardianStepSearcher", Technique.BrokenWing)]
public sealed partial class GuardianStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the base searcher.
	/// </summary>
	private static readonly PatternOverlayStepSearcher ElimsSearcher = new();


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Check POM eliminations first.
		ref readonly var grid = ref context.Grid;
		var eliminationMaps = (stackalloc CellMap[9]);
		eliminationMaps.Fill(CellMap.Empty);

		var pomSteps = new List<Step>();
		var playground = grid;
		var pomContext = new StepAnalysisContext(playground)
		{
			Accumulator = pomSteps,
			OnlyFindOne = false,
			Options = context.Options
		};
		ElimsSearcher.Collect(ref pomContext);

		foreach (PatternOverlayStep step in pomSteps)
		{
			ref var currentMap = ref eliminationMaps[step.Digit];
			foreach (var conclusion in step.Conclusions)
			{
				currentMap.Add(conclusion.Cell);
			}
		}

		var resultAccumulator = new SortedSet<GuardianStep>();
		for (var digit = 0; digit < 9; digit++)
		{
			if (!eliminationMaps[digit])
			{
				// Using global view, we cannot find any eliminations for this digit.
				// Just skip to the next loop.
				continue;
			}

			var foundData = CollectGuardianLoops(digit);
			if (foundData.Length == 0)
			{
				continue;
			}

			foreach (var (loop, guardians, _) in foundData)
			{
				if ((guardians.PeerIntersection & CandidatesMap[digit]) is not (var elimMap and not []))
				{
					continue;
				}

				// Add found step into the collection.
				// To be honest I can return the step if 'LogicalAnalysisContext.OnlyFindOne' is true,
				// but due to the limit of the algorithm, the method always gets the longer guardian loops instead of shorter ones.
				// If we just return the first found step, we will miss steps being more elegant.
				resultAccumulator.Add(
					new GuardianStep(
						(from c in elimMap select new Conclusion(Elimination, c, digit)).ToArray(),
						[
							[
								.. from c in loop select new CandidateViewNode(ColorIdentifier.Normal, c * 9 + digit),
								.. from c in guardians select new CandidateViewNode(ColorIdentifier.Auxiliary1, c * 9 + digit)
							]
						],
						context.Options,
						digit,
						loop,
						guardians
					)
				);
			}
		}

		if (context.OnlyFindOne && resultAccumulator.Count != 0)
		{
			return resultAccumulator.Min;
		}
		if (!context.OnlyFindOne && resultAccumulator.Count != 0)
		{
			context.Accumulator.AddRange(resultAccumulator);
		}
		return null;
	}


	/// <summary>
	/// Try to collect all possible loops which should satisfy the specified condition.
	/// </summary>
	/// <param name="digit">The digit used.</param>
	/// <returns>
	/// Returns a list of array of candidates used in the loop, as the data of possible found loops.
	/// </returns>
	private static unsafe GuardianPattern[] CollectGuardianLoops(Digit digit)
	{
		static bool predicate(in CellMap loop) => loop.Count is var l && (l & 1) != 0 && l >= 5;
		var result = new HashSet<GuardianPattern>();
		foreach (var cell in CandidatesMap[digit])
		{
			dfs(cell, cell, 0, cell.AsCellMap(), CellMap.Empty, digit, &predicate, result);
		}
		return [.. result];


		static void dfs(
			Cell startCell,
			Cell lastCell,
			House lastHouse,
			in CellMap currentLoop,
			in CellMap currentGuardians,
			Digit digit,
			delegate*<in CellMap, bool> condition,
			HashSet<GuardianPattern> result
		)
		{
			foreach (var houseType in HouseTypes)
			{
				var house = lastCell.ToHouse(houseType);
				if ((lastHouse >> house & 1) != 0)
				{
					continue;
				}

				var cellsToBeChecked = CandidatesMap[digit] & HousesMap[house];
				if (cellsToBeChecked.Count < 2 || (currentLoop & HousesMap[house]).Count > 2)
				{
					continue;
				}

				foreach (var tempCell in cellsToBeChecked)
				{
					if (tempCell == lastCell)
					{
						continue;
					}

					var housesUsed = 0;
					foreach (var tempHouseType in HouseTypes)
					{
						if (tempCell.ToHouse(tempHouseType) == lastCell.ToHouse(tempHouseType))
						{
							housesUsed |= 1 << lastCell.ToHouse(tempHouseType);
						}
					}

					var tempGuardians = (CandidatesMap[digit] & HousesMap[house]) - tempCell - lastCell;
					if (tempCell == startCell && condition(currentLoop)
						&& !!((currentGuardians | tempGuardians).PeerIntersection & CandidatesMap[digit]))
					{
						result.Add(new(currentLoop, currentGuardians | tempGuardians, digit));

						// Exit the current of this recursion frame.
						return;
					}

					if ((currentLoop | currentGuardians).Contains(tempCell)
						|| !((currentGuardians | tempGuardians).PeerIntersection & CandidatesMap[digit])
						|| (HousesMap[house] & currentLoop).Count > 1)
					{
						continue;
					}

					dfs(
						startCell,
						tempCell,
						lastHouse | housesUsed,
						currentLoop + tempCell,
						currentGuardians | tempGuardians,
						digit,
						condition,
						result
					);
				}
			}
		}
	}
}
