namespace Sudoku.Runtime.InteropServices.Hodoku;

/// <summary>
/// Represents some methods that are used for get the details supported and defined
/// by another program called <see href="https://sourceforge.net/projects/hodoku/">HoDoKu</see>.
/// </summary>
public static class HodokuCompatibility
{
	/// <summary>
	/// Indicates the default library format prefix "<c>0000</c>".
	/// </summary>
	private const string DefaultLibraryFormatPrefix = "0000";

	/// <summary>
	/// Indicates the default library format digit placeholder "<c>x</c>".
	/// </summary>
	private const string DefaultLibraryFormatDigitPlaceholder = "x";


	/// <summary>
	/// Formats the current puzzle with specified step information, to output as HoDoKu library format.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="step">The corresponding step.</param>
	/// <returns>The library format.</returns>
	public static string GetHodokuLibraryFormat(in Grid grid, Step? step)
	{
		if (step is null)
		{
			return grid.ToString(
				new SusserGridFormatInfo<Grid>
				{
					IsCompatibleMode = true,
					WithModifiables = true,
					WithCandidates = true
				}
			);
		}

		var conclusionConverter = new HodokuTripletCandidateMapFormatInfo();
		var coordinateConverter = CoordinateConverter.InvariantCultureInstance;

		var codeString = GetHodokuLibraryPrefix(step.Code) ?? DefaultLibraryFormatPrefix;
		var digitsUsedString = step.DigitsUsed == 0
			? DefaultLibraryFormatDigitPlaceholder
			: coordinateConverter.DigitConverter(step.DigitsUsed);
		var gridString = grid.ToString("#");
		var conclusions = from conclusion in step.Conclusions.Span group conclusion by conclusion.ConclusionType;
		var eliminationsString = (from g in conclusions where g.Key == Elimination select g) switch
		{
			[var g, ..] => (from c in g select c.Candidate).AsCandidateMap().ToString(conclusionConverter),
			_ => string.Empty
		};
		var assignmentsString = (from g in conclusions where g.Key == Assignment select g) switch
		{
			[var g, ..] => (from c in g select c.Candidate).AsCandidateMap().ToString(conclusionConverter),
			_ => string.Empty
		};
		var extraString = step is NormalChainStep { Pattern: NamedChain { Length: var nodesLength } pattern }
			? (nodesLength - (pattern is AlternatingInferenceChain ? 1 : 0)).ToString()
			: string.Empty;
		var gridStringColonTokenIfNecessary = gridString.Contains(':') ? string.Empty : ":";
		return $":{codeString}:{digitsUsedString}:{gridString}{gridStringColonTokenIfNecessary}:{eliminationsString}:{assignmentsString}:{extraString}";
	}

	/// <summary>
	/// Try to get the prefix of the specified technique. The return value will be a four-digit value
	/// represented as a <see cref="string"/> value. For more information please visit
	/// <see href="https://sourceforge.net/p/hodoku/code/HEAD/tree/HoDoKu/trunk/reglib-1.4.txt">this link</see>.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <returns>
	/// <para>
	/// A <see cref="string"/> value using a four-digit value to describe a technique used
	/// in a sudoku grid, which is defined by the project HoDoKu.
	/// </para>
	/// <para>
	/// If this technique is not supported by HoDoKu, <see langword="null"/> will be returned.
	/// </para>
	/// </returns>
	/// <remarks>
	/// <para>
	/// Please note that some technique fields are separated into multiple sub-types, but they all
	/// belong to a same type of a technique supported by HoDoKu. For example,
	/// <see cref="Technique.SueDeCoq"/> and <see cref="Technique.SueDeCoqIsolated"/> are supported by
	/// HoDoKu as one technique "Sue de Coq", so if you get the prefix from the two fields, the return value
	/// are same (i.e. the <see cref="string"/> value <c>"1101"</c>).
	/// </para>
	/// <para>
	/// However, if a sub-type is not supported by HoDoKu (e.g. the field
	/// <see cref="Technique.UniqueRectangle2B1"/>), this method will still return <see langword="null"/>
	/// indicating the "Not supported" state;
	/// no exceptions will throw, except the value <paramref name="this"/> is not defined in the containing enumeration type.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified value is not defined by the type <see cref="Technique"/>,
	/// or the value is <see cref="Technique.None"/>.
	/// </exception>
	/// <seealso cref="Technique"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? GetHodokuLibraryPrefix(Technique @this)
		=> (@this != Technique.None && Enum.IsDefined(@this))
			? Technique.FieldInfoOf(@this) is { } fieldInfo
				? fieldInfo.GetCustomAttribute<HodokuAttribute>() is { Prefix: var prefix } ? prefix : null
				: null
			: throw new ArgumentOutOfRangeException(nameof(@this));

	/// <summary>
	/// Gets all possible aliased names that are defined by HoDoKu.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <returns>
	/// The array of aliased names, or <see langword="null"/> if it is not defined by HoDoKu.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified value is not defined by the type <see cref="Technique"/>,
	/// or the value is <see cref="Technique.None"/>.
	/// </exception>
	/// <seealso cref="Technique"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string[]? GetAliases(Technique @this)
		=> (@this != Technique.None && Enum.IsDefined(@this))
			? Technique.FieldInfoOf(@this) is { } fieldInfo
				? fieldInfo.GetCustomAttribute<HodokuAttribute>() is { Aliases: var aliases } ? aliases : null
				: null
			: throw new ArgumentOutOfRangeException(nameof(@this));

	/// <summary>
	/// Try to get difficulty score of the specified technique.
	/// </summary>
	/// <param name="this">The technique.</param>
	/// <param name="difficultyLevel">The difficulty level that is defined by HoDoKu.</param>
	/// <returns>
	/// <para>An <see cref="int"/> value defined by the project HoDoKu.</para>
	/// <para>If this technique is not supported by HoDoKu, <see langword="null"/> will be returned.</para>
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified value is not defined by the type <see cref="Technique"/>,
	/// or the value is <see cref="Technique.None"/>.
	/// </exception>
	/// <seealso cref="Technique"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: NotNullIfNotNull(nameof(difficultyLevel))]
	public static int? GetDifficultyScore(Technique @this, out HodokuDifficultyLevel? difficultyLevel)
	{
		if (@this == Technique.None || !Enum.IsDefined(@this))
		{
			throw new ArgumentOutOfRangeException(nameof(@this));
		}

		(var @return, difficultyLevel) = Technique.FieldInfoOf(@this) switch
		{
			{ } fieldInfo => fieldInfo.GetCustomAttribute<HodokuAttribute>() switch
			{
				{ Rating: var rating, DifficultyLevel: var level } => (rating, level),
				_ => ((int?)null, (HodokuDifficultyLevel?)null)
			},
			_ => default
		};
		return @return;
	}
}
