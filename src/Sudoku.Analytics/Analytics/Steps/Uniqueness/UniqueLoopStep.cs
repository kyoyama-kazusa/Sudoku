namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="Loop" path="/summary"/></param>
/// <param name="loopPath"><inheritdoc cref="LoopPath" path="/summary"/></param>
public abstract class UniqueLoopStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap loop,
	Cell[] loopPath
) :
	UnconditionalDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait,
	ICellListTrait
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

	/// <summary>
	/// Indicates the first digit used.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <summary>
	/// Indicates the whole loop of cells used.
	/// </summary>
	public CellMap Loop { get; } = loop;

	/// <summary>
	/// Indicates the loop path.
	/// </summary>
	public Cell[] LoopPath { get; } = loopPath;

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
