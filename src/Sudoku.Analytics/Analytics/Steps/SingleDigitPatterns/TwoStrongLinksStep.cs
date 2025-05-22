namespace Sudoku.Analytics.Steps.SingleDigitPatterns;

/// <summary>
/// Provides with a step that is a <b>Single-Digit Pattern</b> or <b>Grouped Single-Digit Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleDigitPatternStep.Digit" path="/summary"/></param>
/// <param name="baseHouse"><inheritdoc cref="BaseHouse" path="/summary"/></param>
/// <param name="targetHouse"><inheritdoc cref="TargetHouse" path="/summary"/></param>
/// <param name="isGrouped"><inheritdoc cref="IsGrouped" path="/summary"/></param>
public sealed class TwoStrongLinksStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	House baseHouse,
	House targetHouse,
	bool isGrouped
) : SingleDigitPatternStep(conclusions, views, options, digit)
{
	/// <summary>
	/// Indicates whether the links is grouped.
	/// </summary>
	public bool IsGrouped { get; } = isGrouped;

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

	/// <summary>
	/// Indicates the base house used.
	/// </summary>
	public House BaseHouse { get; } = baseHouse;

	/// <summary>
	/// Indicates the target house used.
	/// </summary>
	public House TargetHouse { get; } = targetHouse;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, BaseHouseStr, TargetHouseStr]), new(SR.ChineseLanguage, [DigitStr, BaseHouseStr, TargetHouseStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));

	private string BaseHouseStr => Options.Converter.HouseConverter(1 << BaseHouse);

	private string TargetHouseStr => Options.Converter.HouseConverter(1 << TargetHouse);
}
