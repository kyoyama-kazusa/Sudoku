namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Template</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Template Set</item>
/// <item>Template Delete</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_TemplateStepSearcher",
	Technique.TemplateDelete, Technique.TemplateSet,
	IsCachingUnsafe = true,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class TemplateStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the technique searcher only checks template deletes.
	/// </summary>
	[SettingItemName(SettingItemNames.TemplateDeleteOnly)]
	public bool TemplateDeleteOnly { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (Solution.IsUndefined)
		{
			return null;
		}

		// Iterate on each digit.
		var distributedMapsByDigit = Solution.ValuesMap;
		for (var digit = 0; digit < 9; digit++)
		{
			if (!TemplateDeleteOnly)
			{
				// Check template sets.
				if ((distributedMapsByDigit[digit] & CandidatesMap[digit]) is not (var templateSetMap and not []))
				{
					continue;
				}

				var templateSetConclusions = from cell in templateSetMap select new Conclusion(Assignment, cell, digit);
				var candidateOffsets = new CandidateViewNode[templateSetConclusions.Length];
				var z = 0;
				foreach (var (_, candidate) in templateSetConclusions)
				{
					candidateOffsets[z++] = new(ColorIdentifier.Normal, candidate);
				}

				var templateSetStep = new TemplateStep(templateSetConclusions.ToArray(), [[.. candidateOffsets]], context.Options, false);
				if (context.OnlyFindOne)
				{
					return templateSetStep;
				}

				context.Accumulator.Add(templateSetStep);
			}

			// Then check template deletes.
			if ((CandidatesMap[digit] & ~distributedMapsByDigit[digit]) is not (var templateDelete and not []))
			{
				continue;
			}

			var templateDeleteStep = new TemplateStep(
				(from cell in templateDelete select new Conclusion(Elimination, cell, digit)).ToArray(),
				null,
				context.Options,
				true
			);
			if (context.OnlyFindOne)
			{
				return templateDeleteStep;
			}

			context.Accumulator.Add(templateDeleteStep);
		}

		return null;
	}
}
