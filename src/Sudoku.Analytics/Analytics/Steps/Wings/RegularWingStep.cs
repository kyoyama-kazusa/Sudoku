namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>Regular Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pivot"><inheritdoc cref="Pivot" path="/summary"/></param>
/// <param name="pivotCandidatesCount"><inheritdoc cref="PivotCandidatesCount" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="petals"><inheritdoc cref="Petals" path="/summary"/></param>
public sealed class RegularWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell pivot,
	int pivotCandidatesCount,
	Mask digitsMask,
	in CellMap petals
) : WingStep(conclusions, views, options), ISizeTrait
{
	/// <summary>
	/// Indicates whether the pattern is incomplete.
	/// </summary>
	public bool IsIncomplete => Size == PivotCandidatesCount + 1;

	/// <inheritdoc/>
	public override int BaseDifficulty => 42;

	/// <inheritdoc/>
	/// <remarks>
	/// The size indicates the number of candidates that the pivot cell holds. All names are:
	/// <list type="table">
	/// <item>
	/// <term>3</term>
	/// <description>XY-Wing or XYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>4</term>
	/// <description>WXYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>5</term>
	/// <description>VWXYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>6</term>
	/// <description>UVWXYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>7</term>
	/// <description>TUVWXYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>8</term>
	/// <description>STUVWXYZ-Wing</description>
	/// </item>
	/// <item>
	/// <term>9</term>
	/// <description>RSTUVWXYZ-Wing</description>
	/// </item>
	/// </list>
	/// </remarks>
	public int Size => BitOperations.PopCount(DigitsMask);

	/// <inheritdoc/>
	public override Technique Code
		=> TechniqueNaming.RegularWing.MakeRegularWingTechniqueCode(TechniqueNaming.RegularWing.GetRegularWingEnglishName(Size, IsIncomplete));

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the cell that blossomed its petals.
	/// </summary>
	public Cell Pivot { get; } = pivot;

	/// <summary>
	/// Indicates the number of digits in the pivot cell.
	/// </summary>
	public int PivotCandidatesCount { get; } = pivotCandidatesCount;

	/// <summary>
	/// Indicates a mask that contains all digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the petals used.
	/// </summary>
	public CellMap Petals { get; } = petals;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, PivotCellStr, CellsStr]), new(SR.ChineseLanguage, [DigitsStr, PivotCellStr, CellsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_RegularWingSizeFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! switch { 3 => 0, 4 => 2, 5 => 4, 6 => 7, 7 => 10, 8 => 13, 9 => 16, _ => 20 }
			),
			Factor.Create(
				"Factor_RegularWingIncompletenessFactor",
				[nameof(Code), nameof(IsIncomplete)],
				GetType(),
				static args => ((Technique)args![0]!, (bool)args![1]!) switch
				{
					(Technique.XyWing, _) => 0,
					(Technique.XyzWing, _) => 2,
					(_, true) => 1,
					_ => 0
				}
			)
		];

	private string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private string PivotCellStr => Options.Converter.CellConverter(in Pivot.AsCellMap());

	private string CellsStr => Options.Converter.CellConverter(Petals);
}
