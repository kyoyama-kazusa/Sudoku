namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>(Grouped) W-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="startCell">Indicates the start cell.</param>
/// <param name="endCell">Indicates the end cell.</param>
/// <param name="bridge">Indicates the bridge cells connecting with cells <see cref="StartCell"/> and <see cref="EndCell"/>.</param>
/// <param name="digitsMask">Indicates the digits used.</param>
public sealed partial class WWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Cell startCell,
	[Property] Cell endCell,
	[Property] in CellMap bridge,
	[Property] Mask digitsMask
) : IrregularWingStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override bool IsSymmetricPattern => true;

	/// <inheritdoc/>
	public override bool IsGrouped => Bridge.Count >= 3;

	/// <inheritdoc/>
	public override int BaseDifficulty => 44;

	/// <inheritdoc/>
	public override Technique Code => IsGrouped ? Technique.GroupedWWing : Technique.WWing;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [StartCellStr, EndCellStr, BridgeStr]), new(SR.ChineseLanguage, [StartCellStr, EndCellStr, BridgeStr])];

	private string StartCellStr => Options.Converter.CellConverter(in StartCell.AsCellMap());

	private string EndCellStr => Options.Converter.CellConverter(in EndCell.AsCellMap());

	private string BridgeStr => Options.Converter.CellConverter(Bridge);
}
