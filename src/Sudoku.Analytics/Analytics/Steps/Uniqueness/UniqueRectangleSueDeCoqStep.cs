namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Sue de Coq</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
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
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class UniqueRectangleSueDeCoqStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	BlockIndex block,
	House line,
	Mask blockMask,
	Mask lineMask,
	Mask intersectionMask,
	bool isCannibalism,
	Mask isolatedDigitsMask,
	in CellMap blockCells,
	in CellMap lineCells,
	in CellMap intersectionCells,
	int absoluteOffset
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		isAvoidable ? Technique.AvoidableRectangleSueDeCoq : Technique.UniqueRectangleSueDeCoq,
		digit1,
		digit2,
		cells,
		isAvoidable,
		absoluteOffset
	),
	IIsolatedDigitTrait
{
	/// <summary>
	/// Indicates whether the Sue de Coq pattern is a cannibalism.
	/// </summary>
	public bool IsCannibalism { get; } = isCannibalism;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 5;

	/// <inheritdoc/>
	public override Mask DigitsUsed
		=> (Mask)(base.DigitsUsed | (Mask)((Mask)((Mask)(BlockMask | LineMask) | IntersectionMask) | IsolatedDigitsMask));

	/// <summary>
	/// Indicates the block index that the Sue de Coq pattern used.
	/// </summary>
	public BlockIndex Block { get; } = block;

	/// <summary>
	/// Indicates the line (row or column) index that the Sue de Coq pattern used.
	/// </summary>
	public House Line { get; } = line;

	/// <summary>
	/// Indicates the mask that contains all digits from the block of the Sue de Coq pattern.
	/// </summary>
	public Mask BlockMask { get; } = blockMask;

	/// <summary>
	/// Indicates the cells in the line of the Sue de Coq pattern.
	/// </summary>
	public Mask LineMask { get; } = lineMask;

	/// <summary>
	/// Indicates the mask that contains all digits from the intersection of houses <see cref="Block"/> and <see cref="Line"/>.
	/// </summary>
	public Mask IntersectionMask { get; } = intersectionMask;

	/// <summary>
	/// Indicates the mask that contains all isolated digits.
	/// </summary>
	public Mask IsolatedDigitsMask { get; } = isolatedDigitsMask;

	/// <summary>
	/// Indicates the cells in the block of the Sue de Coq pattern.
	/// </summary>
	public CellMap BlockCells { get; } = blockCells;

	/// <summary>
	/// Indicates the cells in the line (row or column) of the Sue de Coq pattern.
	/// </summary>
	public CellMap LineCells { get; } = lineCells;

	/// <summary>
	/// Indicates the cells in the intersection from houses <see cref="Block"/> and <see cref="Line"/>.
	/// </summary>
	public CellMap IntersectionCells { get; } = intersectionCells;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, MergedCellsStr, SueDeCoqDigitsMask]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, MergedCellsStr, SueDeCoqDigitsMask])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_RectangleSueDeCoqIsolatedFactor",
				[nameof(IsCannibalism), nameof(IIsolatedDigitTrait.ContainsIsolatedDigits)],
				GetType(),
				static args => !(bool)args![0]! && (bool)args![1]! ? 1 : 0
			),
			Factor.Create(
				"Factor_RectangleSueDeCoqCannibalismFactor",
				[nameof(IsCannibalism)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			),
			Factor.Create(
				"Factor_RectangleIsAvoidableFactor",
				[nameof(IsAvoidable)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	bool IIsolatedDigitTrait.ContainsIsolatedDigits => IsolatedDigitsMask != 0;

	/// <inheritdoc/>
	int IIsolatedDigitTrait.IsolatedDigitsCount => IsolatedDigitsMask == 0 ? 0 : BitOperations.PopCount(IsolatedDigitsMask);

	private string MergedCellsStr => Options.Converter.CellConverter(LineCells | BlockCells);

	private string SueDeCoqDigitsMask => Options.Converter.DigitConverter((Mask)(LineMask | BlockMask));
}
