namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>Regular Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="pivot">Indicates the cell that blossomed its petals.</param>
/// <param name="pivotCandidatesCount">Indicates the number of digits in the pivot cell.</param>
/// <param name="digitsMask">Indicates a mask that contains all digits used.</param>
/// <param name="petals">Indicates the petals used.</param>
public sealed partial class RegularWingStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Cell pivot,
	[Property] int pivotCandidatesCount,
	[Property] Mask digitsMask,
	[Property] in CellMap petals
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
