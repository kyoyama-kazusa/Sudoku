namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Avoidable Rectangle Hidden Single</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="baseCell">Indicates the base cell used.</param>
/// <param name="targetCell">Indicates the target cell used.</param>
/// <param name="house">Indicates the house where the pattern lies.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class AvoidableRectangleHiddenSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	[Property] Cell baseCell,
	[Property] Cell targetCell,
	[Property] House house,
	int absoluteOffset
) : UniqueRectangleStep(
	conclusions,
	views,
	options,
	(Technique)((int)Technique.AvoidableRectangleHiddenSingleBlock + (int)house.HouseType),
	digit1,
	digit2,
	cells,
	true,
	absoluteOffset
)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, BaseCellStr, HouseStr, TargetCellStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, BaseCellStr, HouseStr, TargetCellStr])
		];

	private string BaseCellStr => Options.Converter.CellConverter(in BaseCell.AsCellMap());

	private string TargetCellStr => Options.Converter.CellConverter(in TargetCell.AsCellMap());

	private string HouseStr => Options.Converter.HouseConverter(1 << House);
}
