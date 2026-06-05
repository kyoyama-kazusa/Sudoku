namespace Sudoku.Analytics;

/// <summary>
/// Provides the result after <see cref="Analyzer"/> solving a puzzle.
/// </summary>
/// <param name="Puzzle">Indicates the original puzzle to be solved.</param>
public sealed partial record AnalysisResult(in Grid Puzzle) :
	IAnyAllMethod<AnalysisResult, Step>,
	ICastMethod<AnalysisResult, Step>,
	IEnumerable<Step>,
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


	/// <summary>
	/// Indicates whether the solver has solved the puzzle.
	/// </summary>
	[MemberNotNullWhen(true, nameof(InterimSteps), nameof(InterimGrids))]
	[MemberNotNullWhen(true, nameof(PearlStep), nameof(DiamondStep))]
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
			{ IsSolved: true, PearlStep: { Difficulty: var ep } and not SingleStep } when ep == MaxDifficulty => true,
			{ IsSolved: true } => false,
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
	/// <description>
	/// The puzzle has a unique solution, and the first deletion step has same difficulty with the whole steps.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>
	/// The puzzle has a unique solution, but the first deletion step does not have same difficulty with the whole steps.
	/// </description>
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
			{
				IsSolved: true,
				PearlStep: { Difficulty: var ep } and not SingleStep,
				DiamondStep: { Difficulty: var ed } and not SingleStep
			} when ed == MaxDifficulty && ep == ed => true,
			{ IsSolved: true } => false,
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

	/// <summary>
	/// Indicates the result sudoku grid solved. If the solver can't solve this puzzle, the value will be
	/// <see cref="Grid.Undefined"/>.
	/// </summary>
	/// <seealso cref="Grid.Undefined"/>
	public Grid Solution { get; init; }

	/// <summary>
	/// Indicates the elapsed time used during solving the puzzle. The value may not be an useful value.
	/// Some case if the puzzle doesn't contain a valid unique solution, the value may be
	/// <see cref="TimeSpan.Zero"/>.
	/// </summary>
	/// <seealso cref="TimeSpan.Zero"/>
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
	/// <remarks>
	/// This property is only meaningful when the puzzle has been solved.
	/// It is computed as follows:
	/// <list type="number">
	/// <item>
	/// If the puzzle is not solved, or if every recorded step is either a <see cref="FullHouseStep"/>
	/// or a <see cref="HiddenSingleStep"/> with <see cref="HiddenSingleStep.House"/> &lt; 9 (i.e. hidden single in block),
	/// the result is <see langword="null"/>.
	/// </item>
	/// <item>
	/// If all steps are <see cref="SingleStep"/> instances, the property returns the first step that is not a hidden single in block.
	/// </item>
	/// <item>
	/// Otherwise, the property finds the first step that is not a <see cref="SingleStep"/>,
	/// and the first step that is a <see cref="SingleStep"/>.
	/// </item>
	/// <item>
	/// If a non-first <see cref="SingleStep"/> exists,
	/// the property returns the step with the maximum difficulty in the range between those two positions.
	/// </item>
	/// <item>
	/// If the first <see cref="SingleStep"/> is at index 0, the property falls back to the first step.
	/// </item>
	/// </list>
	/// </remarks>
	/// <seealso cref="FullHouseStep"/>
	/// <seealso cref="HiddenSingleStep"/>
	/// <seealso cref="HiddenSingleStep.House"/>
	/// <seealso cref="SingleStep"/>
	public Step? PearlStep
		=> IsSolved && !StepsSpan.All(static s => s is FullHouseStep or HiddenSingleStep { House: < 9 })
			? StepsSpan.AllAre<Step, SingleStep>()
				? StepsSpan.First(static s => s is not HiddenSingleStep { House: < 9 })
				: StepsSpan.FindIndex(static s => s is not SingleStep) is var a
					? StepsSpan.FindIndex(static s => s is SingleStep) is var b and not 0
						? StepsSpan[a..b].MaxBy(static s => s.Difficulty)
						: StepsSpan[0]
					: null
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
	/// Indicates the unhandled exception thrown.
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
	IEnumerable<Grid> IReadOnlyDictionary<Grid, Step>.Keys => InterimGrids ?? [];

	/// <inheritdoc/>
	IEnumerable<Step> IReadOnlyDictionary<Grid, Step>.Values => InterimSteps ?? [];

	/// <summary>
	/// A span of values.
	/// </summary>
	private ReadOnlySpan<KeyValuePair<Grid, Step>> Span => Step.Combine(GridsSpan, StepsSpan);


	/// <summary>
	/// Gets the found <see cref="Step"/> instance whose corresponding candidates are same
	/// with the specified argument <paramref name="grid"/>.
	/// </summary>
	/// <param name="grid">The grid to be matched.</param>
	/// <returns>The found <see cref="Step"/> instance.</returns>
	/// <exception cref="InvalidOperationException">
	/// Throws when the puzzle is not solved (i.e. <see cref="IsSolved"/> property returns <see langword="false"/>).
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified puzzle cannot correspond to a paired <see cref="Step"/> instance.
	/// </exception>
	public Step this[in Grid grid]
	{
		get
		{
			if (!IsSolved)
			{
				throw new InvalidOperationException(SR.ExceptionMessage("GridMustBeSolved"));
			}

			foreach (var (g, s) in Span)
			{
				if (g == grid)
				{
					return s;
				}
			}
			throw new ArgumentOutOfRangeException(SR.ExceptionMessage("GridInvalid"));
		}
	}

	/// <summary>
	/// Gets the first found <see cref="Step"/> whose name is specified one, or nearly same as the specified one.
	/// </summary>
	/// <param name="techniqueName">Technique name.</param>
	/// <returns>The first found step.</returns>
	public KeyValuePair<Grid, Step>? this[string techniqueName]
	{
		get
		{
			if (!IsSolved)
			{
				return null;
			}

			foreach (var pair in Span)
			{
				var (_, step) = pair;
				var name = step.GetName(null);
				if (oic(name))
				{
					return pair;
				}

				var aliases = step.Code.GetAliasedNames(null);
				if (aliases is not null && Array.Exists(aliases, oic))
				{
					return pair;
				}

				var abbr = step.Code.Abbreviation;
				if (abbr is not null && oic(abbr))
				{
					return pair;
				}
			}
			return null;


			bool oic(string name) => name == techniqueName || name.Contains(techniqueName, StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// Gets a list of <see cref="Step"/>s that has the same difficulty rating value as argument <paramref name="difficultyRating"/>. 
	/// </summary>
	/// <param name="difficultyRating">The specified difficulty rating value.</param>
	/// <returns>
	/// A list of <see cref="Step"/>s found. If the puzzle cannot be solved (i.e. <see cref="IsSolved"/> returns <see langword="false"/>),
	/// the return value will be <see langword="null"/>. If the puzzle is solved, but the specified value is not found,
	/// the return value will be an empty array, rather than <see langword="null"/>. The nullability of the return value
	/// only depends on property <see cref="IsSolved"/>.
	/// </returns>
	/// <seealso cref="IsSolved"/>
	public ReadOnlySpan<Step> this[int difficultyRating] => StepsSpan.FindAll(step => step.Difficulty == difficultyRating);

	/// <summary>
	/// Gets a list of <see cref="Step"/>s that matches the specified technique.
	/// </summary>
	/// <param name="code">The specified technique code.</param>
	/// <returns>
	/// <inheritdoc cref="this[int]" path="/returns"/>
	/// </returns>
	/// <seealso cref="IsSolved"/>
	public ReadOnlySpan<Step> this[Technique code] => StepsSpan.FindAll(step => step.Code == code);

	/// <summary>
	/// Gets a list of <see cref="Step"/>s that has the same difficulty level as argument <paramref name="difficultyLevel"/>. 
	/// </summary>
	/// <param name="difficultyLevel">The specified difficulty level.</param>
	/// <returns>
	/// <inheritdoc cref="this[int]" path="/returns"/>
	/// </returns>
	/// <seealso cref="IsSolved"/>
	public ReadOnlySpan<Step> this[DifficultyLevel difficultyLevel]
		=> StepsSpan.FindAll(step => step.DifficultyLevel == difficultyLevel);


	/// <summary>
	/// Cast the object into a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TStep"/> instances.
	/// </summary>
	/// <typeparam name="TStep">The type of each element casted.</typeparam>
	/// <returns>A list of <typeparamref name="TStep"/> instances.</returns>
	public ReadOnlySpan<TStep> Cast<TStep>() where TStep : Step => from element in this select (TStep)element;

	/// <summary>
	/// Filters the current collection, preserving steps that are of type <typeparamref name="TStep"/>.
	/// </summary>
	/// <typeparam name="TStep">The type of the step you want to get.</typeparam>
	/// <returns>An array of <typeparamref name="TStep"/> instances.</returns>
	public ReadOnlySpan<TStep> OfType<TStep>() where TStep : Step
	{
		if (StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var list = new List<TStep>(stepsCount);
		foreach (var element in steps)
		{
			if (element is TStep current)
			{
				list.Add(current);
			}
		}
		return list.AsSpan();
	}

	/// <summary>
	/// Filters the current collection, preserving <see cref="Step"/> instances that are satisfied the specified condition.
	/// </summary>
	/// <param name="condition">The condition to be satisfied.</param>
	/// <returns>An array of <see cref="Step"/> instances.</returns>
	public ReadOnlySpan<Step> Where(Func<Step, bool> condition)
	{
		if (StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var result = new List<Step>(stepsCount);
		foreach (var step in steps)
		{
			if (condition(step))
			{
				result.Add(step);
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Projects the collection, to an immutable result of target type.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="selector">
	/// The selector to project the <see cref="Step"/> instance into type <typeparamref name="TResult"/>.
	/// </param>
	/// <returns>The projected collection of element type <typeparamref name="TResult"/>.</returns>
	public ReadOnlySpan<TResult> Select<TResult>(Func<Step, TResult> selector)
	{
		if (StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var arr = new TResult[stepsCount];
		var i = 0;
		foreach (var step in steps)
		{
			arr[i++] = selector(step);
		}
		return arr;
	}

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
	public bool Equals([NotNullWhen(true)] AnalysisResult? other) => other is not null && Puzzle == other.Puzzle;

	/// <inheritdoc/>
	public override int GetHashCode() => Puzzle.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => ToString(DefaultOptions);

	/// <inheritdoc cref="ToString(FormattingOptions, CoordinateConverter)"/>
	public string ToString(FormattingOptions options) => ToString(options, CoordinateConverter.InvariantCulture);

	/// <inheritdoc cref="ToString(FormattingOptions, CoordinateConverter, Func{string, Step, string}?)"/>
	public string ToString(Func<string, Step, string>? stepStringReplacer)
		=> ToString(DefaultOptions, CoordinateConverter.InvariantCulture, stepStringReplacer);

	/// <inheritdoc cref="ToString(FormattingOptions, CoordinateConverter)"/>
	public string ToString(CoordinateConverter converter) => ToString(DefaultOptions, converter);

	/// <inheritdoc cref="ToString(FormattingOptions, CoordinateConverter, Func{string, Step, string}?)"/>
	public string ToString(CoordinateConverter converter, Func<string, Step, string> stepStringReplacer)
		=> ToString(DefaultOptions, converter, stepStringReplacer);

	/// <inheritdoc cref="ToString(FormattingOptions, CoordinateConverter, Func{string, Step, string}?)"/>
	public string ToString(FormattingOptions options, Func<string, Step, string> stepStringReplacer)
		=> ToString(options, CoordinateConverter.InvariantCulture, stepStringReplacer);

	/// <summary>
	/// Returns a string that represents the current object, with the specified formatting options.
	/// </summary>
	/// <param name="options">The formatting options.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>A string that represents the current object.</returns>
	public string ToString(FormattingOptions options, CoordinateConverter converter) => ToString(options, converter, null);

	/// <summary>
	/// Returns a string that represents the current object, with the specified formatting options,
	/// with a string replacer method that can enhance the output string.
	/// </summary>
	/// <param name="options">The formatting options.</param>
	/// <param name="converter">The converter.</param>
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
	public string ToString(FormattingOptions options, CoordinateConverter converter, Func<string, Step, string>? stepStringReplacer)
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
				StepsSpan: var steps
			})
		{
			throw new UnreachableException();
		}
		var r = stepStringReplacer ?? (static (self, _) => self);
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
						: Step.CompareName(left, right, culture)
			);

			foreach (ref readonly var solvingStepsGroup in
				from step in stepsSortedByName
				select step into step
				group step by step.GetName(culture))
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

		a(sb, f(FormattingOptions.ShowSeparators));
		return sb.ToString();


		static void a(StringBuilder sb, bool showSeparator)
		{
			if (showSeparator)
			{
				sb.AppendLine(new('-', 10));
			}
		}

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
			ref readonly var grid = ref pair.KeyRef;
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
	IEnumerator IEnumerable.GetEnumerator() => StepsSpan.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<KeyValuePair<Grid, Step>> IEnumerable<KeyValuePair<Grid, Step>>.GetEnumerator()
		=> Span.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Step> IEnumerable<Step>.GetEnumerator() => StepsSpan.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<Step> IWhereMethod<AnalysisResult, Step>.Where(Func<Step, bool> predicate) => Where(predicate).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<AnalysisResult, Step>.Select<TResult>(Func<Step, TResult> selector)
		=> Select(selector).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> ICastMethod<AnalysisResult, Step>.Cast<TResult>() => Cast<TResult>().ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> IOfTypeMethod<AnalysisResult, Step>.OfType<TResult>() => OfType<TResult>().ToArray();


	/// <summary>
	/// The inner executor to get the difficulty value (total, average).
	/// </summary>
	/// <param name="steps">The steps to be calculated.</param>
	/// <param name="executor">The execute method.</param>
	/// <param name="defaultRating">The default value as the return value when <see cref="StepsSpan"/> is empty.</param>
	/// <returns>The result.</returns>
	/// <seealso cref="StepsSpan"/>
	private static int EvaluateRating(
		ReadOnlySpan<Step> steps,
		Func<ReadOnlySpan<Step>, Func<Step, int>, int> executor,
		int defaultRating
	) => steps.IsEmpty ? defaultRating : executor(steps, static step => step.Difficulty);
}
