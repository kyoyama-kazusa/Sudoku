namespace Sudoku.Analytics.Steps.Permutations;

/// <summary>
/// Provides with a step that is an <b>Aligned Exclusion</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="lockedCombinations"><inheritdoc cref="LockedCombinations" path="/summary"/></param>
public sealed class AlignedExclusionStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	(Digit[], Cell)[] lockedCombinations
) : PermutationStep(conclusions, views, options), ISizeTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> Size switch
		{
			2 => 62,
			3 => 75,
			4 => 81,
			5 => 84,
			_ => throw new NotSupportedException(SR.ExceptionMessage("SubsetSizeExceeds"))
		};

	/// <inheritdoc/>
	public int Size => Cells.Count;

	/// <inheritdoc/>
	public override Technique Code
		=> Size switch
		{
			>= 2 and <= 5 => Technique.AlignedPairExclusion + (Size - 2),
			_ => throw new NotSupportedException(SR.ExceptionMessage("SubsetSizeExceeds"))
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the target cells used in the pattern.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates all locked combinations.
	/// </summary>
	public (Digit[], Cell)[] LockedCombinations { get; } = lockedCombinations;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.ChineseLanguage, [CellsStr, ConclusionNegatedStr]), new(SR.EnglishLanguage, [CellsStr, ConclusionNegatedStr])];

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string ConclusionNegatedStr => Options.Converter.ConclusionConverter(from c in Conclusions.Span select ~c);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is AlignedExclusionStep comparer && Cells == comparer.Cells;
}
