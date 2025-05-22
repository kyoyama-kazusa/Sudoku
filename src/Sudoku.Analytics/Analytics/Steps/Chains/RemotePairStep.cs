namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>(Complex) Remote Pair</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="isComplex"><inheritdoc cref="IsComplex" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class RemotePairStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	bool isComplex,
	Mask digitsMask
) : SpecializedChainStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override bool IsMultiple => false;

	/// <inheritdoc/>
	public override bool IsDynamic => Code == Technique.ComplexRemotePair;

	/// <summary>
	/// Indicates whether the pattern is complex.
	/// </summary>
	public bool IsComplex { get; } = isComplex;

	/// <inheritdoc/>
	public override int Complexity => Cells.Count;

	/// <inheritdoc/>
	public override int BaseDifficulty => IsComplex ? 50 : 52;

	/// <inheritdoc/>
	public override Technique Code => IsComplex ? Technique.ComplexRemotePair : Technique.RemotePair;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [CellsStr, FirstUnknownCharacterString, SecondUnknownCharacterString]),
			new(SR.ChineseLanguage, [CellsStr, FirstUnknownCharacterString, SecondUnknownCharacterString])
		];

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string FirstUnknownCharacterString => UnknownCharacters[0].ToString();

	private string SecondUnknownCharacterString => UnknownCharacters[1].ToString();

	private ReadOnlySpan<char> UnknownCharacters => Options.BabaGroupInitialLetter.GetSequence(Options.BabaGroupLetterCasing);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is RemotePairStep comparer && Cells == comparer.Cells && IsComplex == comparer.IsComplex;
}
