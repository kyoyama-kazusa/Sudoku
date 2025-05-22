namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Sue de Coq</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="block"><inheritdoc cref="Block" path="/summary"/></param>
/// <param name="line"><inheritdoc cref="Line" path="/summary"/></param>
/// <param name="blockMask"><inheritdoc cref="BlockMask" path="/summary"/></param>
/// <param name="lineMask"><inheritdoc cref="LineMask" path="/summary"/></param>
/// <param name="intersectionMask"><inheritdoc cref="IntersectionMask" path="/summary"/></param>
/// <param name="isCannibalism"><inheritdoc cref="IsCannibalism" path="/summary"/></param>
/// <param name="isolatedDigitsMask"><inheritdoc cref="IsolatedDigitsMask" path="/summary"/></param>
/// <param name="blockCells"><inheritdoc cref="BlockCells" path="/summary"/></param>
/// <param name="lineCells"><inheritdoc cref="LineCells" path="/summary"/></param>
/// <param name="intersectionCells"><inheritdoc cref="IntersectionCells" path="/summary"/></param>
public sealed class SueDeCoqStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House block,
	House line,
	Mask blockMask,
	Mask lineMask,
	Mask intersectionMask,
	bool isCannibalism,
	Mask isolatedDigitsMask,
	in CellMap blockCells,
	in CellMap lineCells,
	in CellMap intersectionCells
) :
	LockedSetStep(conclusions, views, options),
	IIsolatedDigitTrait
{
	/// <summary>
	/// Indicates whether the current pattern is cannibalism.
	/// </summary>
	public bool IsCannibalism { get; } = isCannibalism;

	/// <inheritdoc/>
	public override int BaseDifficulty => 50;

	/// <inheritdoc/>
	public override Technique Code
		=> (IsCannibalism, IsolatedDigitsMask) switch
		{
			(true, _) => Technique.SueDeCoqCannibalism,
			(_, not 0) => Technique.SueDeCoqIsolated,
			_ => Technique.SueDeCoq
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)((Mask)((Mask)(BlockMask | LineMask) | IntersectionMask) | IsolatedDigitsMask);

	/// <summary>
	/// Indicates the block index that the current pattern used.
	/// </summary>
	public House Block { get; } = block;

	/// <summary>
	/// Indicates the line (row or column) index that the current pattern used.
	/// </summary>
	public House Line { get; } = line;

	/// <summary>
	/// Indicates the block mask.
	/// </summary>
	public Mask BlockMask { get; } = blockMask;

	/// <summary>
	/// Indicates the line mask.
	/// </summary>
	public Mask LineMask { get; } = lineMask;

	/// <summary>
	/// Indicates the intersection mask.
	/// </summary>
	public Mask IntersectionMask { get; } = intersectionMask;

	/// <summary>
	/// The isolated digits mask.
	/// </summary>
	public Mask IsolatedDigitsMask { get; } = isolatedDigitsMask;

	/// <summary>
	/// Indicates the cells that the current pattern used in a block.
	/// </summary>
	public CellMap BlockCells { get; } = blockCells;

	/// <summary>
	/// Indicates the cells that the current pattern used in a line (row or column).
	/// </summary>
	public CellMap LineCells { get; } = lineCells;

	/// <summary>
	/// Indicates the cells that the current pattern used
	/// in an intersection of <see cref="BlockCells"/> and <see cref="LineCells"/>.
	/// </summary>
	public CellMap IntersectionCells { get; } = intersectionCells;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(
				SR.EnglishLanguage,
				[IntersectionCellsStr, IntersectionDigitsStr, BlockCellsStr, BlockDigitsStr, LineCellsStr, LineDigitsStr]
			),
			new(
				SR.ChineseLanguage,
				[IntersectionCellsStr, IntersectionDigitsStr, BlockCellsStr, BlockDigitsStr, LineCellsStr, LineDigitsStr]
			)
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_SueDeCoqIsolatedFactor",
				[nameof(IIsolatedDigitTrait.ContainsIsolatedDigits)],
				GetType(),
				static args => (bool)args![0]! ? 2 : 0
			),
			Factor.Create(
				"Factor_SueDeCoqCannibalismFactor",
				[nameof(IsCannibalism)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	bool IIsolatedDigitTrait.ContainsIsolatedDigits => IsolatedDigitsMask != 0;

	/// <inheritdoc/>
	int IIsolatedDigitTrait.IsolatedDigitsCount => IsolatedDigitsMask == 0 ? 0 : BitOperations.PopCount(IsolatedDigitsMask);

	private string IntersectionCellsStr => Options.Converter.CellConverter(IntersectionCells);

	private string IntersectionDigitsStr
		=> new LiteralCoordinateConverter(DigitsSeparator: string.Empty).DigitConverter(IntersectionMask);

	private string BlockCellsStr => Options.Converter.CellConverter(BlockCells);

	private string BlockDigitsStr => new LiteralCoordinateConverter(DigitsSeparator: string.Empty).DigitConverter(BlockMask);

	private string LineCellsStr => Options.Converter.CellConverter(LineCells);

	private string LineDigitsStr => new LiteralCoordinateConverter(DigitsSeparator: string.Empty).DigitConverter(LineMask);
}
