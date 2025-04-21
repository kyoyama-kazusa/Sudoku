namespace Sudoku.Analytics;

public partial record AnalysisResult
{
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


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
}
