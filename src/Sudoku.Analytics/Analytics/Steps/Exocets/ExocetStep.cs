namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Exocet</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="TargetCells" path="/summary"/></param>
/// <param name="endoTargetCells"><inheritdoc cref="EndoTargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="CrosslineCells" path="/summary"/></param>
public abstract class ExocetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells
) : FullPencilmarkingStep(conclusions, views, options)
{
	/// <summary>
	/// <para>Indicates the delta value of the pattern.</para>
	/// <para>
	/// The values can be -2, -1, 0, 1 and 2, separated with 3 groups:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>-2 or -1 <![CDATA[(< 0)]]></term>
	/// <description>The base contain more cells than the target, meaning the pattern will be a "Senior Exocet"</description>
	/// </item>
	/// <item>
	/// <term>1 or 2 <![CDATA[(> 0)]]></term>
	/// <description>
	/// The target contain more cells than the base, meaning the pattern will contain extra items like conjugate pairs of other digits
	/// </description>
	/// </item>
	/// <item>
	/// <term>0</term>
	/// <description>The base has same number of cells with the target, a standard "Junior Exocet" will be formed</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	public int Delta => TargetCells.Count - BaseCells.Count;

	/// <inheritdoc/>
	public override int BaseDifficulty => Delta switch { -2 or -1 => 96, 0 => 94, 1 or 2 => 95 };

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the mask that holds a list of digits used in the pattern.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the base cells used.
	/// </summary>
	public CellMap BaseCells { get; } = baseCells;

	/// <summary>
	/// Indicates the target cells used.
	/// </summary>
	public CellMap TargetCells { get; } = targetCells;

	/// <summary>
	/// Indicates the endo-target cells used.
	/// </summary>
	public CellMap EndoTargetCells { get; } = endoTargetCells;

	/// <summary>
	/// Indicates the cross-line cells used.
	/// </summary>
	public CellMap CrosslineCells { get; } = crosslineCells;
}
