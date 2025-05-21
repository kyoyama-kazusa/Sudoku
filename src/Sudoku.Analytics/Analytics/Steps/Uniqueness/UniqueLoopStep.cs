namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1">Indicates the first digit used.</param>
/// <param name="digit2">Indicates the second digit used.</param>
/// <param name="loop">Indicates the whole loop of cells used.</param>
/// <param name="loopPath">Indicates the loop path.</param>
public abstract partial class UniqueLoopStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Digit digit1,
	[Property] Digit digit2,
	[Property] in CellMap loop,
	[Property] Cell[] loopPath
) : UnconditionalDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait, ICellListTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => true;

	/// <inheritdoc/>
	public override int BaseDifficulty => 45;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public override Technique Code => Technique.Parse($"UniqueLoopType{Type}");

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit1 | 1 << Digit2);

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueLoopLengthFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!) - 3
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => Loop.Count;

	private protected string LoopStr => Options.Converter.CellConverter(Loop);

	private protected string Digit1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private protected string Digit2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is UniqueLoopStep comparer
		&& (Type, Loop, Digit1, Digit2) == (comparer.Type, comparer.Loop, comparer.Digit1, comparer.Digit2)
		&& (this, comparer) switch
		{
			(UniqueLoopType3Step { SubsetDigitsMask: var a }, UniqueLoopType3Step { SubsetDigitsMask: var b }) => a == b,
			(UniqueLoopType4Step { ConjugatePair: var a }, UniqueLoopType4Step { ConjugatePair: var b }) => a == b,
			_ => true
		};

	/// <inheritdoc/>
	public override int CompareTo(Step? other) => other is UniqueLoopStep comparer ? Math.Abs(Loop.Count - comparer.Loop.Count) : 1;
}
