namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Strong Link Type</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueLoopStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueLoopStep.Digit2" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="UniqueLoopStep.Loop" path="/summary"/></param>
/// <param name="loopPath"><inheritdoc cref="UniqueLoopStep.LoopPath" path="/summary"/></param>
/// <param name="extraDigitsCellsCount"><inheritdoc cref="ExtraDigitsCellsCount" path="/summary"/></param>
/// <param name="conjugatePairs"><inheritdoc cref="ConjugatePairs" path="/summary"/></param>
public sealed class UniqueLoopConjugatePairsTypeStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap loop,
	Cell[] loopPath,
	int extraDigitsCellsCount,
	Conjugate[] conjugatePairs
) : UniqueLoopStep(conclusions, views, options, digit1, digit2, loop, loopPath)
{
	/// <inheritdoc/>
	public override int Type => 5;

	/// <summary>
	/// Indicates the number of conjugate pairs used.
	/// </summary>
	public int ConjugatePairsCount => ConjugatePairs.Length;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override string EnglishName
	{
		get
		{
			var uniqueLoopName = SR.Get("UniqueNameName", SR.DefaultCulture);
			return $"{uniqueLoopName} + {ExtraDigitsCellsCount}/{ConjugatePairs.Length}SL";
		}
	}

	/// <inheritdoc/>
	public override Technique Code => Technique.UniqueLoopStrongLinkType;

	/// <summary>
	/// Indicates the number cells containing extra digits.
	/// </summary>
	public int ExtraDigitsCellsCount { get; } = extraDigitsCellsCount;

	/// <summary>
	/// Indicates the conjugate pairs.
	/// </summary>
	public Conjugate[] ConjugatePairs { get; } = conjugatePairs;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [Digit1Str, Digit2Str, LoopStr, ConjugatePairsStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [Digit1Str, Digit2Str, LoopStr, ConjugatePairsStr(SR.ChineseLanguage)])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			.. base.Factors,
			Factor.Create(
				"Factor_UniqueLoopConjugatePairsCountFactor",
				[nameof(ConjugatePairsCount)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	private string ConjugatePairsStr(string cultureName)
	{
		var converter = Options.Converter;
		var culture = new CultureInfo(cultureName);
		return string.Join(
			SR.Get("_Token_Comma", culture),
			from cp in ConjugatePairs select cp.ToString(converter)
		);
	}


	/// <inheritdoc/>
	public override string GetName(IFormatProvider? formatProvider)
	{
		var culture = GetCulture(formatProvider);
		var uniqueLoopName = SR.Get("UniqueNameName", culture);
		return $"{uniqueLoopName} + {ExtraDigitsCellsCount}/{ConjugatePairs.Length}SL";
	}
}
