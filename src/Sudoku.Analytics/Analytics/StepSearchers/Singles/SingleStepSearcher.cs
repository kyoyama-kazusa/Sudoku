namespace Sudoku.Analytics.StepSearchers.Singles;

/// <summary>
/// Provides with a <b>Single</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Common techniques:
/// <list type="bullet">
/// <item>Full House (If the property <see cref="EnableFullHouse"/> is <see langword="true"/>)</item>
/// <item>Last Digit (If the property <see cref="EnableLastDigit"/> is <see langword="true"/>)</item>
/// <item>Naked Single</item>
/// </list>
/// </item>
/// <item>
/// Direct techniques:
/// <list type="bullet">
/// <item>Crosshatching</item>
/// </list>
/// </item>
/// <item>
/// Indirect techniques:
/// <list type="bullet">
/// <item>Hidden Single</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_SingleStepSearcher",
	Technique.CrosshatchingBlock, Technique.CrosshatchingRow, Technique.CrosshatchingColumn, Technique.LastDigit,
	Technique.FullHouse, Technique.HiddenSingleBlock, Technique.HiddenSingleRow, Technique.HiddenSingleColumn, Technique.NakedSingle,
	IsCachingSafe = true,
	IsOrderingFixed = true,
	IsAvailabilityReadOnly = true,
	RuntimeFlags = StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class SingleStepSearcher : StepSearcher
{
	/// <summary>
	/// Represents a method set.
	/// </summary>
	private static readonly unsafe delegate*<SingleStepSearcher, ref StepAnalysisContext, in Grid, Step?>[]
		MethodSet1 = [&CheckFullHouse, &CheckNakedSingle, &CheckHiddenSingle],
		MethodSet2 = [&CheckFullHouse, &CheckHiddenSingle, &CheckNakedSingle],
		MethodSet3 = [&CheckNakedSingle, &CheckHiddenSingle],
		MethodSet4 = [&CheckHiddenSingle, &CheckNakedSingle];


	/// <summary>
	/// Indicates whether the current searcher enables the technique full house.
	/// </summary>
	[SettingItemName(SettingItemNames.EnableFullHouse)]
	public bool EnableFullHouse { get; set; }

	/// <summary>
	/// Indicates whether the current searcher enables the technique last digit.
	/// </summary>
	[SettingItemName(SettingItemNames.EnableLastDigit)]
	public bool EnableLastDigit { get; set; }

	/// <summary>
	/// Indicates whether the current searcher checks for hidden single in block firstly.
	/// </summary>
	[SettingItemName(SettingItemNames.HiddenSinglesInBlockFirst)]
	public bool HiddenSinglesInBlockFirst { get; set; }

	/// <summary>
	/// Indicates whether the current searcher will make lasting value have a higher priority,
	/// ignoring which techniques used, to sort step and choose one.
	/// </summary>
	/// <remarks>
	/// This option will work if <see cref="Analyzer.IsFullApplying"/> is <see langword="false"/>,
	/// and <see cref="StepGathererOptions.IsDirectMode"/> is <see langword="true"/>.
	/// </remarks>
	/// <seealso cref="Analyzer.IsFullApplying"/>
	/// <seealso cref="StepGathererOptions.IsDirectMode"/>
	[SettingItemName(SettingItemNames.EnableOrderingStepsByLastingValue)]
	public bool EnableOrderingStepsByLastingValue { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
		=> (EnableOrderingStepsByLastingValue, context.Options) switch
		{
			(true, { UseIttoryuMode: var ittoryu, IsDirectMode: true }) => Collect_LastingValueHasPriority(ref context, ittoryu),
			(_, { UseIttoryuMode: true }) => Collect_IttoryuMode(ref context),
			_ => Collect_NonIttoryuMode(ref context)
		};

	/// <summary>
	/// Checks for single steps, making lasting value have a priority.
	/// </summary>
	private Step? Collect_LastingValueHasPriority(ref StepAnalysisContext context, bool isIttoryuMode)
	{
		var localContext = context with { OnlyFindOne = false, Accumulator = [] };
		var a = Collect_IttoryuMode;
		var b = Collect_NonIttoryuMode;
		(isIttoryuMode ? a : b)(ref localContext);

		Debug.Assert(!localContext.OnlyFindOne);
		var accumulator = localContext.Accumulator;
		if (accumulator.Count != 0)
		{
			accumulator.Sort(stepComparison);
		}

		if (context.OnlyFindOne && accumulator.Count != 0)
		{
			// Here we should choose one step from accumulator that should match user's configuration.
			if (context.Options.UseIttoryuMode)
			{
				for (var delta = 0; delta < 9; delta++)
				{
					foreach (SingleStep step in accumulator)
					{
						var correctedDigit = step.Digit is var digit && digit < context.PreviousSetDigit ? digit + 9 : digit;
						if (correctedDigit - context.PreviousSetDigit == delta)
						{
							context.PreviousSetDigit = step.Digit;
							return step;
						}
					}
				}
			}

			// Bottoming rule.
			return accumulator[0];
		}

		if (!context.OnlyFindOne && accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int stepComparison(Step left, Step right)
			=> ((SingleStep)left, (SingleStep)right) is var (l, r)
			&& handleEasyTechnique(left.Code) is var leftEasyCode
			&& handleEasyTechnique(right.Code) is var rightEasyCode
			&& leftEasyCode.CompareTo(rightEasyCode) is var easyTechniqueComparisonResult and not 0
				? easyTechniqueComparisonResult
				: (((ILastingTrait)l).Lasting, ((ILastingTrait)r).Lasting) is var (ll, rl)
				&& ll.CompareTo(rl) is var lastingComparisonResult and not 0
					? lastingComparisonResult
					: l.Code.CompareTo(r.Code) is var codeComparisonResult and not 0
						? codeComparisonResult
						: (l.Cell * 9 + l.Digit, r.Cell * 9 + r.Digit) is var (lc, rc)
						&& lc.CompareTo(rc) is var candidateComparisonResult and not 0
							? candidateComparisonResult
							: 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int handleEasyTechnique(Technique technique)
			=> technique switch
			{
				Technique.FullHouse => 0,
				Technique.LastDigit => 1,
				Technique.CrosshatchingBlock or Technique.HiddenSingleBlock => 2,
				_ => 3
			};
	}

	/// <summary>
	/// Checks for single steps using ittoryu mode.
	/// </summary>
	private Step? Collect_IttoryuMode(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		for (var (i, digit) = (0, context.PreviousSetDigit); i < 9; i++, digit = (digit + 1) % 9)
		{
			if (!EnableFullHouse)
			{
				goto CheckForHiddenSingle;
			}

			for (var house = 0; house < 27; house++)
			{
				var (count, resultCell, flag) = (0, -1, true);
				foreach (var cell in HousesMap[house])
				{
					if (grid.GetState(cell) == CellState.Empty && (resultCell = cell) is var _ && ++count > 1)
					{
						flag = false;
						break;
					}
				}
				if (!flag || count == 0)
				{
					continue;
				}

				if (BitOperations.TrailingZeroCount(grid.GetCandidates(resultCell)) != digit)
				{
					continue;
				}

				var emptyCellsCountFromAllPeerHouses = 0;
				foreach (var houseType in HouseTypes)
				{
					var peerHouse = resultCell.ToHouse(houseType);
					foreach (var cell in HousesCells[peerHouse])
					{
						if (grid.GetState(cell) == CellState.Empty)
						{
							emptyCellsCountFromAllPeerHouses++;
						}
					}
				}

				var step = new FullHouseStep(
					new SingletonArray<Conclusion>(new(Assignment, resultCell, digit)),
					[[new HouseViewNode(ColorIdentifier.Normal, house)]],
					context.Options,
					house,
					resultCell,
					digit,
					Lasting.GetLasting(grid, resultCell, house)
				);

				if (context.OnlyFindOne)
				{
					context.PreviousSetDigit = digit;
					return step;
				}

				context.Accumulator.Add(step);
			}

		CheckForHiddenSingle:
			for (var house = 0; house < 27; house++)
			{
				if (CheckForHiddenSingleAndLastDigit(this, grid, ref context, digit, house) is not { } step)
				{
					continue;
				}

				if (context.OnlyFindOne)
				{
					context.PreviousSetDigit = digit;
					return step;
				}

				context.Accumulator.Add(step);
			}

			for (var cell = 0; cell < 81; cell++)
			{
				if (grid.GetState(cell) != CellState.Empty)
				{
					continue;
				}

				if (grid.GetCandidates(cell) is var mask && !BitOperations.IsPow2(mask))
				{
					continue;
				}

				if (BitOperations.TrailingZeroCount(mask) != digit)
				{
					continue;
				}

				if (TechniqueNaming.Single.GetNakedSingleSubtype(grid, cell) is var subtype && subtype.IsUnnecessary
					&& grid.PuzzleType != SudokuType.Sukaku)
				{
					continue;
				}

				var step = new NakedSingleStep(
					new SingletonArray<Conclusion>(new(Assignment, cell, digit)),
					[[.. Excluder.GetNakedSingleExcluders(grid, cell, digit, out _)]],
					context.Options,
					cell,
					digit,
					subtype,
					Lasting.GetLastingAllHouses(grid, cell, out var lastingHouse),
					lastingHouse.HouseType
				);
				if (context.OnlyFindOne)
				{
					context.PreviousSetDigit = digit;
					return step;
				}

				context.Accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Checks for single steps using non-ittoryu mode.
	/// </summary>
	private unsafe Step? Collect_NonIttoryuMode(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Please note that, by default we should start with hidden singles. However, if a user has set the option
		// that a step searcher should distinct with direct mode and in-direct mode (i.e. all candidates are displayed),
		// we should start with a naked single because they are "direct" in such mode.
		var methodSet = (EnableFullHouse, !context.Options.IsDirectMode) switch
		{
			(true, true) => MethodSet1,
			(true, _) => MethodSet2,
			(_, true) => MethodSet3,
			_ => MethodSet4
		};
		for (var i = 0; i < (ReferenceEquals(methodSet, MethodSet1) || ReferenceEquals(methodSet, MethodSet2) ? 3 : 2); i++)
		{
			if (methodSet[i](this, ref context, grid) is { } step)
			{
				return step;
			}
		}
		return null;
	}


	/// <summary>
	/// Check for full houses.
	/// </summary>
	private static FullHouseStep? CheckFullHouse(SingleStepSearcher @this, ref StepAnalysisContext context, in Grid grid)
	{
		for (var house = 0; house < 27; house++)
		{
			var (count, resultCell, flag) = (0, -1, true);
			foreach (var cell in HousesMap[house])
			{
				if (grid.GetState(cell) == CellState.Empty)
				{
					resultCell = cell;
					if (++count > 1)
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag || count == 0)
			{
				continue;
			}

			var digit = BitOperations.TrailingZeroCount(grid.GetCandidates(resultCell));
			var step = new FullHouseStep(
				new SingletonArray<Conclusion>(new(Assignment, resultCell, digit)),
				[[new HouseViewNode(ColorIdentifier.Normal, house)]],
				context.Options,
				house,
				resultCell,
				digit,
				Lasting.GetLasting(grid, resultCell, house)
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Check for hidden singles.
	/// </summary>
	private static HiddenSingleStep? CheckHiddenSingle(SingleStepSearcher @this, ref StepAnalysisContext context, in Grid grid)
	{
		if (@this.HiddenSinglesInBlockFirst)
		{
			// If block first, we'll extract all blocks and iterate on them firstly.
			for (var house = 0; house < 9; house++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					if (CheckForHiddenSingleAndLastDigit(@this, grid, ref context, digit, house) is not { } step)
					{
						continue;
					}

					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}

			// Then secondly rows and columns.
			for (var house = 9; house < 27; house++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					if (CheckForHiddenSingleAndLastDigit(@this, grid, ref context, digit, house) is not { } step)
					{
						continue;
					}

					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}
		else
		{
			// We'll directly iterate on each house.
			// Theoretically, this iteration should be faster than above one, but in practice,
			// we may found hidden singles in block much more times than in row or column.
			for (var digit = 0; digit < 9; digit++)
			{
				for (var house = 0; house < 27; house++)
				{
					if (CheckForHiddenSingleAndLastDigit(@this, grid, ref context, digit, house) is not { } step)
					{
						continue;
					}

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
	/// Check for naked singles.
	/// </summary>
	private static NakedSingleStep? CheckNakedSingle(SingleStepSearcher @this, ref StepAnalysisContext context, in Grid grid)
	{
		for (var cell = 0; cell < 81; cell++)
		{
			if (grid.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			var mask = grid.GetCandidates(cell);
			if (!BitOperations.IsPow2(mask))
			{
				continue;
			}

			var digit = BitOperations.TrailingZeroCount(mask);
			var step = new NakedSingleStep(
				new SingletonArray<Conclusion>(new(Assignment, cell, digit)),
				[[.. Excluder.GetNakedSingleExcluders(grid, cell, digit, out _)]],
				context.Options,
				cell,
				digit,
				TechniqueNaming.Single.GetNakedSingleSubtype(grid, cell),
				Lasting.GetLastingAllHouses(grid, cell, out var lastingHouse),
				lastingHouse.HouseType
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Checks for existence of hidden single and last digit conclusion in the specified house.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="digit">The digit used.</param>
	/// <param name="house">The house used.</param>
	/// <returns>Not <see langword="null"/> if conclusion can be found.</returns>
	/// <remarks>
	/// <para><include file="../../global-doc-comments.xml" path="/g/developer-notes"/></para>
	/// <para>
	/// The main idea of hidden single is to search for a digit can only appear once in a house,
	/// so we should check all possibilities in a house to found whether the house exists a digit
	/// that only appears once indeed.
	/// </para>
	/// </remarks>
	private static HiddenSingleStep? CheckForHiddenSingleAndLastDigit(SingleStepSearcher @this, in Grid grid, ref StepAnalysisContext context, Digit digit, House house)
	{
		var (count, resultCell, flag) = (0, -1, true);
		foreach (var cell in HousesMap[house])
		{
			if (grid.Exists(cell, digit) is true)
			{
				resultCell = cell;
				if (++count > 1)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag || count == 0)
		{
			// The digit has been filled into the house, or the digit appears more than once,
			// it will be invalid for a hidden single. Just skip it.
			return null;
		}

		// The digit is a hidden single.
		// Now collect information (especially for drawing & text) from the current found step.
		var (enableAndIsLastDigit, cellOffsets) = (false, new List<IconViewNode>());
		if (@this.EnableLastDigit)
		{
			// Sum up the number of appearing in the grid of 'digit'.
			var digitCount = 0;
			for (var cell = 0; cell < 81; cell++)
			{
				if (grid.GetDigit(cell) == digit)
				{
					digitCount++;
					cellOffsets.Add(new CircleViewNode(ColorIdentifier.Normal, cell));
				}
			}

			enableAndIsLastDigit = digitCount == 8;
		}

		return (enableAndIsLastDigit, house) switch
		{
			(true, >= 9) => null,
			(true, _) => new LastDigitStep(
				new SingletonArray<Conclusion>(new(Assignment, resultCell, digit)),
				[[.. cellOffsets]],
				context.Options,
				resultCell,
				digit,
				house,
				Lasting.GetLastingAllHouses(grid, resultCell, out _)
			),
			_ => Excluder.GetHiddenSingleExcluders(grid, digit, house, resultCell, out var chosenCells, out var excluderInfo) switch
			{
				var cellOffsets2 => TechniqueNaming.Single.GetHiddenSingleSubtype(grid, resultCell, house, chosenCells) switch
				{
					var subtype when subtype.IsUnnecessary && grid.PuzzleType != SudokuType.Sukaku => null,
					var subtype => new HiddenSingleStep(
						new SingletonArray<Conclusion>(new(Assignment, resultCell, digit)),
						[[.. cellOffsets2, new HouseViewNode(ColorIdentifier.Normal, house)]],
						context.Options,
						resultCell,
						digit,
						house,
						enableAndIsLastDigit,
						Lasting.GetLasting(grid, resultCell, house),
						subtype,
						excluderInfo
					)
				}
			}
		};
	}
}
