namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>(Siamese) (Grouped) XYZ-Ring</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="intersectDigit"><inheritdoc cref="IntersectDigit" path="/summary"/></param>
/// <param name="pivot"><inheritdoc cref="Pivot" path="/summary"/></param>
/// <param name="leafCell1"><inheritdoc cref="LeafCell1" path="/summary"/></param>
/// <param name="leafCell2"><inheritdoc cref="LeafCell2" path="/summary"/></param>
/// <param name="xyzDigitsMask"><inheritdoc cref="XyzDigitsMask" path="/summary"/></param>
/// <param name="conjugateHousesMask"><inheritdoc cref="ConjugateHousesMask" path="/summary"/></param>
/// <param name="isNice"><inheritdoc cref="IsNice" path="/summary"/></param>
/// <param name="isGrouped"><inheritdoc cref="IsGrouped" path="/summary"/></param>
/// <param name="isSiamese"><inheritdoc cref="IsSiamese" path="/summary"/></param>
public sealed class XyzRingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit intersectDigit,
	Cell pivot,
	Cell leafCell1,
	Cell leafCell2,
	Mask xyzDigitsMask,
	HouseMask conjugateHousesMask,
	bool isNice,
	bool isGrouped,
	bool isSiamese = false
) : WingStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates whether the pattern is a nice loop.
	/// </summary>
	public bool IsNice { get; } = isNice;

	/// <summary>
	/// Indicates whether the conjugate pair is grouped one.
	/// </summary>
	public bool IsGrouped { get; } = isGrouped;

	/// <summary>
	/// Indicates whether the XYZ-loop is a Siamese one.
	/// </summary>
	public bool IsSiamese { get; } = isSiamese;

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
	/// Indicates the digit Z for XYZ-Wing pattern.
	/// </summary>
	public Digit IntersectDigit { get; } = intersectDigit;

	/// <summary>
	/// Indicates the pivot cell.
	/// </summary>
	public Cell Pivot { get; } = pivot;

	/// <summary>
	/// Indicates the leaf cell 1.
	/// </summary>
	public Cell LeafCell1 { get; } = leafCell1;

	/// <summary>
	/// Indicates the leaf cell 2.
	/// </summary>
	public Cell LeafCell2 { get; } = leafCell2;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask XyzDigitsMask { get; } = xyzDigitsMask;

	/// <summary>
	/// Indicates the conjugate houses used.
	/// </summary>
	public HouseMask ConjugateHousesMask { get; } = conjugateHousesMask;

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
