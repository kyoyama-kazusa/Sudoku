namespace Sudoku.Analytics.Steps.Permutations;

/// <summary>
/// Provides with a step that is an <b>Aligned Exclusion</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells">Indicates the target cells used in the pattern.</param>
/// <param name="digitsMask">Indicates the digits used.</param>
/// <param name="lockedCombinations">Indicates all locked combinations.</param>
public sealed partial class AlignedExclusionStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap cells,
	[Property] Mask digitsMask,
	[Property] (Digit[], Cell)[] lockedCombinations
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

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.ChineseLanguage, [CellsStr, ConclusionNegatedStr]), new(SR.EnglishLanguage, [CellsStr, ConclusionNegatedStr])];

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string ConclusionNegatedStr => Options.Converter.ConclusionConverter(from c in Conclusions.Span select ~c);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is AlignedExclusionStep comparer && Cells == comparer.Cells;
}
