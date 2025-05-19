namespace Sudoku.Runtime.InteropServices.SudokuExplainer;

/// <summary>
/// Represents some methods that are used for get the details supported and defined
/// by another program called
/// <see href="http://diuf.unifr.ch/pai/people/juillera/Sudoku/Sudoku.html">Sudoku Explainer</see> (Broken link).
/// </summary>
public static class SudokuExplainerCompatibility
{
	/// <summary>
	/// Gets all possible aliased names that are defined by Sudoku Explainer.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <returns>
	/// The array of aliased names, or <see langword="null"/> if it is not defined by Sudoku Explainer.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified value is not defined by the type <see cref="Technique"/>,
	/// or the value is <see cref="Technique.None"/>.
	/// </exception>
	/// <seealso cref="Technique"/>
	public static string[]? GetAliases(Technique @this)
		=> (@this != Technique.None && Enum.IsDefined(@this))
			? Technique.FieldInfoOf(@this) is { } fieldInfo
				? fieldInfo.GetCustomAttribute<SudokuExplainerAttribute>() is { Aliases: var names } ? names : null
				: null
			: throw new ArgumentOutOfRangeException(nameof(@this));

	/// <summary>
	/// Try to get the corresponding technique defined by Sudoku Explainer.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <returns>
	/// The corresponding technique defined by Sudoku Explainer. If not found, <see langword="null"/> will be returned.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SudokuExplainerTechnique? GetCorrespondingTechnique(Technique @this)
	{
		var found = Technique.FieldInfoOf(@this)?.GetCustomAttribute<SudokuExplainerAttribute>();
		return found is { Technique: var flag } ? flag : null;
	}

	/// <summary>
	/// Try to get difficulty rating of the specified technique.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <returns>
	/// <para>A <see cref="SudokuExplainerRating"/> value defined by the project Sudoku Explainer.</para>
	/// <para>If this technique is not supported by Sudoku Explainer, <see langword="null"/> will be returned.</para>
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified value is not defined by the type <see cref="Technique"/>,
	/// or the value is <see cref="Technique.None"/>.
	/// </exception>
	/// <seealso cref="Technique"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SudokuExplainerRating? GetDifficultyRatingRange(Technique @this)
		=> @this == Technique.None || !Enum.IsDefined(@this)
			? throw new ArgumentOutOfRangeException(nameof(@this))
			: (SudokuExplainerAttribute[])Technique.FieldInfoOf(@this)!.GetCustomAttributes<SudokuExplainerAttribute>() switch
			{
				[] => null,
				[{ RatingOriginal: [var min], RatingAdvanced: null }] => new(new((Half)min, (Half)min), null),
				[{ RatingOriginal: [var min, var max], RatingAdvanced: null }] => new(new((Half)min, (Half)max), null),
				[{ RatingAdvanced: [var min], RatingOriginal: null }] => new(null, new((Half)min, (Half)min)),
				[{ RatingAdvanced: [var min, var max], RatingOriginal: null }] => new(null, new((Half)min, (Half)max)),
				[{ RatingOriginal: [var min1], RatingAdvanced: [var min2] }]
					=> new(new((Half)min1, (Half)min1), new((Half)min2, (Half)min2)),
				[{ RatingOriginal: [var min1, var max1], RatingAdvanced: [var min2] }]
					=> new(new((Half)min1, (Half)max1), new((Half)min2, (Half)min2)),
				[{ RatingOriginal: [var min1], RatingAdvanced: [var min2, var max2] }]
					=> new(new((Half)min1, (Half)min1), new((Half)min2, (Half)max2)),
				[{ RatingOriginal: [var min1, var max1], RatingAdvanced: [var min2, var max2] }]
					=> new(new((Half)min1, (Half)max1), new((Half)min2, (Half)max2)),
				_ => throw new InvalidOperationException(SR.ExceptionMessage("TooMuchAttributes"))
			};
}
