namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Chromatic Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="blocks"><inheritdoc cref="Blocks" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="Pattern" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public abstract class ChromaticPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House[] blocks,
	in CellMap pattern,
	Mask digitsMask
) : InvalidityStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 65;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the blocks that the current pattern lies in.
	/// </summary>
	public House[] Blocks { get; } = blocks;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Pattern { get; } = pattern;

	/// <summary>
	/// Indicates the mask of digits.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	private protected string BlocksStr => Options.Converter.HouseConverter(Blocks.Aggregate(@delegate.BitMerger));

	private protected string CellsStr => Options.Converter.CellConverter(Pattern);

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);
}
