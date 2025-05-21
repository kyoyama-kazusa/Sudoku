namespace Sudoku.Analytics.Steps.SingleDigitPatterns;

/// <summary>
/// Provides with a step that is a <b>Single-Digit Pattern</b> or <b>Grouped Single-Digit Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc/></param>
/// <param name="baseHouse">Indicates the base house used.</param>
/// <param name="targetHouse">Indicates the target house used.</param>
/// <param name="isGrouped">Indicates whether the links is grouped.</param>
public sealed partial class TwoStrongLinksStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	[Property] House baseHouse,
	[Property] House targetHouse,
	[Property] bool isGrouped
) : SingleDigitPatternStep(conclusions, views, options, digit)
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> Code switch
		{
			Technique.TurbotFish => 42,
			Technique.Skyscraper => 40,
			Technique.TwoStringKite => 41,
			Technique.GroupedTurbotFish => 44,
			Technique.GroupedSkyscraper => 42,
			Technique.GroupedTwoStringKite => 43
		};

	/// <inheritdoc/>
	public override Technique Code
		=> (BaseHouse / 9, TargetHouse / 9) switch
		{
			(0, _) or (_, 0) => IsGrouped ? Technique.GroupedTurbotFish : Technique.TurbotFish,
			(1, 1) or (2, 2) => IsGrouped ? Technique.GroupedSkyscraper : Technique.Skyscraper,
			(1, 2) or (2, 1) => IsGrouped ? Technique.GroupedTwoStringKite : Technique.TwoStringKite
		};

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, BaseHouseStr, TargetHouseStr]), new(SR.ChineseLanguage, [DigitStr, BaseHouseStr, TargetHouseStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));

	private string BaseHouseStr => Options.Converter.HouseConverter(1 << BaseHouse);

	private string TargetHouseStr => Options.Converter.HouseConverter(1 << TargetHouse);
}
