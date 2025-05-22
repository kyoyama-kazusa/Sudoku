namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Direct Intersection</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="intersectionCells"><inheritdoc cref="IntersectionCells" path="/summary"/></param>
/// <param name="intersectionHouse"><inheritdoc cref="IntersectionHouse" path="/summary"/></param>
/// <param name="interim"><inheritdoc cref="Interim" path="/summary"/></param>
/// <param name="interimDigit"><inheritdoc cref="InterimDigit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="basedOn"><inheritdoc cref="ComplexSingleStep.BasedOn" path="/summary"/></param>
/// <param name="isPointing"><inheritdoc cref="IsPointing" path="/summary"/></param>
public sealed class DirectIntersectionStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	in CellMap intersectionCells,
	House intersectionHouse,
	in CellMap interim,
	Digit interimDigit,
	SingleSubtype subtype,
	Technique basedOn,
	bool isPointing
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
	/// <summary>
	/// Indicates whether the current locked candidates pattern used is pointing.
	/// </summary>
	public bool IsPointing { get; } = isPointing;

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

	/// <summary>
	/// Indicates the intersection cells.
	/// </summary>
	public CellMap IntersectionCells { get; } = intersectionCells;

	/// <summary>
	/// Indicates the intersection house.
	/// </summary>
	public House IntersectionHouse { get; } = intersectionHouse;

	/// <summary>
	/// Indicates the interim cells.
	/// </summary>
	public CellMap Interim { get; } = interim;

	/// <summary>
	/// Indicates the interim digit.
	/// </summary>
	public Digit InterimDigit { get; } = interimDigit;

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
