namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="trueCandidates"><inheritdoc cref="TrueCandidates" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="isNaked"><inheritdoc cref="IsNaked" path="/summary"/></param>
public sealed class BivalueUniversalGraveType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CandidateMap trueCandidates,
	Mask subsetDigitsMask,
	in CellMap cells,
	bool isNaked
) :
	BivalueUniversalGraveStep(conclusions, views, options),
	IPatternType3StepTrait<BivalueUniversalGraveType3Step>
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Technique Code => Technique.BivalueUniversalGraveType3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(TrueCandidates.Digits | SubsetDigitsMask);

	/// <summary>
	/// Indicates the true candidates used.
	/// </summary>
	public CandidateMap TrueCandidates { get; } = trueCandidates;

	/// <summary>
	/// Indicates the mask of subset digits.
	/// </summary>
	public Mask SubsetDigitsMask { get; } = subsetDigitsMask;

	/// <summary>
	/// Indicates the subset cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates whether the subset is naked.
	/// </summary>
	public bool IsNaked { get; } = isNaked;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [TrueCandidatesStr, SubsetTypeStr(SR.EnglishLanguage), SizeStr, ExtraDigitsStr, CellsStr]),
			new(SR.ChineseLanguage, [TrueCandidatesStr, SubsetTypeStr(SR.ChineseLanguage), SizeStr, CellsStr, ExtraDigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BivalueUniversalGraveSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			),
			Factor.Create(
				"Factor_BivalueUniversalGraveSubsetIsHiddenFactor",
				[nameof(IPatternType3StepTrait<>.IsHidden)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<BivalueUniversalGraveType3Step>.IsHidden => !IsNaked;

	/// <inheritdoc/>
	int IPatternType3StepTrait<BivalueUniversalGraveType3Step>.SubsetSize => Size;

	/// <inheritdoc/>
	CellMap IPatternType3StepTrait<BivalueUniversalGraveType3Step>.SubsetCells => Cells;

	/// <summary>
	/// Indicates the size of the subset.
	/// </summary>
	private int Size => BitOperations.PopCount(SubsetDigitsMask);

	private string TrueCandidatesStr => Options.Converter.CandidateConverter(TrueCandidates);

	private string SizeStr => TechniqueNaming.Subset.GetSubsetName(Size);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string CellsStr => Options.Converter.CellConverter(Cells);


	private string SubsetTypeStr(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		return SR.Get(IsNaked ? "NakedKeyword" : "HiddenKeyword", culture);
	}
}
