namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Broken Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="Loop" path="/summary"/></param>
/// <param name="guardians"><inheritdoc cref="Guardians" path="/summary"/></param>
public closed class BrokenLoopStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	ReadOnlyMemory<Candidate> loop,
	in CandidateMap guardians
) : InvalidityStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 52;

	/// <summary>
	/// Indicates the type of the pattern.
	/// </summary>
	public abstract int Type { get; }

	/// <summary>
	/// Indicates the length of loop.
	/// </summary>
	public int LoopLength => Loop.Length;

	/// <inheritdoc/>
	public sealed override Technique Code => Technique.BrokenLoopType1 - 1 + Type;

	/// <summary>
	/// Indicates the loop.
	/// </summary>
	public ReadOnlyMemory<Candidate> Loop { get; } = loop;

	/// <summary>
	/// Indicates the guardians.
	/// </summary>
	public CandidateMap Guardians { get; } = guardians;

	/// <inheritdoc/>
	public override Mask DigitsUsed
	{
		get
		{
			var result = (Mask)0;
			foreach (var candidate in Loop)
			{
				result |= (Mask)(1 << candidate % 9);
			}
			foreach (var candidate in Guardians)
			{
				result |= (Mask)(1 << candidate % 9);
			}
			return result;
		}
	}

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BrokenLoopLengthFactor",
				[nameof(LoopLength)],
				GetType(),
				static args => DifficultyCalculator.Chaining.GetLengthDifficulty((int)args[0]!)
			)
		];

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [LoopStr, GuardiansStr]), new(SR.ChineseLanguage, [LoopStr, GuardiansStr])];

	private protected string LoopStr
	{
		get
		{
			var result = new List<string>();
			foreach (var candidate in Loop)
			{
				result.Add(Candidate.ToCandidateString(candidate, Options.Converter));
			}
			return string.Join(" -> ", result);
		}
	}

	private protected string GuardiansStr => Options.Converter.CandidateConverter(Guardians);
}
