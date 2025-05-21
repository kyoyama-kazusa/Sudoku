namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>(Siamese) (Grouped) XYZ-Ring</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="intersectDigit">Indicates the digit Z for XYZ-Wing pattern.</param>
/// <param name="pivot">Indicates the pivot cell.</param>
/// <param name="leafCell1">Indicates the leaf cell 1.</param>
/// <param name="leafCell2">Indicates the leaf cell 2.</param>
/// <param name="xyzDigitsMask">Indicates the digits used.</param>
/// <param name="conjugateHousesMask">Indicates the conjugate houses used.</param>
/// <param name="isNice">Indicates whether the pattern is a nice loop.</param>
/// <param name="isGrouped">Indicates whether the conjugate pair is grouped one.</param>
/// <param name="isSiamese">Indicates whether the XYZ-loop is a Siamese one.</param>
public sealed partial class XyzRingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Digit intersectDigit,
	[Property] Cell pivot,
	[Property] Cell leafCell1,
	[Property] Cell leafCell2,
	[Property] Mask xyzDigitsMask,
	[Property] HouseMask conjugateHousesMask,
	[Property] bool isNice,
	[Property] bool isGrouped,
	[Property] bool isSiamese = false
) : WingStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => IsNice ? 50 : 52;

	/// <inheritdoc/>
	public override Technique Code
		=> (IsSiamese, IsGrouped, IsNice) switch
		{
			(true, true, true) => Technique.SiameseGroupedXyzNiceLoop,
			(_, true, true) => Technique.GroupedXyzNiceLoop,
			(true, true, _) => Technique.SiameseGroupedXyzLoop,
			(_, true, _) => Technique.GroupedXyzLoop,
			(true, _, true) => Technique.SiameseXyzNiceLoop,
			(_, _, true) => Technique.XyzNiceLoop,
			(true, _, _) => Technique.SiameseXyzLoop,
			_ => Technique.XyzLoop
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => XyzDigitsMask;

	/// <summary>
	/// Indicates the pattern.
	/// </summary>
	private CellMap Pattern => Pivot.AsCellMap() + LeafCell1 + LeafCell2;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is XyzRingStep comparer
		&& Pattern == comparer.Pattern && IntersectDigit == comparer.IntersectDigit && IsGrouped == comparer.IsGrouped
		&& IsNice == comparer.IsNice && IsSiamese == comparer.IsSiamese;
}
