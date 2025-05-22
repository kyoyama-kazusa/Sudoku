namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Avoidable Rectangle Hidden Single</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="baseCell"><inheritdoc cref="BaseCell" path="/summary"/></param>
/// <param name="targetCell"><inheritdoc cref="TargetCell" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class AvoidableRectangleHiddenSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	Cell baseCell,
	Cell targetCell,
	House house,
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

	/// <summary>
	/// Indicates the base cell used.
	/// </summary>
	public Cell BaseCell { get; } = baseCell;

	/// <summary>
	/// Indicates the target cell used.
	/// </summary>
	public Cell TargetCell { get; } = targetCell;

	/// <summary>
	/// Indicates the house where the pattern lies.
	/// </summary>
	public House House { get; } = house;

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
