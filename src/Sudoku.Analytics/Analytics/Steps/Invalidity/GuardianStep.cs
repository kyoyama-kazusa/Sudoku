namespace Sudoku.Analytics.Steps.Invalidity;

/// <summary>
/// Provides with a step that is a <b>Guardian</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="loopCells"><inheritdoc cref="LoopCells" path="/summary"/></param>
/// <param name="guardians"><inheritdoc cref="Guardians" path="/summary"/></param>
public sealed class GuardianStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	in CellMap loopCells,
	in CellMap guardians
) : InvalidityStep(conclusions, views, options), ICellListTrait, IGuardianTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 55;

	/// <inheritdoc/>
	public override Technique Code => Technique.BrokenWing;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the cells of the loop used.
	/// </summary>
	public CellMap LoopCells { get; } = loopCells;

	/// <summary>
	/// Indicates the guardian cells.
	/// </summary>
	public CellMap Guardians { get; } = guardians;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [CellsStr, GuardianSingularOrPlural(SR.EnglishLanguage), GuardianStr]),
			new(SR.ChineseLanguage, [CellsStr, GuardianSingularOrPlural(SR.ChineseLanguage), GuardianStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_GuardianFactor",
				[nameof(ICellListTrait.CellSize), nameof(IGuardianTrait.GuardianCellsCount)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]! + OeisSequences.A004526((int)args![1]!))
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => LoopCells.Count;

	/// <inheritdoc/>
	int IGuardianTrait.GuardianCellsCount => Guardians.Count;

	/// <inheritdoc/>
	CellMap IGuardianTrait.GuardianCells => Guardians;

	private string CellsStr => Options.Converter.CellConverter(LoopCells);

	private string GuardianStr => Options.Converter.CellConverter(Guardians);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is GuardianStep comparer && (Digit, LoopCells, Guardians) == (comparer.Digit, comparer.LoopCells, comparer.Guardians);

	/// <inheritdoc/>
	public override int CompareTo(Step? other)
	{
		if (other is not GuardianStep comparer)
		{
			return -1;
		}

		var r1 = Math.Abs(LoopCells.Count - comparer.LoopCells.Count);
		if (r1 != 0)
		{
			return r1;
		}

		return Math.Abs(Guardians.Count - comparer.Guardians.Count);
	}

	private string GuardianSingularOrPlural(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		return SR.Get(Guardians.Count == 1 ? "GuardianSingular" : "GuardianPlural", culture);
	}
}
