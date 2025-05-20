namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Direct Intersection</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="intersectionCells">Indicates the intersection cells.</param>
/// <param name="intersectionHouse">Indicates the intersection house.</param>
/// <param name="interim">Indicates the interim cells.</param>
/// <param name="interimDigit">Indicates the interim digit.</param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="basedOn"><inheritdoc cref="ComplexSingleStep.BasedOn" path="/summary"/></param>
/// <param name="isPointing">Indicates whether the current locked candidates pattern used is pointing.</param>
public sealed partial class DirectIntersectionStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	[Property] in CellMap intersectionCells,
	[Property] House intersectionHouse,
	[Property] in CellMap interim,
	[Property] Digit interimDigit,
	SingleSubtype subtype,
	Technique basedOn,
	[Property] bool isPointing
) : ComplexSingleStep(
	conclusions,
	views,
	options,
	cell,
	digit,
	subtype,
	basedOn,
	[[isPointing ? Technique.Pointing : Technique.Claiming]]
)
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> BasedOn switch
		{
			Technique.FullHouse => 10,
			Technique.CrosshatchingBlock => 12,
			Technique.CrosshatchingRow or Technique.CrosshatchingColumn => 15,
			Technique.NakedSingle => 23,
			_ => throw new NotSupportedException(SR.ExceptionMessage("TechiqueIsNotSupported"))
		} + 2;

	/// <inheritdoc/>
	public override Technique Code => BasedOn;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [CellsStr, HouseStr, InterimCellStr, InterimDigitStr, TechniqueNameStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [CellsStr, HouseStr, InterimCellStr, InterimDigitStr, TechniqueNameStr(SR.ChineseLanguage)])
		];

	private string CellsStr => Options.Converter.CellConverter(IntersectionCells);

	private string HouseStr => Options.Converter.HouseConverter(1 << IntersectionHouse);

	private string InterimCellStr => Options.Converter.CellConverter(Interim);

	private string InterimDigitStr => Options.Converter.DigitConverter((Mask)(1 << InterimDigit));


	private string TechniqueNameStr(string cultureName) => BasedOn.GetName(new CultureInfo(cultureName));
}
