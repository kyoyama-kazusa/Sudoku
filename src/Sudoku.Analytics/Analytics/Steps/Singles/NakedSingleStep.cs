namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Naked Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="Lasting" path="/summary"/></param>
/// <param name="lastingHouseType"><inheritdoc cref="LastingHouseType" path="/summary"/></param>
public sealed class NakedSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype,
	int lasting,
	HouseType lastingHouseType
) :
	SingleStep(conclusions, views, options, cell, digit, subtype),
	ILastingTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => Options.IsDirectMode ? 23 : 10;

	/// <inheritdoc cref="ILastingTrait.Lasting"/>
	public int Lasting { get; } = lasting;

	/// <inheritdoc/>
	public override Technique Code => Technique.NakedSingle;

	/// <summary>
	/// Indicates the lasting house type.
	/// </summary>
	public HouseType LastingHouseType { get; } = lastingHouseType;


	/// <inheritdoc/>
	public override string GetName(IFormatProvider? formatProvider)
	{
		var baseName = base.GetName(formatProvider);
		if (!Options.IsDirectMode)
		{
			return baseName;
		}

		var culture = Options.CurrentCulture;
		var lastDigitsCountString = string.Format(
			SR.Get("DirectSingleLastSuffix", culture),
			SR.Get($"{LastingHouseType}Name", culture),
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

		var a = LastingHouseType;
		var b = ((NakedSingleStep)other).LastingHouseType;
		if (a.CompareTo(b) is var lastingHouseTypeComparisonResult and not 0)
		{
			return lastingHouseTypeComparisonResult;
		}

		var culture = GetCulture(formatProvider);
		if (!SR.IsChinese(culture))
		{
			return base.NameCompareTo(other, formatProvider);
		}

		var leftName = GetName(formatProvider);
		var rightName = other.GetName(formatProvider);
		var leftDigit = TechniqueNaming.GetChineseDigit(TechniqueNaming.ChineseDigitsPattern.Match(leftName).Value[0]);
		var rightDigit = TechniqueNaming.GetChineseDigit(TechniqueNaming.ChineseDigitsPattern.Match(rightName).Value[0]);
		return leftDigit.CompareTo(rightDigit);
	}
}
