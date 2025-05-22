namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Almost Locked Candidates (ALC)</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="BaseCells" path="/summary"/></param>
/// <param name="coverCells"><inheritdoc cref="CoverCells" path="/summary"/></param>
/// <param name="hasValueCell"><inheritdoc cref="HasValueCell" path="/summary"/></param>
public sealed class AlmostLockedCandidatesStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap coverCells,
	bool hasValueCell
) : IntersectionStep(conclusions, views, options), ISizeTrait
{
	/// <summary>
	/// Indicates whether the step contains value cells.
	/// </summary>
	public bool HasValueCell { get; } = hasValueCell;

	/// <inheritdoc/>
	public override int BaseDifficulty => 45;

	/// <inheritdoc/>
	public int Size => BitOperations.PopCount(DigitsMask);

	/// <inheritdoc/>
	public override Technique Code
		=> (HasValueCell, Size) switch
		{
			(_, 2) => Technique.AlmostLockedPair,
			(true, 3) => Technique.AlmostLockedTripleValueType,
			(_, 3) => Technique.AlmostLockedTriple,
			(true, 4) => Technique.AlmostLockedQuadrupleValueType,
			(_, 4) => Technique.AlmostLockedQuadruple
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the mask that contains the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the cells in base set.
	/// </summary>
	public CellMap BaseCells { get; } = baseCells;

	/// <summary>
	/// Indicates the cells in cover set.
	/// </summary>
	public CellMap CoverCells { get; } = coverCells;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, BaseCellsStr, TargetCellsStr]), new(SR.ChineseLanguage, [DigitsStr, BaseCellsStr, TargetCellsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_AlmostLockedCandidatesSizeFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! switch { 2 => 0, 3 => 7, 4 => 12 }
			),
			Factor.Create(
				"Factor_AlmostLockedCandidatesValueCellExistenceFactor",
				[nameof(HasValueCell), nameof(Size)],
				GetType(),
				static args => (bool)args![0]! ? (int)args![1]! switch { 2 or 3 => 1, 4 => 2 } : 0
			)
		];

	private string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private string BaseCellsStr => Options.Converter.CellConverter(BaseCells);

	private string TargetCellsStr => Options.Converter.CellConverter(CoverCells);
}
