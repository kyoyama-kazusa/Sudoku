namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>(Grouped) W-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="startCell"><inheritdoc cref="StartCell" path="/summary"/></param>
/// <param name="endCell"><inheritdoc cref="EndCell" path="/summary"/></param>
/// <param name="bridge"><inheritdoc cref="Bridge" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class WWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell startCell,
	Cell endCell,
	in CellMap bridge,
	Mask digitsMask
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

	/// <summary>
	/// Indicates the start cell.
	/// </summary>
	public Cell StartCell { get; } = startCell;

	/// <summary>
	/// Indicates the end cell.
	/// </summary>
	public Cell EndCell { get; } = endCell;

	/// <summary>
	/// Indicates the bridge cells connecting with cells <see cref="StartCell"/> and <see cref="EndCell"/>.
	/// </summary>
	public CellMap Bridge { get; } = bridge;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [StartCellStr, EndCellStr, BridgeStr]), new(SR.ChineseLanguage, [StartCellStr, EndCellStr, BridgeStr])];

	private string StartCellStr => Options.Converter.CellConverter(in StartCell.AsCellMap());

	private string EndCellStr => Options.Converter.CellConverter(in EndCell.AsCellMap());

	private string BridgeStr => Options.Converter.CellConverter(Bridge);
}
