namespace Sudoku.Behaviors.Bottlenecks;

/// <summary>
/// Provides with extension methods on <see cref="AnalysisResult"/>.
/// </summary>
/// <seealso cref="AnalysisResult"/>
public static class AnalysisResultExtensions
{
	/// <summary>
	/// Try to get bottleneck steps under the specified rules.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="filters">The bottleneck filters.</param>
	/// <returns>A list of bottleneck steps.</returns>
	/// <exception cref="NotSupportedException">
	/// Throws when the filter contains invalid configuration,
	/// like <see cref="BottleneckType.SingleStepOnly"/> in full-marking mode.
	/// </exception>
	/// <exception cref="InvalidOperationException">Throws when the puzzle is not fully solved.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when argument <paramref name="filters"/> contains one filter holding an undefined <see cref="BottleneckType"/> flag.
	/// </exception>
	public static ReadOnlySpan<Step> GetBottlenecks(this AnalysisResult @this, params ReadOnlySpan<BottleneckFilter> filters)
	{
		if (!@this.IsSolved)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("BottlenecksShouldBeAvailableInSolvedPuzzle"));
		}

		if (@this.StepsSpan is not { Length: not 0 } steps)
		{
			return [];
		}

		var pencilmarkMode = steps.Aggregate(
			PencilmarkVisibility.None,
			static (interim, next) => interim | next switch
			{
				FullPencilmarkingStep => PencilmarkVisibility.FullMarking,
				PartialPencilmarkingStep => PencilmarkVisibility.PartialMarking,
				DirectStep => PencilmarkVisibility.Direct,
				_ => PencilmarkVisibility.None
			}
		);
		var filterMode = pencilmarkMode.HasFlag(PencilmarkVisibility.FullMarking)
			? PencilmarkVisibility.FullMarking
			: pencilmarkMode.HasFlag(PencilmarkVisibility.PartialMarking)
				? PencilmarkVisibility.PartialMarking
				: PencilmarkVisibility.Direct;
		return (filters.FirstRefOrNullRef((ref readonly f) => f.Visibility == filterMode).Type, filterMode) switch
		{
			(BottleneckType.SingleStepOnly, PencilmarkVisibility.Direct or PencilmarkVisibility.PartialMarking) => singleStepOnly(),
			(BottleneckType.SingleStepSameLevelOnly, PencilmarkVisibility.PartialMarking) => singleStepSameLevelOnly(),
			(BottleneckType.EliminationGroup, PencilmarkVisibility.FullMarking) => eliminationGroup(steps),
			(BottleneckType.SequentialInversion, not PencilmarkVisibility.Direct) => sequentialInversion(steps),
			(BottleneckType.HardestRating, _) => hardestRating(steps, steps.MaxBy(static s => s.Difficulty)!),
			(BottleneckType.HardestLevel, not PencilmarkVisibility.Direct) => hardestLevel(steps, steps.MaxBy(static s => (int)s.DifficultyLevel)!),
			_ => throw new ArgumentOutOfRangeException(nameof(filters))
		};


		ReadOnlySpan<Step> singleStepOnly()
		{
			var collector = GridPartialMarkingExtensions.Collector;
			var result = new List<Step>();
			foreach (var (g, s) in StepMarshal.Combine(@this.GridsSpan, @this.StepsSpan))
			{
				if ((
					from step in collector.Collect(g)
					select (SingleStep)step into step
					select step.Cell * 9 + step.Digit
				).AsCandidateMap().Count == 1)
				{
					result.Add(s);
				}
			}
			return result.AsSpan();
		}

		ReadOnlySpan<Step> singleStepSameLevelOnly()
		{
			var collector = GridPartialMarkingExtensions.Collector;
			var result = new List<Step>();
			foreach (var (g, s) in StepMarshal.Combine(@this.GridsSpan, @this.StepsSpan))
			{
				var currentStepPencilmarkVisibility = s.PencilmarkType;
				if ((
					from step in collector.Collect(g)
					select ((SingleStep)step) into step
					where step.PencilmarkType <= currentStepPencilmarkVisibility
					select step.Cell * 9 + step.Digit
				).AsCandidateMap().Count == 1)
				{
					result.Add(s);
				}
			}
			return result.AsSpan();
		}

		static ReadOnlySpan<Step> eliminationGroup(ReadOnlySpan<Step> steps)
		{
			var result = new List<Step>();
			for (var i = 0; i < steps.Length - 1; i++)
			{
				if (steps[i].IsAssignment is false)
				{
					for (var j = i + 1; j < steps.Length; j++)
					{
						if (steps[j].IsAssignment is not false)
						{
							// Okay. Now we have a group of steps that only produce eliminations.
							// Set the outer loop pointer to skip elimination steps.
							result.Add(steps[i = j]);
							break;
						}
					}
				}
			}
			return result.AsSpan();
		}

		static ReadOnlySpan<Step> sequentialInversion(ReadOnlySpan<Step> steps)
		{
			var result = new List<Step>();
			for (var i = 0; i < steps.Length - 1; i++)
			{
				var (previous, next) = (steps[i], steps[i + 1]);
				if (previous.DifficultyLevel > next.DifficultyLevel && next.DifficultyLevel != DifficultyLevel.Unknown)
				{
					result.Add(previous);
				}
			}
			return result.AsSpan();
		}

		static ReadOnlySpan<Step> hardestRating(ReadOnlySpan<Step> steps, Step maxStep)
		{
			var result = new List<Step>();
			foreach (var element in steps)
			{
				if (element.Code == maxStep.Code)
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}

		static ReadOnlySpan<Step> hardestLevel(ReadOnlySpan<Step> steps, Step maxStep)
		{
			var result = new List<Step>();
			foreach (var element in steps)
			{
				if (element.DifficultyLevel == maxStep.DifficultyLevel)
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}
	}
}
