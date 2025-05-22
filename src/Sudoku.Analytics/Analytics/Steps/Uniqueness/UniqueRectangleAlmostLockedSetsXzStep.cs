namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Almost Locked Sets XZ Rule</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isIncomplete">Indicates whether the pattern is incomplete.</param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="isDoublyLinked">Indicates whether the ALS-XZ pattern is doubly-linked.</param>
/// <param name="almostLockedSet">The extra ALS.</param>
/// <param name="multivalueCellsCount">Indicates the number of multi-value cells.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangleAlmostLockedSetsXzStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	[Property] bool isIncomplete,
	bool isAvoidable,
	[Property] bool isDoublyLinked,
	[Property] AlmostLockedSetPattern almostLockedSet,
	Cell multivalueCellsCount,
	int absoluteOffset
) : UniqueRectangleStep(
	conclusions,
	views,
	options,
	(almostLockedSet.IsBivalueCell, multivalueCellsCount, isAvoidable, isDoublyLinked) switch
	{
		(true, 2, true, _) => Technique.AvoidableRectangle2D,
		(true, 3, true, _) => Technique.AvoidableRectangle3X,
		(true, 2, _, _) => Technique.UniqueRectangle2D,
		(true, 3, _, _) => Technique.UniqueRectangle3X,
		(_, _, true, true) => Technique.AvoidableRectangleDoublyLinkedAlmostLockedSetsXz,
		(_, _, true, _) => Technique.AvoidableRectangleSinglyLinkedAlmostLockedSetsXz,
		(_, _, _, true) => Technique.UniqueRectangleDoublyLinkedAlmostLockedSetsXz,
		_ => Technique.UniqueRectangleSinglyLinkedAlmostLockedSetsXz
	},
	digit1,
	digit2,
	cells,
	isAvoidable,
	absoluteOffset
)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + (IsDoublyLinked ? 7 : 6);

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | AlmostLockedSet.DigitsMask);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, AlsStr]), new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, AlsStr])];

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
				"Factor_UniqueRectangleAlmostLockedSetsXzIsIncompleteFactor",
				[nameof(IsIncomplete)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	private string AlsStr => AlmostLockedSet.ToString(Options.Converter);
}
