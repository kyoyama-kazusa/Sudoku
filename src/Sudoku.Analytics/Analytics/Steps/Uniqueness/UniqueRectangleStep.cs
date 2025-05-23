namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc cref="Step.Code" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="IsAvoidable" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="AbsoluteOffset" path="/summary"/></param>
public abstract class UniqueRectangleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	int absoluteOffset
) :
	UnconditionalDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => true;

	/// <summary>
	/// Indicates whether the current rectangle is an avoidable rectangle.
	/// If <see langword="true"/>, an avoidable rectangle; otherwise, a unique rectangle.
	/// </summary>
	public bool IsAvoidable { get; } = isAvoidable;

	/// <inheritdoc/>
	public virtual int Type => 7;

	/// <inheritdoc/>
	public override int BaseDifficulty => 45;

	/// <summary>
	/// <para>Indicates the absolute offset.</para>
	/// <para>
	/// The value is an <see cref="int"/> value, as an index, in order to distinct with all unique rectangle patterns.
	/// The greater the value is, the later the corresponding pattern will be processed.
	/// The value must be between 0 and 485, because the total number of possible patterns is 486.
	/// </para>
	/// </summary>
	public int AbsoluteOffset { get; } = absoluteOffset;

	/// <summary>
	/// The generated property declaration for parameter <c>code</c>.
	/// </summary>
	public sealed override Technique Code => code;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit1 | 1 << Digit2);

	/// <summary>
	/// Indicates the first digit used.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used. This value is always greater than <see cref="Digit1"/>.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <summary>
	/// Indicates the cells used in this pattern.
	/// </summary>
	public CellMap Cells { get; } = cells;

	private protected string DigitsStr => Options.Converter.DigitConverter((Mask)(1 << Digit1 | 1 << Digit2));

	private protected string D1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private protected string D2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));

	private protected string CellsStr => Options.Converter.CellConverter(Cells);


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] Step? other)
	{
		if (other is not UniqueRectangleStep comparer)
		{
			return false;
		}

		if ((Code, AbsoluteOffset, Digit1, Digit2) != (comparer.Code, comparer.AbsoluteOffset, comparer.Digit1, comparer.Digit2))
		{
			return false;
		}

		var l = (from conclusion in Conclusions.ToArray() select conclusion.Candidate).AsCandidateMap();
		var r = (from conclusion in comparer.Conclusions.ToArray() select conclusion.Candidate).AsCandidateMap();
		return l == r;
	}

	/// <inheritdoc/>
	public sealed override int CompareTo(Step? other)
		=> other is UniqueRectangleStep comparer
			? Math.Sign(Code - comparer.Code) switch { 0 => Math.Sign(AbsoluteOffset - comparer.AbsoluteOffset), var result => result }
			: 1;
}
