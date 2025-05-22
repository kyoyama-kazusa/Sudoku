namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Hidden Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="enableAndIsLastDigit"><inheritdoc cref="EnableAndIsLastDigit" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="Lasting" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="excluderInfo"><inheritdoc cref="ExcluderInfo" path="/summary"/></param>
public class HiddenSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	House house,
	bool enableAndIsLastDigit,
	Cell lasting,
	SingleSubtype subtype,
	ExcluderInfo? excluderInfo
) : SingleStep(conclusions, views, options, cell, digit, subtype), ILastingTrait
{
	/// <summary>
	/// Indicates whether currently options enable "Last Digit" technique, and the current instance is a real Last Digit.
	/// If the technique is not a Last Digit, the value must be <see langword="false"/>.
	/// </summary>
	public bool EnableAndIsLastDigit { get; } = enableAndIsLastDigit;

	/// <inheritdoc/>
	public sealed override int BaseDifficulty
		=> EnableAndIsLastDigit ? 11 : House < 9 ? Options.IsDirectMode ? 12 : 19 : Options.IsDirectMode ? 15 : 23;

	/// <inheritdoc/>
	public sealed override Technique Code
		=> (Options.IsDirectMode, EnableAndIsLastDigit) switch
		{
			(_, true) => Technique.LastDigit,
			(true, false) => (Technique)((int)Technique.CrosshatchingBlock + (int)House.HouseType),
			_ => (Technique)((int)Technique.HiddenSingleBlock + (int)House.HouseType)
		};

	/// <summary>
	/// The house to be displayed.
	/// </summary>
	public House House { get; } = house;

	/// <inheritdoc cref="ILastingTrait.Lasting"/>
	public Cell Lasting { get; } = lasting;

	/// <inheritdoc/>
	public sealed override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, EnableAndIsLastDigit ? [DigitStr] : [HouseStr]),
			new(SR.ChineseLanguage, EnableAndIsLastDigit ? [DigitStr] : [HouseStr])
		];

	/// <summary>
	/// Indicates the excluder information. The value can be <see langword="null"/> if the target step is a Last Digit.
	/// </summary>
	public ExcluderInfo? ExcluderInfo { get; } = excluderInfo;

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));

	private string HouseStr => Options.Converter.HouseConverter(1 << House);


	/// <inheritdoc/>
	public override string GetName(IFormatProvider? formatProvider)
	{
		var baseName = base.GetName(formatProvider);
		if (!Options.IsDirectMode || Code is Technique.LastDigit or Technique.CrosshatchingBlock)
		{
			return baseName;
		}

		var culture = Options.CurrentCulture;
		var lastDigitsCountString = string.Format(
			SR.Get("DirectSingleLastSuffix", culture),
			null, // Placeholder for the house type - hidden singles won't check on this case.
			TechniqueNaming.GetDigitCharacter(culture, Lasting - 1)
		);
		if (SR.IsChinese(culture))
		{
			var centerDot = SR.Get("_Token_CenterDot", culture);
			return $"{baseName}{centerDot}{lastDigitsCountString}";
		}
		return $"{baseName} ({lastDigitsCountString})";
	}

	/// <inheritdoc/>
	protected internal override int NameCompareTo(Step other, IFormatProvider? formatProvider)
	{
		if (Code.CompareTo(other.Code) is var codeComparisonResult and not 0)
		{
			return codeComparisonResult;
		}

		var culture = GetCulture(formatProvider);
		if (!SR.IsChinese(culture))
		{
			return base.NameCompareTo(other, formatProvider);
		}

		var leftName = GetName(formatProvider);
		var rightName = other.GetName(formatProvider);
		var leftMatch = TechniqueNaming.ChineseDigitsPattern.Match(leftName);
		var rightMatch = TechniqueNaming.ChineseDigitsPattern.Match(rightName);
		return (leftMatch, rightMatch) switch
		{
			// Invalid case. Use this check to ignore 'not null' checking in the following case branches.
			not (not null, not null) => throw new InvalidOperationException(),

			// Valid case. Now we should check the first matched character to compare its corresponding digit.
			({ Success: true, Value: [var l] }, { Success: true, Value: [var r] })
			when (TechniqueNaming.GetChineseDigit(l), TechniqueNaming.GetChineseDigit(r)) is var (leftDigit, rightDigit)
				=> leftDigit.CompareTo(rightDigit),

			// The first value is less than the second because it has no lasting value to be matched,
			// meaning it can only be hidden single in block, full house or last digit.
			({ Success: false }, { Success: true }) => -1,

			// Same case as above.
			({ Success: true }, { Success: false }) => 1,

			// Same technique but neither matched a lasting value.
			_ => 0
		};
	}
}
