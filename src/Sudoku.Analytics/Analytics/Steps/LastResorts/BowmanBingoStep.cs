namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bowman's Bingo</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="contradictionLinks"><inheritdoc cref="ContradictionLinks" path="/summary"/></param>
public sealed class BowmanBingoStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Conclusion[] contradictionLinks
) :
	LastResortStep(conclusions, views, options),
	ISizeTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 80;

	/// <inheritdoc/>
	public override Technique Code => Technique.BowmanBingo;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Mask.Create(from link in ContradictionLinks select link.Digit);

	/// <summary>
	/// Indicates the list of contradiction links.
	/// </summary>
	public Conclusion[] ContradictionLinks { get; } = contradictionLinks;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ContradictionSeriesStr]), new(SR.ChineseLanguage, [ContradictionSeriesStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BowmanBingoLengthFactor",
				[nameof(ISizeTrait.Size)],
				GetType(),
				static args => ChainingLength.GetLengthDifficulty((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int ISizeTrait.Size => ContradictionLinks.Length;

	private string ContradictionSeriesStr
	{
		get
		{
			var snippets = new List<string>();
			foreach (var conclusion in ContradictionLinks)
			{
				snippets.Add(Options.Converter.ConclusionConverter([conclusion]));
			}
			return string.Join(" -> ", snippets);
		}
	}
}
