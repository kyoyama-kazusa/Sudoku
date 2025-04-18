namespace Sudoku.Analytics.Steps.LastResorts;

/// <summary>
/// Provides with a step that is a <b>Bowman's Bingo</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="contradictionLinks">Indicates the list of contradiction links.</param>
public sealed partial class BowmanBingoStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Conclusion[] contradictionLinks
) : LastResortStep(conclusions, views, options), ISizeTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 80;

	/// <inheritdoc/>
	public override Technique Code => Technique.BowmanBingo;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Mask.Create(from link in ContradictionLinks select link.Digit);

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
