namespace Sudoku.Analytics;

/// <summary>
/// Provides the result after <see cref="Analyzer"/> solving a puzzle.
/// </summary>
/// <param name="Puzzle"><inheritdoc cref="IAnalysisResult{TSolver, TSolverResult}.Puzzle" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Equatable)]
public sealed partial record AnalysisResult([property: EquatableMember] in Grid Puzzle) :
	IAnalysisResult<AnalysisResult, Analyzer>,
	meta_analysis::IAnalysisResult<AnalysisResult, Grid, Step>,
	IAnyAllMethod<AnalysisResult, Step>,
	ICastMethod<AnalysisResult, Step>,
	IEnumerable<Step>,
	IFormattable,
	IOfTypeMethod<AnalysisResult, Step>,
	IReadOnlyDictionary<Grid, Step>,
	ISelectMethod<AnalysisResult, Step>,
	IWhereMethod<AnalysisResult, Step>
{
	/// <summary>
	/// Indicates the maximum rating value in theory.
	/// </summary>
	public const int MaximumRatingValueTheory = 200;

	/// <summary>
	/// Indicates the maximum rating value in fact.
	/// </summary>
	public const int MaximumRatingValueFact = 120;

	/// <summary>
	/// Indicates the minimum rating value.
	/// </summary>
	public const int MinimumRatingValue = 0;

	/// <summary>
	/// Indicates the default options.
	/// </summary>
	public const FormattingOptions DefaultOptions = FormattingOptions.ShowDifficulty
		| FormattingOptions.ShowSeparators
		| FormattingOptions.ShowStepsAfterBottleneck
		| FormattingOptions.ShowSteps
		| FormattingOptions.ShowGridAndSolutionCode
		| FormattingOptions.ShowElapsedTime;


	/// <inheritdoc/>
	[MemberNotNullWhen(true, nameof(InterimSteps), nameof(InterimGrids))]
	[MemberNotNullWhen(true, nameof(PearlStep), nameof(DiamondStep))]
	[MemberNotNullWhen(true, nameof(MemoryUsed))]
	public required bool IsSolved { get; init; }

	/// <summary>
	/// Indicates whether the puzzle is solved, or failed by <see cref="FailedReason.AnalyzerGiveUp"/>.
	/// If the property returns <see langword="true"/>, properties <see cref="StepsSpan"/> and <see cref="GridsSpan"/>
	/// won't be empty.
	/// </summary>
	/// <seealso cref="FailedReason.AnalyzerGiveUp"/>
	/// <seealso cref="StepsSpan"/>
	/// <seealso cref="GridsSpan"/>
	[MemberNotNullWhen(true, nameof(InterimSteps), nameof(InterimGrids))]
	public bool IsPartiallySolved => IsSolved || FailedReason == FailedReason.AnalyzerGiveUp;

	/// <summary>
	/// Indicates whether the puzzle is a pearl puzzle, which means the first step must be an indirect technique usage.
	/// </summary>
	/// <returns>
	/// Returns a <see cref="bool"/>? value indicating the result. The values are:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>The puzzle has a unique solution, and the first set step has same difficulty with the whole steps.</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>The puzzle has a unique solution, but the first set step does not have same difficulty with the whole steps.</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>The puzzle has multiple solutions, or the puzzle has no valid solution.</description>
	/// </item>
	/// </list>
	/// </returns>
	public bool? IsPearl
		=> this switch
		{
			{ IsSolved: true, PearlStep: { Difficulty: var ep } step } => ep == MaxDifficulty && step is not SingleStep,
			_ => null
		};

	/// <summary>
	/// Indicates whether the puzzle is a diamond puzzle, which means the first deletion has the same difficulty
	/// with the maximum difficulty of the whole steps.
	/// </summary>
	/// <returns>
	/// Returns a <see cref="bool"/>? value indicating the result. The values are:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>The puzzle has a unique solution, and the first deletion step has same difficulty with the whole steps.</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>The puzzle has a unique solution, but the first deletion step does not have same difficulty with the whole steps.</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>The puzzle has multiple solutions, or the puzzle has no valid solution.</description>
	/// </item>
	/// </list>
	/// </returns>
	public bool? IsDiamond
		=> this switch
		{
			{ IsSolved: true, PearlStep: { Difficulty: var ep } pStep, DiamondStep: { Difficulty: var ed } dStep }
				=> ed == MaxDifficulty && ep == ed && (pStep, dStep) is (not SingleStep, not SingleStep),
			_ => null
		};

	/// <summary>
	/// Indicates the maximum difficulty of the puzzle.
	/// </summary>
	/// <remarks>
	/// When the puzzle is solved by <see cref="Analyzer"/>,
	/// the value will be the maximum value among all difficulty ratings in solving steps. If the puzzle has not been solved,
	/// or else the puzzle is solved by other solvers, this value will be always <c>200</c>,
	/// equal to <see cref="MaximumRatingValueTheory"/>.
	/// </remarks>
	/// <seealso cref="Analyzer"/>
	/// <seealso cref="MaximumRatingValueTheory"/>
	public int MaxDifficulty => EvaluateRating(StepsSpan, SpanEnumerable.Max, MaximumRatingValueTheory);

	/// <summary>
	/// Indicates the total difficulty rating of the puzzle.
	/// </summary>
	/// <remarks>
	/// When the puzzle is solved by <see cref="Analyzer"/>, the value will be the sum of all difficulty ratings of steps.
	/// If the puzzle has not been solved, the value will be the sum of all difficulty ratings of steps recorded
	/// in <see cref="StepsSpan"/>.
	/// However, if the puzzle is solved by other solvers, this value will be <c>0</c>.
	/// </remarks>
	/// <seealso cref="Analyzer"/>
	/// <seealso cref="StepsSpan"/>
	public int TotalDifficulty => EvaluateRating(StepsSpan, SpanEnumerable.Sum, MinimumRatingValue);

	/// <summary>
	/// Indicates the pearl difficulty rating of the puzzle, calculated during only by <see cref="Analyzer"/>.
	/// </summary>
	/// <remarks>
	/// When the puzzle is solved, the value will be the difficulty rating of the first delete step that cause a set.
	/// </remarks>
	/// <seealso cref="Analyzer"/>
	/// <seealso href="http://forum.enjoysudoku.com/the-hardest-sudokus-new-thread-t6539-690.html#p293738">Concept for EP, ER and ED</seealso>
	public int? PearlDifficulty => PearlStep?.Difficulty;

	/// <summary>
	/// <para>
	/// Indicates the pearl difficulty rating of the puzzle, calculated during only by <see cref="Analyzer"/>.
	/// </para>
	/// <para>
	/// When the puzzle is solved, the value will be the difficulty rating of the first delete step.
	/// </para>
	/// </summary>
	/// <seealso cref="Analyzer"/>
	/// <seealso href="http://forum.enjoysudoku.com/the-hardest-sudokus-new-thread-t6539-690.html#p293738">Concept for EP, ER and ED</seealso>
	public int? DiamondDifficulty => DiamondStep?.Difficulty;

	/// <summary>
	/// Indicates the memory used in bytes. This value is not <see langword="null"/> if the puzzle is solved.
	/// </summary>
	public long? MemoryUsed { get; init; }

	/// <summary>
	/// Indicates why the solving operation is failed.
	/// This property is meaningless when <see cref="IsSolved"/> keeps the <see langword="true"/> value.
	/// </summary>
	/// <seealso cref="IsSolved"/>
	public FailedReason FailedReason { get; init; }

	/// <summary>
	/// Indicates the difficulty level of the puzzle.
	/// If the puzzle has not solved or solved by other solvers, this value will be <see cref="DifficultyLevel.Unknown"/>.
	/// </summary>
	public DifficultyLevel DifficultyLevel => (DifficultyLevel)StepsSpan.Max(static step => (int)step.DifficultyLevel);

	/// <inheritdoc/>
	public Grid Solution { get; init; }

	/// <inheritdoc/>
	public TimeSpan ElapsedTime { get; init; }

	/// <summary>
	/// Returns a <see cref="ReadOnlySpan{T}"/> of <see cref="Grid"/> instances,
	/// whose internal values come from <see cref="InterimGrids"/>.
	/// </summary>
	/// <seealso cref="InterimGrids"/>
	public ReadOnlySpan<Grid> GridsSpan => InterimGrids;

	/// <summary>
	/// Returns a <see cref="ReadOnlySpan{T}"/> of <see cref="Step"/> instances,
	/// whose internal values come from <see cref="InterimSteps"/>.
	/// </summary>
	/// <seealso cref="InterimSteps"/>
	public ReadOnlySpan<Step> StepsSpan => InterimSteps;

	/// <summary>
	/// <para>
	/// Indicates the wrong step found. In general cases, if the property <see cref="IsSolved"/> keeps
	/// <see langword="false"/> value, it'll mean the puzzle is invalid to solve, or the solver has found
	/// one error step to apply, that causes the original puzzle <see cref="Puzzle"/> become invalid.
	/// In this case we can check this property to get the wrong information to debug the error,
	/// or tell the author himself directly, with the inner value of this property held.
	/// </para>
	/// <para>
	/// However, if the puzzle is successful to be solved, the property won't contain any value,
	/// so it'll keep the <see langword="null"/> reference. Therefore, please check the nullability
	/// of this property before using.
	/// </para>
	/// <para>
	/// In general, this table will tell us the nullability of this property:
	/// <list type="table">
	/// <listheader>
	/// <term>Nullability</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Not <see langword="null"/></term>
	/// <description>The puzzle is failed to solve, and the solver has found an invalid step to apply.</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Other cases.</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	/// <seealso cref="IsSolved"/>
	/// <seealso cref="Puzzle"/>
	public Step? WrongStep => (UnhandledException as WrongStepException)?.WrongStep;

	/// <summary>
	/// Indicates the pearl step.
	/// </summary>
	public Step? PearlStep
		=> IsSolved && !StepsSpan.All(static s => s is FullHouseStep or HiddenSingleStep { House: < 9 })
			? StepsSpan.AllAre<Step, SingleStep>()
				// If a puzzle can be solved using only singles, just check for the first step not hidden single in block.
				? StepsSpan.First(static s => s is not HiddenSingleStep { House: < 9 })
				// Otherwise, an deletion step should be chosen. There're two cases:
				//    1) If the first step is a single, just return it as the diamond difficulty.
				//    2) If the first step is not a single, find for the first step that is a single,
				//       and check the maximum difficulty rating of the span of steps
				//       between the first step and the first single step.
				: StepsSpan.FirstIndex(static s => s is not SingleStep) is var a
					? StepsSpan.FirstIndex(static s => s is SingleStep) is var b and not 0
						? StepsSpan[a..b].MaxBy(static s => s.Difficulty)
						: StepsSpan[0]
					: null
			// No diamond step exist in all steps are hidden singles in block.
			: null;

	/// <summary>
	/// Indicates the diamond step.
	/// </summary>
	public Step? DiamondStep => IsSolved ? StepsSpan[0] : null;

	/// <summary>
	/// Indicates the techniques used during the solving operation.
	/// </summary>
	public TechniqueSet TechniquesUsed => [.. from step in StepsSpan select step.Code];

	/// <summary>
	/// <inheritdoc cref="IAnalysisResult{TSolver, TSolverResult}.UnhandledException" path="/summary"/>
	/// </summary>
	/// <remarks>
	/// You can visit the property value if the property <see cref="FailedReason"/>
	/// is <see cref="FailedReason.ExceptionThrown"/> or <see cref="FailedReason.WrongStep"/>.
	/// </remarks>
	/// <seealso cref="FailedReason"/>
	/// <seealso cref="FailedReason.ExceptionThrown"/>
	/// <seealso cref="FailedReason.WrongStep"/>
	public Exception? UnhandledException { get; init; }

	/// <summary>
	/// Indicates a list, whose element is the intermediate grid for each step.
	/// </summary>
	/// <seealso cref="InterimSteps"/>
	internal Grid[]? InterimGrids { get; init; }

	/// <summary>
	/// Indicates all solving steps that the solver has recorded.
	/// </summary>
	/// <seealso cref="InterimGrids"/>
	internal Step[]? InterimSteps { get; init; }

	/// <inheritdoc/>
	int IReadOnlyCollection<KeyValuePair<Grid, Step>>.Count => Span.Length;

	/// <inheritdoc/>
	meta_analysis::FailedReason meta_analysis::IAnalysisResult<AnalysisResult, Grid, Step>.FailedReason
	{
		get => FailedReason switch
		{
			FailedReason.Nothing => meta_analysis::FailedReason.None,
			FailedReason.PuzzleIsInvalid or FailedReason.PuzzleHasNoSolution or FailedReason.PuzzleHasMultipleSolutions
				or FailedReason.AnalyzerGiveUp
				=> meta_analysis::FailedReason.PuzzleInvalid,
			FailedReason.UserCancelled => meta_analysis::FailedReason.UserCancelled,
			FailedReason.ExceptionThrown or FailedReason.WrongStep or FailedReason.NotImplemented
				=> meta_analysis::FailedReason.ExceptionThrown
		};

		init => FailedReason = value switch
		{
			meta_analysis::FailedReason.None => FailedReason.Nothing,
			meta_analysis::FailedReason.PuzzleInvalid => FailedReason.PuzzleIsInvalid,
			meta_analysis::FailedReason.UserCancelled => FailedReason.UserCancelled,
			meta_analysis::FailedReason.ExceptionThrown => FailedReason.ExceptionThrown,
			_ => throw new ArgumentOutOfRangeException(nameof(value))
		};
	}

	/// <inheritdoc/>
	ReadOnlySpan<Step> meta_analysis::IAnalysisResult<AnalysisResult, Grid, Step>.Steps => StepsSpan;

	/// <inheritdoc/>
	IEnumerable<Grid> IReadOnlyDictionary<Grid, Step>.Keys => InterimGrids ?? [];

	/// <inheritdoc/>
	IEnumerable<Step> IReadOnlyDictionary<Grid, Step>.Values => InterimSteps ?? [];


	/// <inheritdoc/>
	Step IReadOnlyDictionary<Grid, Step>.this[Grid key] => this[key];


	/// <summary>
	/// Determine whether the analyzer result instance contains any step with the specified rating.
	/// </summary>
	/// <param name="rating">The rating value to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool HasRating(int rating)
	{
		foreach (var step in this)
		{
			if (step.Difficulty == rating)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether the analyzer result instance contains any step with specified technique.
	/// </summary>
	/// <param name="technique">The technique you want to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="InvalidOperationException">Throws when the puzzle has not been solved.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasTechnique(Technique technique) => TechniquesUsed.Contains(technique);

	/// <summary>
	/// Determine whether the analyzer result instance contains the specified grid.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool HasGrid(in Grid grid)
	{
		foreach (ref readonly var g in GridsSpan)
		{
			if (g == grid)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether all <see cref="Step"/> instances satisfy the specified condition.
	/// </summary>
	/// <param name="predicate">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool TrueForAll(Func<Step, bool> predicate)
	{
		foreach (var step in this)
		{
			if (!predicate(step))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determines whether at least one <see cref="Step"/> instance satisfies the specified condition.
	/// </summary>
	/// <param name="predicate">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Exists(Func<Step, bool> predicate)
	{
		foreach (var step in this)
		{
			if (predicate(step))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => Puzzle.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => ToString(DefaultOptions);

	/// <inheritdoc cref="ToString(FormattingOptions, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(FormattingOptions options) => ToString(options, default(IFormatProvider));

	/// <inheritdoc cref="ToString(FormattingOptions, IFormatProvider?, Func{string, Step, string}?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(Func<string, Step, string>? stepStringReplacer) => ToString(DefaultOptions, null, stepStringReplacer);

	/// <inheritdoc cref="ToString(FormattingOptions, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider) => ToString(DefaultOptions, formatProvider);

	/// <inheritdoc cref="ToString(FormattingOptions, IFormatProvider?, Func{string, Step, string}?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider, Func<string, Step, string> stepStringReplacer)
		=> ToString(DefaultOptions, formatProvider, stepStringReplacer);

	/// <inheritdoc cref="ToString(FormattingOptions, IFormatProvider?, Func{string, Step, string}?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(FormattingOptions options, Func<string, Step, string> stepStringReplacer)
		=> ToString(options, null, stepStringReplacer);

	/// <summary>
	/// Returns a string that represents the current object, with the specified formatting options.
	/// </summary>
	/// <param name="options">The formatting options.</param>
	/// <param name="formatProvider">The format provider instance.</param>
	/// <returns>A string that represents the current object.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(FormattingOptions options, IFormatProvider? formatProvider) => ToString(options, formatProvider, null);

	/// <summary>
	/// Returns a string that represents the current object, with the specified formatting options,
	/// with a string replacer method that can enhance the output string.
	/// </summary>
	/// <param name="options">The formatting options.</param>
	/// <param name="formatProvider">The format provider instance.</param>
	/// <param name="stepStringReplacer">The string replacer method that can substitute each step string.</param>
	/// <returns>A string that represents the current object.</returns>
	/// <remarks>
	/// <para>
	/// The argument <paramref name="stepStringReplacer"/> enhances the text output.
	/// You can replace it with whatever you want to display.
	/// For example, in <see cref="Console.Out"/> stream, you can use Epson formatting syntax to output text
	/// by using special escaped character <c>'\e'</c>:
	/// <code><![CDATA[
	/// \e[38;2;255;0;0mRed text\e[0m
	/// \e[48;2;0;255;0mGreen background\e[0m
	/// \e[38;2;0;0;255;48;2;255;255;0mBlue text with a yellow background\e[0m
	/// ]]></code>
	/// Here, you can use statements like <c><![CDATA[\e[38;2;<red>;<green>;<blue>m]]></c> and <c>\e[0m</c>
	/// to control output text color, where:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term>38</term>
	/// <description>Foreground</description>
	/// </item>
	/// <item>
	/// <term>48</term>
	/// <description>Background</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// For example, if you want to change the color to red, just surround original text with <c>\e</c> statements:
	/// <code><![CDATA[
	/// static string replacer(string str, Step step)
	///     => step.DifficultyLevel == DifficultyLevel.Nightmare ? $"\e[38;2;255;0;0m{str}\e[0m" : str;
	/// ]]></code>
	/// </para>
	/// </remarks>
	/// <seealso cref="Console.Out"/>
	/// <seealso href="https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/epson-esc-pos-with-formatting">
	/// Epson formatting
	/// </seealso>
	public string ToString(FormattingOptions options, IFormatProvider? formatProvider, Func<string, Step, string>? stepStringReplacer)
	{
		// Initialize and deconstruct variables.
		if (this is not
			{
				IsSolved: var isSolved,
				TotalDifficulty: var total,
				MaxDifficulty: var max,
				PearlDifficulty: var pearl,
				DiamondDifficulty: var diamond,
				Puzzle: var puzzle,
				Solution: var solution,
				ElapsedTime: var elapsed,
				StepsSpan: var steps,
				MemoryUsed: var memoryUsed
			})
		{
			throw new();
		}
		var r = stepStringReplacer ?? (static (self, _) => self);
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var culture = converter.CurrentCulture ?? CultureInfo.CurrentUICulture;

		// Print header.
		var sb = new StringBuilder();
		if (f(FormattingOptions.ShowGridAndSolutionCode))
		{
			sb.AppendLine($"{SR.Get("AnalysisResultPuzzle", culture)}{puzzle:#}");
		}

		// Print solving steps (if worth).
		if (f(FormattingOptions.ShowSteps) && steps.Length != 0)
		{
			sb.AppendLine(SR.Get("AnalysisResultSolvingSteps", culture));

			if (getBottleneck() is var (bIndex, bottleneckStep))
			{
				for (var i = 0; i < steps.Length; i++)
				{
					if (i > bIndex && !f(FormattingOptions.ShowStepsAfterBottleneck))
					{
						sb.AppendLine(SR.Get("Ellipsis", culture));
						break;
					}

					var step = steps[i];
					var stepStr = f(FormattingOptions.ShowSimple) ? step.ToSimpleString(culture) : step.ToString(culture);
					var showDiff = f(FormattingOptions.ShowDifficulty);
					var d = $"({step.Difficulty,5}";
					var s = $"{i + 1,4}";
					var labelInfo = (f(FormattingOptions.ShowStepLabel), showDiff) switch
					{
						(true, true) => $"{s}, {d}) ",
						(true, false) => $"{s} ",
						(false, true) => $"{d}) ",
						_ => string.Empty
					};
					sb.AppendLine(r($"{labelInfo}{stepStr}", step));
				}

				if (f(FormattingOptions.ShowBottleneck))
				{
					a(sb, f(FormattingOptions.ShowSeparators));

					sb.Append(SR.Get("AnalysisResultBottleneckStep", culture));

					if (f(FormattingOptions.ShowStepLabel))
					{
						sb.Append(SR.Get("AnalysisResultInStep", culture));
						sb.Append(bIndex + 1);
						sb.Append(SR.Get("_Token_Colon", culture));
					}

					sb.Append(' ');
					sb.AppendLine(r(bottleneckStep.ToString(), bottleneckStep));
				}

				a(sb, f(FormattingOptions.ShowSeparators));
			}
		}

		// Print solving step statistics (if worth).
		if (steps.Length != 0)
		{
			var stepsCount = steps.Length;

			sb.AppendLine(SR.Get("AnalysisResultTechniqueUsed", culture));

			if (f(FormattingOptions.ShowStepDetail))
			{
				sb.Append($"{SR.Get("AnalysisResultMin", culture),6}, ");
				sb.Append($"{SR.Get("AnalysisResultTotal", culture),6}");
				sb.Append(SR.Get("AnalysisResultTechniqueUsing", culture));
			}

			var stepsSortedByName = new List<Step>();
			stepsSortedByName.AddRange(steps);
			stepsSortedByName.Sort(
				(left, right) => left.DifficultyLevel.CompareTo(right.DifficultyLevel) is var difficultyLevelComparisonResult and not 0
					? difficultyLevelComparisonResult
					: left.Code.CompareTo(right.Code) is var codeComparisonResult and not 0
						? codeComparisonResult
						: StepMarshal.CompareName(left, right, formatProvider)
			);

			foreach (ref readonly var solvingStepsGroup in
				from step in stepsSortedByName
				select step into step
				group step by step.GetName(formatProvider))
			{
				if (f(FormattingOptions.ShowStepDetail))
				{
					var (currentTotal, currentMinimum) = (0, int.MaxValue);
					foreach (var solvingStep in solvingStepsGroup)
					{
						var difficulty = solvingStep.Difficulty;
						currentTotal += difficulty;
						currentMinimum = Math.Min(currentMinimum, difficulty);
					}
					sb.Append($"{currentMinimum,6}, {currentTotal,6}) ");
				}
				sb.AppendLine($"{solvingStepsGroup.Length,3} * {solvingStepsGroup.Key}");
			}

			if (f(FormattingOptions.ShowStepDetail))
			{
				sb.Append($"  (---{total,8}) ");
			}

			sb.Append($"{stepsCount,3} ");
			sb.AppendLine(SR.Get(stepsCount == 1 ? "AnalysisResultStepSingular" : "AnalysisResultStepPlural", culture));

			a(sb, f(FormattingOptions.ShowSeparators));
		}

		// Print detail data.
		sb.Append(SR.Get("AnalysisResultPuzzleRating", culture));
		sb.AppendLine($"{max}/{pearl ?? MaximumRatingValueTheory}/{diamond ?? MaximumRatingValueTheory}");

		// Print the solution (if not null and worth).
		if (!solution.IsUndefined && f(FormattingOptions.ShowGridAndSolutionCode))
		{
			sb.AppendLine($"{SR.Get("AnalysisResultPuzzleSolution", culture)}{solution:!}");
		}

		// Print the elapsed time.
		sb.Append(SR.Get("AnalysisResultPuzzleHas", culture));
		if (!isSolved)
		{
			sb.Append(SR.Get("AnalysisResultNot", culture));
		}
		sb.AppendLine(SR.Get("AnalysisResultBeenSolved", culture));
		if (f(FormattingOptions.ShowElapsedTime))
		{
			sb.Append(SR.Get("AnalysisResultTimeElapsed", culture));
			sb.AppendLine(elapsed.ToString(@"hh\:mm\:ss\.fff"));
		}
		if (memoryUsed is not null && f(FormattingOptions.ShowMemoryUsage))
		{
			sb.Append(SR.Get("AnalysisResultMemoryUsed", culture));
			sb.AppendLine(
				memoryUsed <= 0
					? $"{-memoryUsed / 1048576D:#.###} MB{SR.Get("AnalysisResultMemoryUsedMinusResult", culture)}"
					: $"{memoryUsed / 1048576D:#.###} MB"
			);
		}

		a(sb, f(FormattingOptions.ShowSeparators));
		return sb.ToString();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void a(StringBuilder sb, bool showSeparator)
		{
			if (showSeparator)
			{
				sb.AppendLine(new('-', 10));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool f(FormattingOptions x) => options.HasFlag(x);

		(int, Step)? getBottleneck()
		{
			if (this is not { IsSolved: true, StepsSpan: { Length: var stepsCount and not 0 } steps })
			{
				return null;
			}

			for (var i = stepsCount - 1; i >= 0; i--)
			{
				if (steps[i] is var step and not SingleStep)
				{
					return (i, step);
				}
			}

			// If code goes to here, all steps are more difficult than single techniques.
			// Get the first one is okay.
			return (0, steps[0]);
		}
	}

	/// <summary>
	/// Gets the enumerator of the current instance in order to use <see langword="foreach"/> loop.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnonymousSpanEnumerator<Step> GetEnumerator() => new(StepsSpan);

	/// <inheritdoc/>
	bool IAnyAllMethod<AnalysisResult, Step>.Any() => StepsSpan.Length != 0;

	/// <inheritdoc/>
	bool IAnyAllMethod<AnalysisResult, Step>.Any(Func<Step, bool> predicate) => Exists(predicate);

	/// <inheritdoc/>
	bool IAnyAllMethod<AnalysisResult, Step>.All(Func<Step, bool> predicate) => TrueForAll(predicate);

	/// <inheritdoc/>
	bool IReadOnlyDictionary<Grid, Step>.ContainsKey(Grid key) => HasGrid(key);

	/// <inheritdoc/>
	bool IReadOnlyDictionary<Grid, Step>.TryGetValue(Grid key, [NotNullWhen(true)] out Step? value)
	{
		foreach (ref readonly var pair in Span)
		{
			ref readonly var grid = ref pair.KeyRef();
			if (grid == key)
			{
				value = pair.Value;
				return true;
			}
		}

		value = null;
		return false;
	}

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => StepsSpan.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<KeyValuePair<Grid, Step>> IEnumerable<KeyValuePair<Grid, Step>>.GetEnumerator()
		=> Span.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Step> IEnumerable<Step>.GetEnumerator() => StepsSpan.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<Step> IWhereMethod<AnalysisResult, Step>.Where(Func<Step, bool> predicate) => this.Where(predicate).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<AnalysisResult, Step>.Select<TResult>(Func<Step, TResult> selector)
		=> this.Select(selector).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> ICastMethod<AnalysisResult, Step>.Cast<TResult>() => this.Cast<TResult>().ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> IOfTypeMethod<AnalysisResult, Step>.OfType<TResult>() => this.OfType<TResult>().ToArray();


	/// <summary>
	/// The inner executor to get the difficulty value (total, average).
	/// </summary>
	/// <param name="steps">The steps to be calculated.</param>
	/// <param name="executor">The execute method.</param>
	/// <param name="defaultRating">The default value as the return value when <see cref="StepsSpan"/> is empty.</param>
	/// <returns>The result.</returns>
	/// <seealso cref="StepsSpan"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int EvaluateRating(
		ReadOnlySpan<Step> steps,
		Func<ReadOnlySpan<Step>, Func<Step, int>, int> executor,
		int defaultRating
	) => steps.IsEmpty ? defaultRating : executor(steps, static step => step.Difficulty);
}
