namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Oddagon</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="loopCells"><inheritdoc cref="LoopCells" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
public abstract class BivalueOddagonStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap loopCells,
	Digit digit1,
	Digit digit2
) : InvalidityStep(conclusions, views, options), ICellListTrait
{
	/// <summary>
	/// Indicates the type of the technique.
	/// </summary>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public override int BaseDifficulty => 63;

	/// <inheritdoc/>
	public sealed override Technique Code => Technique.Parse($"BivalueOddagonType{Type}");

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit1 | 1 << Digit2);

	/// <summary>
	/// Indicates the loop of cells used.
	/// </summary>
	public CellMap LoopCells { get; } = loopCells;

	/// <summary>
	/// Indicates the first digit used.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BivalueOddagonLengthFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			)
		];

	private protected string LoopStr => Options.Converter.CellConverter(LoopCells);

	/// <inheritdoc/>
	int ICellListTrait.CellSize => LoopCells.Count;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is BivalueOddagonStep comparer
		&& (Type, Digit1, Digit2, LoopCells) == (comparer.Type, comparer.Digit1, comparer.Digit2, comparer.LoopCells);

	/// <inheritdoc/>
	public override int CompareTo(Step? other)
	{
		if (other is not BivalueOddagonStep comparer)
		{
			return -1;
		}

		var r1 = Math.Abs(LoopCells.Count - comparer.LoopCells.Count);
		if (r1 != 0)
		{
			return r1;
		}

		return Math.Abs(Code - comparer.Code);
	}
}
