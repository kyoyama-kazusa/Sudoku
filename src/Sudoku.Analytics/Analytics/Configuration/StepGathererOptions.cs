namespace Sudoku.Analytics.Configuration;

/// <summary>
/// <para>
/// Represents a type that encapsulates a list of options adjusted by users,
/// consumed by <see cref="Analyzer"/> or <see cref="Collector"/>.
/// </para>
/// <para>
/// Some options may not relate to a real <see cref="StepSearcher"/> instance directly,
/// but relate to a <see cref="Step"/> that a <see cref="StepSearcher"/> instance can create.
/// For example, setting notation to the coordinates.
/// </para>
/// </summary>
/// <seealso cref="StepGatherer"/>
/// <seealso cref="StepSearcher"/>
/// <seealso cref="Analyzer"/>
/// <seealso cref="Collector"/>
public sealed record StepGathererOptions
{
	/// <summary>
	/// Indicates whether the current solver uses direct mode to solve a puzzle,
	/// meaning UI will display the grid without any candidates.
	/// </summary>
	public bool IsDirectMode { get; init; } = false;

	/// <summary>
	/// Indicates whether the step searcher will checks for hidden singles in rows or columns
	/// if <see cref="PrimarySingle"/> property has flag <see cref="SingleTechniqueFlag.HiddenSingle"/>.
	/// </summary>
	/// <seealso cref="SingleTechniqueFlag.HiddenSingle"/>
	public bool PrimaryHiddenSingleAllowsLines { get; init; } = false;

	/// <summary>
	/// Indicates whether the searching module will use ittoryu mode to calculate steps.
	/// </summary>
	public bool UseIttoryuMode { get; init; } = false;

	/// <summary>
	/// Indicates the preferred single technique.
	/// </summary>
	/// <remarks>
	/// <para>
	/// If the value isn't <see cref="SingleTechniqueFlag.None"/>,
	/// the analyzer will ignore the other single technique not related to this field;
	/// in other words, the analyzer module will only perform usages of the specified technique,
	/// determining whether the puzzle can be finished by only use this technique.
	/// </para>
	/// <para>
	/// For example, if the value is <see cref="SingleTechniqueFlag.NakedSingle"/>,
	/// the analyzer will only use naked singles to finish the puzzle.
	/// If the puzzle cannot be solved, the analyzer will return an <see cref="AnalysisResult"/> with
	/// <see cref="AnalysisResult.IsSolved"/> a <see langword="false"/> value.
	/// </para>
	/// <para>
	/// Please note that if hidden single flag is chosen <see cref="SingleTechniqueFlag.HiddenSingle"/>,
	/// full houses will also be used; this is an only exception.
	/// </para>
	/// </remarks>
	/// <seealso cref="SingleTechniqueFlag"/>
	/// <seealso cref="AnalysisResult.IsSolved"/>
	public SingleTechniqueFlag PrimarySingle { get; init; } = SingleTechniqueFlag.None;

	/// <summary>
	/// Indicates the initial letter to be used and displayed in a <see cref="BabaGroupViewNode"/>.
	/// </summary>
	/// <seealso cref="BabaGroupViewNode"/>
	public BabaGroupInitialLetter BabaGroupInitialLetter { get; init; } = BabaGroupInitialLetter.EnglishLetter_X;

	/// <summary>
	/// Indicates letter casing of characters in <see cref="BabaGroupViewNode"/> should be displayed.
	/// </summary>
	/// <seealso cref="BabaGroupViewNode"/>
	public BabaGroupLetterCasing BabaGroupLetterCasing { get; init; } = BabaGroupLetterCasing.Lower;

	/// <summary>
	/// Indicates the default link option.
	/// </summary>
	public LinkOption DefaultLinkOption { get; init; } = LinkOption.House;

	/// <inheritdoc cref="CoordinateConverter"/>
	public CoordinateConverter Converter { get; init; } = CoordinateConverter.InvariantCultureInstance;

	/// <summary>
	/// Indicates the current culture used.
	/// </summary>
	public CultureInfo CurrentCulture => Converter.CurrentCulture ?? CultureInfo.CurrentUICulture;

	/// <summary>
	/// Indicates the link options overridden.
	/// </summary>
	public IDictionary<LinkType, LinkOption>? OverriddenLinkOptions { get; set; } = new Dictionary<LinkType, LinkOption>
	{
		{ LinkType.KrakenNormalFish, LinkOption.None },
		{ LinkType.XyzWing, LinkOption.None }
	};


	/// <summary>
	/// Represents a default instance, with all property having been initialized with default state.
	/// </summary>
	/// <remarks>
	/// This default option makes the internal members be:
	/// <list type="bullet">
	/// <item><see cref="Converter"/>: <see langword="new"/> <see cref="RxCyConverter"/>()</item>
	/// <item><see cref="IsDirectMode"/>: <see langword="false"/></item>
	/// <item><see cref="UseIttoryuMode"/>: <see langword="false"/></item>
	/// <item><see cref="PrimarySingle"/>: <see cref="SingleTechniqueFlag.None"/></item>
	/// <item><see cref="PrimaryHiddenSingleAllowsLines"/>: <see langword="false"/></item>
	/// <item><see cref="BabaGroupInitialLetter"/>: <see cref="BabaGroupInitialLetter.EnglishLetter_X"/></item>
	/// <item><see cref="BabaGroupLetterCasing"/>: <see cref="BabaGroupLetterCasing.Lower"/></item>
	/// <item><see cref="DefaultLinkOption"/>: <see cref="LinkOption.House"/></item>
	/// <item>
	/// <see cref="OverriddenLinkOptions"/>:
	/// [<see cref="LinkType.KrakenNormalFish"/>: <see cref="LinkOption.None"/>,
	/// <see cref="LinkType.XyzWing"/>: <see cref="LinkOption.None"/>]
	/// </item>
	/// </list>
	/// </remarks>
	public static StepGathererOptions Default => new();


	/// <summary>
	/// Try to get the link option for the specified link type.
	/// </summary>
	/// <param name="linkType">The link type.</param>
	/// <returns>
	/// The link option returned. If there's no overridden link option, return <see cref="DefaultLinkOption"/>.
	/// </returns>
	/// <seealso cref="DefaultLinkOption"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LinkOption GetLinkOption(LinkType linkType)
		=> OverriddenLinkOptions is { } p && p.TryGetValue(linkType, out var lo) ? lo : DefaultLinkOption;
}
