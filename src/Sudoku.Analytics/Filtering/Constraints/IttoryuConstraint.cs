namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that checks whether a puzzle can be finished by ittoryu rules.
/// </summary>
public sealed class IttoryuConstraint : Constraint, IComparisonOperatorConstraint, ILimitCountConstraint<int>
{
	/// <summary>
	/// Indicates whether the constraint use strict mode to check for ittoryu mode,
	/// meaning the puzzle must be solved with ittoryu mode.
	/// </summary>
	public bool IsStrictIttoryu { get; set; }

	/// <summary>
	/// Indicates the rounds used.
	/// </summary>
	public int Rounds { get; set; }

	/// <summary>
	/// Indicates the single technique that can be used in the checking.
	/// </summary>
	public SingleTechniqueFlag LimitedSingle { get; set; }

	/// <inheritdoc/>
	public ComparisonOperator Operator { get; set; }

	/// <inheritdoc/>
	int ILimitCountConstraint<int>.LimitCount { get => Rounds; set => Rounds = value; }


	/// <inheritdoc/>
	public static int Minimum => 1;

	/// <inheritdoc/>
	public static int Maximum => 20;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is IttoryuConstraint comparer
		&& IsStrictIttoryu == comparer.IsStrictIttoryu && Rounds == comparer.Rounds
		&& Operator == comparer.Operator && LimitedSingle == comparer.LimitedSingle;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(IsStrictIttoryu, Rounds, LimitedSingle, Operator);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("IttoryuConstraint", culture),
			[
				Operator.OperatorString,
				Rounds,
				IsStrictIttoryu ? SR.Get("IttoryuConstraintInterpolation", culture) : string.Empty
			]
		);

	/// <inheritdoc/>
	public override IttoryuConstraint Clone()
		=> new()
		{
			IsNegated = IsNegated,
			Operator = Operator,
			LimitedSingle = LimitedSingle,
			Rounds = Rounds,
			IsStrictIttoryu = IsStrictIttoryu
		};

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		if (context.AnalysisResult is not { IsSolved: true, DifficultyLevel: DifficultyLevel.Easy })
		{
			// Bug fix: We won't check for steps if the grid is hard than 'DifficultyLevel.Easy'.
			// For example if a moderate puzzle is found, the expression '(SingleStep)steps[i]' will throw an InvalidCastException
			// because the step 'steps[i]' may not be a 'SingleStep'.
			return false;
		}

		_ = context.Grid.CanPrimaryFullHouse;

		var localAnalyzer = Analyzer.Default
			.WithStepSearchers(
				new SingleStepSearcher
				{
					EnableFullHouse = true,
					EnableLastDigit = true,
					HiddenSinglesInBlockFirst = true,
					EnableOrderingStepsByLastingValue = false
				}
			)
			.WithOptions(
				new()
				{
					IsDirectMode = true,
					UseIttoryuMode = true,
					PrimarySingle = LimitedSingle != SingleTechniqueFlag.NakedSingle
						? LimitedSingle
						: SingleTechniqueFlag.None
				}
			);
		if (localAnalyzer.Analyze(in context.Grid) is not
			{
				IsSolved: true,
				DifficultyLevel: DifficultyLevel.Easy,
				StepsSpan: { Length: var stepsCount } steps,
				GridsSpan: var stepGrids
			})
		{
			return false;
		}

		SortedSet<SingleTechniqueFlag> techniqueList = [.. from step in steps select step.Code.SingleTechnique];
		Debug.Assert(techniqueList.Max <= LimitedSingle);

		var roundsCount = 1;
		for (var i = 0; i < stepsCount - 1; i++)
		{
			var previousDigit = ((SingleStep)steps[i]).Digit;
			var currentDigit = ((SingleStep)steps[i + 1]).Digit;
			if (currentDigit < previousDigit)
			{
				roundsCount++;
			}

			// Check whether the all the interval digits are finished between 'previousDigit' and 'currentDigit'.
			// If such digits are already completed, we should consider this case as "consecutive" also.
			if (IsStrictIttoryu)
			{
				if (currentDigit >= previousDigit && currentDigit - previousDigit is 0 or 1)
				{
					continue;
				}

				ref readonly var currentGrid = ref stepGrids[i + 1];
				var valuesMap = currentGrid.ValuesMap;
				var areAllIntervalDigitsCompleted = true;
				for (var digit = (previousDigit + 1) % 9; digit != currentDigit; digit = (digit + 1) % 9)
				{
					if (valuesMap[digit].Count != 9)
					{
						areAllIntervalDigitsCompleted = false;
						break;
					}
				}
				if (areAllIntervalDigitsCompleted)
				{
					continue;
				}

				// This branch can only be triggered when we want to know if a puzzle can be solved with strict-ittoryu way.
				roundsCount = -1;
				break;
			}
		}
		if (roundsCount == -1)
		{
			return false;
		}

		return Operator.OperatorInt32(roundsCount, Rounds);
	}
}
