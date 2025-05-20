namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Direct Subset</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subsetCells">Indicates the subset cells used.</param>
/// <param name="subsetDigitsMask">Indicates the digits that the subset used.</param>
/// <param name="subsetHouse">Indicates the subset house.</param>
/// <param name="interim">Indicates the interim cells used.</param>
/// <param name="interimDigitsMask">Indicates the digits produced in interim.</param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="basedOn"><inheritdoc cref="ComplexSingleStep.BasedOn" path="/summary"/></param>
/// <param name="subsetTechnique">Indicates the subset technique used.</param>
public sealed partial class DirectSubsetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	[Property] in CellMap subsetCells,
	[Property] Mask subsetDigitsMask,
	[Property] House subsetHouse,
	[Property] in CellMap interim,
	[Property] Mask interimDigitsMask,
	SingleSubtype subtype,
	Technique basedOn,
	[Property] Technique subsetTechnique
) :
	ComplexSingleStep(conclusions, views, options, cell, digit, subtype, basedOn, [[subsetTechnique]]),
	ISizeTrait,
	ICellListTrait
{
	/// <summary>
	/// Indicates whether the used subset is a naked subset.
	/// </summary>
	public bool IsNaked
		=> SubsetTechnique is Technique.NakedPair or Technique.NakedPairPlus or Technique.LockedPair
		or Technique.NakedTriple or Technique.NakedTriplePlus or Technique.LockedTriple
		or Technique.NakedQuadruple or Technique.NakedQuadruplePlus;

	/// <summary>
	/// <inheritdoc
	///     cref="NakedSubsetStep(StepConclusions, View[], StepGathererOptions, int, in CellMap, short, bool?)"
	///     path="/param[@name='isLocked']"/>
	/// </summary>
	public bool? IsLocked
		=> SubsetTechnique switch
		{
			Technique.NakedPair or Technique.NakedTriple or Technique.NakedQuadruple => null,
			Technique.NakedPairPlus or Technique.NakedTriplePlus or Technique.NakedQuadruplePlus => false,
			Technique.LockedPair or Technique.LockedTriple => true,
			_ => null
		};

	/// <inheritdoc/>
	public int Size => SubsetCells.Count;

	/// <inheritdoc/>
	public override int BaseDifficulty => IsNaked ? 33 : 37;

	/// <inheritdoc/>
	public override Technique Code => BasedOn;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(
				SR.EnglishLanguage,
				[
					CellsStr,
					HouseStr,
					InterimCellStr,
					InterimDigitStr,
					TechniqueNameStr(SR.EnglishLanguage),
					DigitsStr,
					SubsetNameStr(SR.EnglishLanguage)
				]
			),
			new(
				SR.ChineseLanguage,
				[
					CellsStr,
					HouseStr,
					InterimCellStr,
					InterimDigitStr,
					TechniqueNameStr(SR.ChineseLanguage),
					DigitsStr,
					SubsetNameStr(SR.ChineseLanguage)
				]
			)
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_DirectSubsetSizeFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => (int)args![0]! switch { 2 => 0, 3 => 6, 4 => 20 }
			),
			Factor.Create(
				"Factor_DirectSubsetIsLockedFactor",
				[nameof(IsNaked), nameof(IsLocked), nameof(Size)],
				GetType(),
				static args => (bool)args![0]!
					? (bool?)args[1]! switch { true => (int)args[2]! switch { 2 => -10, 3 => -11 }, false => 1, _ => 0 }
					: (bool?)args[1]! switch { true => (int)args[2]! switch { 2 => -12, 3 => -13 }, _ => 0 }
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => SubsetCells.Count;

	private string CellsStr => Options.Converter.CellConverter(SubsetCells);

	private string HouseStr => Options.Converter.HouseConverter(1 << SubsetHouse);

	private string InterimCellStr => Options.Converter.CellConverter(Interim);

	private string InterimDigitStr => Options.Converter.DigitConverter(InterimDigitsMask);

	private string DigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is DirectSubsetStep comparer
		&& SubsetCells == comparer.SubsetCells && SubsetDigitsMask == comparer.SubsetDigitsMask
		&& Interim == comparer.Interim && InterimDigitsMask == comparer.InterimDigitsMask
		&& Subtype == comparer.Subtype && SubsetTechnique == comparer.SubsetTechnique;

	private string TechniqueNameStr(string cultureName) => BasedOn.GetName(new CultureInfo(cultureName));

	private string SubsetNameStr(string cultureName) => SubsetTechnique.GetName(new CultureInfo(cultureName));
}
