namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents for a parser instance that parses a <see cref="string"/> text,
/// converting into a valid instance that can be represented as a sudoku concept.
/// </summary>
public abstract record CoordinateParser : ICoordinateProvider<CoordinateParser>
{
	/// <summary>
	/// The parser method that can creates a <see cref="CellMap"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="CellMap"/>
	public abstract Func<string, CellMap> CellParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="CandidateMap"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="CandidateMap"/>
	public abstract Func<string, CandidateMap> CandidateParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="HouseMask"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="HouseMask"/>
	public abstract Func<string, HouseMask> HouseParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="Conclusion"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Conclusion"/>
	public abstract Func<string, ConclusionSet> ConclusionParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="Mask"/> via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Mask"/>
	public abstract Func<string, Mask> DigitParser { get; }

	/// <summary>
	/// The parser method that can creates a list of pairs of <see cref="MinilineBase"/> and <see cref="MinilineResult"/>
	/// via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="MinilineBase"/>
	/// <seealso cref="MinilineResult"/>
	public abstract Func<string, ReadOnlySpan<Miniline>> IntersectionParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="Chute"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Chute"/>
	public abstract Func<string, ReadOnlySpan<Chute>> ChuteParser { get; }

	/// <summary>
	/// The parser method that can creates a <see cref="Conjugate"/> list via the specified text to be parsed.
	/// </summary>
	/// <seealso cref="Conjugate"/>
	public abstract Func<string, ReadOnlySpan<Conjugate>> ConjugateParser { get; }


	/// <inheritdoc/>
	public static CoordinateParser InvariantCultureInstance => new RxCyParser();


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public abstract object? GetFormat(Type? formatType);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CoordinateParser GetInstance(IFormatProvider? formatProvider)
		=> formatProvider switch
		{
			CultureInfo { Name: var name } when name.CultureNameEqual(SR.ChineseLanguage) => new K9Parser(),
			CoordinateParser c => c,
			_ => InvariantCultureInstance
		};
}
