namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Regular Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="branches">Indicates the branches used.</param>
/// <param name="petals">Indicates the petals used.</param>
/// <param name="extraDigitsMask">Indicates the mask that contains all extra digits.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangleRegularWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	[Property] in CellMap branches,
	[Property] in CellMap petals,
	[Property] Mask extraDigitsMask,
	int absoluteOffset
) : UniqueRectangleStep(conclusions, views, options, code, digit1, digit2, cells, isAvoidable, absoluteOffset)
{
	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, BranchesStr, SubsetDigitsStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, BranchesStr, SubsetDigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_RectangleIsAvoidableFactor",
				[nameof(IsAvoidable)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			),
			Factor.Create(
				"Factor_UniqueRectangleWingSizeFactor",
				[nameof(Code)],
				GetType(),
				static args => (Technique)args![0]! switch
				{
					Technique.UniqueRectangleXyWing or Technique.AvoidableRectangleXyWing => 2,
					Technique.UniqueRectangleXyzWing or Technique.AvoidableRectangleXyzWing => 3,
					Technique.UniqueRectangleWxyzWing or Technique.AvoidableRectangleWxyzWing => 5
				}
			)
		];

	private string BranchesStr => Options.Converter.CellConverter(Branches);

	private string SubsetDigitsStr => Options.Converter.DigitConverter(ExtraDigitsMask);
}
