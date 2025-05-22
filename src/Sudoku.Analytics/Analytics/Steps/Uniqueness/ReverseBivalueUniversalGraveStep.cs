namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="CompletePattern" path="/summary"/></param>
/// <param name="emptyCells"><inheritdoc cref="EmptyCells" path="/summary"/></param>
public abstract class ReverseBivalueUniversalGraveStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap pattern,
	in CellMap emptyCells
) :
	MiscellaneousDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait,
	ICellListTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <summary>
	/// Indicates whether the pattern is a reverse UR.
	/// </summary>
	public bool IsRectangle => CompletePattern.Count == 4;

	/// <inheritdoc/>
	public override int BaseDifficulty => 60;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override Technique Code => Technique.ReverseBivalueUniversalGraveType1 + (Type - 1);

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit1 | 1 << Digit2);

	/// <summary>
	/// Indicates the first digit used.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <summary>
	/// Indicates the pattern, all possible cells included.
	/// </summary>
	public CellMap CompletePattern { get; } = pattern;

	/// <summary>
	/// Indicates the empty cells used. This cells have already included in <see cref="CompletePattern"/>.
	/// </summary>
	public CellMap EmptyCells { get; } = emptyCells;

	/// <summary>
	/// Indicates the last cells used that are not empty.
	/// </summary>
	public CellMap PatternNonEmptyCells => CompletePattern & ~EmptyCells;

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ReverseBivalueUniversalGraveSizeFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => CompletePattern.Count;

	private protected string Cell1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private protected string Cell2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));

	private protected string PatternStr => Options.Converter.CellConverter(CompletePattern);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is ReverseBivalueUniversalGraveStep comparer
		&& (Type, CompletePattern, Digit1, Digit2) == (comparer.Type, comparer.CompletePattern, comparer.Digit1, comparer.Digit2);
}
