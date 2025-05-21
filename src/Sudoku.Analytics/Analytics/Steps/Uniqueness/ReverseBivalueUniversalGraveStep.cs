namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1">Indicates the first digit used.</param>
/// <param name="digit2">Indicates the second digit used.</param>
/// <param name="pattern">Indicates the pattern, all possible cells included.</param>
/// <param name="emptyCells">Indicates the empty cells used. This cells have already included in <paramref name="pattern"/>.</param>
public abstract partial class ReverseBivalueUniversalGraveStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Digit digit1,
	[Property] Digit digit2,
	[Property(NamingRule = "Complete>@")] in CellMap pattern,
	[Property] in CellMap emptyCells
) : MiscellaneousDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait, ICellListTrait
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
