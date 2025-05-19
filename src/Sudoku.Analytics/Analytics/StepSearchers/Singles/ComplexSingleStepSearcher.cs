namespace Sudoku.Analytics.StepSearchers.Singles;

/// <summary>
/// Provides with a <b>Complex Single</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Complex Full House</item>
/// <item>Complex Hidden Single</item>
/// <item>Complex Naked Single</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_ComplexSingleStepSearcher",
	Technique.ComplexFullHouse, Technique.ComplexCrosshatchingBlock, Technique.ComplexCrosshatchingRow,
	Technique.ComplexCrosshatchingColumn, Technique.ComplexNakedSingle,
	IsAvailabilityReadOnly = true,
	IsOrderingFixed = true,
	RuntimeFlags = StepSearcherRuntimeFlags.DirectTechniquesOnly | StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class ComplexSingleStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the searcher for intersection.
	/// </summary>
	private readonly DirectIntersectionStepSearcher _searcher_DirectLockedCandidates = new()
	{
		AllowDirectPointing = true,
		AllowDirectClaiming = true
	};

	/// <summary>
	/// Indicates the searcher for subset.
	/// </summary>
	private readonly DirectSubsetStepSearcher _searcher_DirectSubset = new()
	{
		AllowDirectHiddenSubset = true,
		AllowDirectLockedHiddenSubset = true,
		AllowDirectLockedSubset = true,
		AllowDirectNakedSubset = true
	};

	/// <summary>
	/// Indicates the searcher for locked candidates.
	/// </summary>
	private readonly LockedCandidatesStepSearcher _searcher_LockedCandidates = new();

	/// <summary>
	/// Indicates the searcher for locked subsets.
	/// </summary>
	private readonly LockedSubsetStepSearcher _searcher_LockedSubset = new();

	/// <summary>
	/// Indicates the searcher for normal subsets.
	/// </summary>
	private readonly NormalSubsetStepSearcher _searcher_Subset = new();


	/// <summary>
	/// Indicates the maximum size of naked subsets. The maximum value is 4.
	/// </summary>
	[SettingItemName(SettingItemNames.NakedSubsetMaxSizeInComplexSingle)]
	public int NakedSubsetMaxSize { get; set; } = 4;

	/// <summary>
	/// Indicates the maximum size of hidden subsets. The maximum value is 4.
	/// </summary>
	[SettingItemName(SettingItemNames.HiddenSubsetMaxSizeInComplexSingle)]
	public int HiddenSubsetMaxSize { get; set; } = 4;

	/// <summary>
	/// Indicates the maximum number of <see cref="Step"/>s can be applied in one interim step. The maximum value is 4.
	/// </summary>
	public int MaxApplyingStepsInOneInterimStep { get; set; } = 1;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Recursively searching for all possible steps.
		var accumulator = new SortedSet<NormalComplexSingleStep>();
		dfs(ref context, accumulator, in context.Grid, [], []);

		// Remove steps that don't satisfy the size limit.
		var stepsFiltered = filterStepsBySize(accumulator);

		// Sort instances if worth.
		// We don't remove duplicate items because the searcher may not produce same steps,
		// and the corresponding step type doesn't override method 'Equals'.
		if (context.OnlyFindOne && stepsFiltered.Count != 0)
		{
			return stepsFiltered.Min;
		}
		if (!context.OnlyFindOne && stepsFiltered.Count != 0)
		{
			context.Accumulator.AddRange(stepsFiltered);
		}
		return null;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void pushStep(out Grid playground, in Grid baseGrid, Step[] indirectStepGroup, LinkedList<Step[]> interimSteps)
		{
			interimSteps.AddLast(indirectStepGroup);
			playground = baseGrid;
			foreach (var indirectStep in indirectStepGroup)
			{
				playground.Apply(indirectStep);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void popStep(ref Grid playground, in Grid baseGrid, LinkedList<Step[]> interimSteps)
		{
			interimSteps.RemoveLast();
			playground = baseGrid;
		}

		void dfs(
			ref StepAnalysisContext context,
			SortedSet<NormalComplexSingleStep> accumulator,
			in Grid grid,
			LinkedList<Step[]> interimSteps,
			List<Step> previousIndirectFoundSteps
		)
		{
			// Collect all steps by using indirect techniques.
			var indirectFoundSteps = new List<Step>();
			var tempContext = new StepAnalysisContext(grid)
			{
				Accumulator = indirectFoundSteps,
				OnlyFindOne = false,
				Options = context.Options
			};
			_searcher_LockedSubset.Collect(ref tempContext);
			_searcher_LockedCandidates.Collect(ref tempContext);
			_searcher_Subset.Collect(ref tempContext);

			// Remove possible steps that has already been recorded into previously found steps.
			// During the searching, we may found a step that may be appeared in the previous grid state,
			// meaning we can found a step in both current state and one of its sub-grid state, which causes a redudant searching
			// if we don't apply them at parent state.
			// We should ignore them in child branches, guaranteeing such steps will be applied in the first meet.
			// However, this limit will produce a potential bug - if two steps are not relative,
			// the searcher will ignore the second one aggressively, but the final assignment will use both,
			// meaning we have removed a step that may be a key one. One example is this:
			// 0002+471630+6+300+500041039+6+500001000+60568+97+51+234500+600900000063052000000+30+6326508000
			//
			// Here the puzzle will use two locked candidates of digit 4 and 9 in r9b9. But both of them can be found
			// in the first step.
			// If we use the first one (i.e. locked candidates of digit 4), the second one (i.e. locked candidates of digit 9)
			// will be ignored and no longer in use.
			// To fix the bug, we should apply both of steps in one same grid state.
			if (indirectFoundSteps.Count != 0)
			{
				foreach (var step in indirectFoundSteps[..])
				{
					if (previousIndirectFoundSteps.Contains(step))
					{
						indirectFoundSteps.Remove(step);
					}
				}
			}
			if (indirectFoundSteps.Count == 0)
			{
				// Nothing can be found.
				return;
			}

			// Record all steps that may not duplicate.
			previousIndirectFoundSteps.AddRange(indirectFoundSteps);

			// Iterate on each step collected, and check whether it can be solved with direct singles.
			for (var applyingSize = 1; applyingSize <= MaxApplyingStepsInOneInterimStep; applyingSize++)
			{
				foreach (var indirectSteps in indirectFoundSteps.AsSpan().GetSubsets(applyingSize))
				{
					// A step will be valid if it has new conclusions that recorded steps don't have.
					var isValid = true;
					foreach (var indirectStep in indirectSteps)
					{
						foreach (var interimStepGroup in interimSteps)
						{
							foreach (var interimStep in interimStepGroup)
							{
								if (interimStep.ConclusionText == indirectStep.ConclusionText)
								{
									isValid = false;
									goto ValidityCheck;
								}
							}
						}
					}
				ValidityCheck:
					if (!isValid)
					{
						continue;
					}

					pushStep(out var playground, grid, indirectSteps, interimSteps);

					// Check whether the puzzle can be solved via a direct single.
					var directStepsFound = new List<Step>();
					var nestedContext = new StepAnalysisContext(playground)
					{
						Accumulator = directStepsFound,
						OnlyFindOne = false,
						Options = context.Options
					};
					_searcher_DirectLockedCandidates.Collect(ref nestedContext);
					_searcher_DirectSubset.Collect(ref nestedContext);

					if (directStepsFound.Count != 0)
					{
						// Good! We have already found a step available! Iterate on each step to create the result value.
						foreach (ComplexSingleStep directStep in directStepsFound)
						{
							var (views, tempConclusions) = (new List<View>(), new List<Conclusion>());
							foreach (var interimStepGroup in interimSteps)
							{
								foreach (var interimStep in interimStepGroup)
								{
									tempConclusions.AddRange(interimStep.Conclusions.Span);
									views.Add(
										[
											.. interimStep.Views![0],
											..
											from conclusion in tempConclusions
											select new CandidateViewNode(ColorIdentifier.Elimination, conclusion.Candidate)
										]
									);
								}
							}
							views.Add(
								[
									.. directStep.Views![0],
									..
									from conclusion in tempConclusions
									select new CandidateViewNode(ColorIdentifier.Elimination, conclusion.Candidate)
								]
							);

							// Add step into accumulator or return step.
							accumulator.Add(
								new(
									directStep.Conclusions.ToArray(),
									[.. views],
									context.Options,
									directStep.Cell,
									directStep.Digit,
									directStep.Subtype,
									directStep.BasedOn,
									[
										.. from @group in interimSteps select from interimStep in @group select interimStep.Code,
										[
											directStep switch
											{
												DirectIntersectionStep { IsPointing: var isPointing } => isPointing switch
												{
													true => Technique.Pointing,
													_ => Technique.Claiming
												},
												DirectSubsetStep { SubsetTechnique: var technique } => technique
											}
										]
									]
								)
							);
							if (context.OnlyFindOne)
							{
								goto PopStep;
							}
						}

						if (!context.OnlyFindOne)
						{
							goto PopStep;
						}
					}

					// If code goes to here, the puzzle won't be solved with the current step.
					// We should continue the searching from the current state.
					// Use this puzzle to check for the next elimination step by recursion.
					dfs(ref context, accumulator, playground, interimSteps, previousIndirectFoundSteps);

				PopStep:
					popStep(ref playground, grid, interimSteps);
				}
			}
		}

		SortedSet<Step> filterStepsBySize(SortedSet<NormalComplexSingleStep> accumulator)
		{
			var stepsSatisfied = new SortedSet<Step>();
			foreach (var step in accumulator)
			{
				var flag = true;
				foreach (var technique in
					from techniquesGroup in step.IndirectTechniques
					from technique in techniquesGroup
					select technique)
				{
					if (technique.Group == TechniqueGroup.Subset)
					{
						var (size, isHidden) = technique switch
						{
							Technique.NakedPair or Technique.NakedPairPlus or Technique.LockedPair => (2, false),
							Technique.HiddenPair or Technique.LockedHiddenPair => (2, true),
							Technique.NakedTriple or Technique.NakedTriplePlus or Technique.LockedTriple => (3, false),
							Technique.HiddenTriple or Technique.LockedHiddenTriple => (3, true),
							Technique.NakedQuadruple or Technique.NakedQuadruplePlus => (4, false),
							Technique.HiddenQuadruple => (4, true)
						};
						if (isHidden && size > HiddenSubsetMaxSize || !isHidden && size > NakedSubsetMaxSize)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					stepsSatisfied.Add(step);
				}
			}

			return stepsSatisfied;
		}
	}
}
