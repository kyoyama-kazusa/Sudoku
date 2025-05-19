namespace Sudoku.Analytics;

/// <summary>
/// Provides a list of methods that can be used for construct chaining method invocations for configuration,
/// applying to a <see cref="StepGatherer"/> instance.
/// </summary>
/// <seealso cref="StepGatherer"/>
public static class StepGathererFactory
{
	/// <summary>
	/// Provides extension members on <see cref="Analyzer"/>.
	/// </summary>
	extension(Analyzer instance)
	{
		/// <summary>
		/// Sets the property <see cref="Analyzer.RandomizedChoosing"/> with the target value.
		/// </summary>
		/// <param name="randomizedChoosing">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithRandomizedChoosing(bool randomizedChoosing)
		{
			instance.RandomizedChoosing = randomizedChoosing;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="Analyzer.IsFullApplying"/> with the target value.
		/// </summary>
		/// <param name="applyAll">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithApplyAll(bool applyAll)
		{
			instance.IsFullApplying = applyAll;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="StepGatherer.StepSearchers"/> with the target value.
		/// </summary>
		/// <param name="stepSearchers">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithStepSearchers(params StepSearcher[] stepSearchers)
		{
			instance.StepSearchers = stepSearchers;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="StepGatherer.Options"/> with the target value.
		/// </summary>
		/// <param name="options">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer WithUserDefinedOptions(StepGathererOptions options)
		{
			instance.Options = options;
			return instance;
		}

		/// <summary>
		/// Try to set property <see cref="StepGatherer.StepSearchers"/> with the specified value.
		/// </summary>
		/// <param name="stepSearchers">The custom collection of <see cref="StepSearcher"/>s.</param>
		/// <param name="level">Indicates the difficulty level preserved.</param>
		/// <returns>The result.</returns>
		/// <seealso cref="StepGatherer.StepSearchers"/>
		/// <seealso cref="StepSearcher"/>
		public Analyzer WithStepSearchers(StepSearcher[] stepSearchers, DifficultyLevel level = DifficultyLevel.Unknown)
			=> instance.WithStepSearchers(
				level == DifficultyLevel.Unknown
					? stepSearchers
					:
					from stepSearcher in stepSearchers
					where stepSearcher.Metadata.DifficultyLevelRange.Any(l => l <= level)
					select stepSearcher
			);

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <typeparam name="TStepSearcher">The type of step searcher.</typeparam>
		/// <param name="setter">The value to be added.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Analyzer ApplySetter<TStepSearcher>(Action<TStepSearcher> setter) where TStepSearcher : StepSearcher
		{
			instance.Setters.Add(
				s =>
				{
					if (s is TStepSearcher target)
					{
						setter(target);
					}
				}
			);
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <param name="setters">The value to be added.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer ApplySetter(Action<StepSearcher> setters)
		{
			instance.Setters.Add(setters);
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <param name="setters">A list of values to be added.</param>
		/// <returns>The value same as <see cref="Analyzer"/>.</returns>
		public Analyzer ApplySetters(params ReadOnlySpan<Action<StepSearcher>> setters)
		{
			foreach (var element in setters)
			{
				instance.Setters.Add(element);
			}
			return instance;
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="Collector"/>.
	/// </summary>
	extension(Collector instance)
	{
		/// <summary>
		/// Sets the property <see cref="Collector.MaxStepsCollected"/> with the target value.
		/// </summary>
		/// <param name="count">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithMaxSteps(int count)
		{
			instance.MaxStepsCollected = count;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="Collector.DifficultyLevelMode"/> with the target value.
		/// </summary>
		/// <param name="collectingMode">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithSameLevelConfiguration(CollectorDifficultyLevelMode collectingMode)
		{
			instance.DifficultyLevelMode = collectingMode;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="StepGatherer.StepSearchers"/> with the target value.
		/// </summary>
		/// <param name="stepSearchers">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithStepSearchers(params StepSearcher[] stepSearchers)
		{
			instance.StepSearchers = stepSearchers;
			return instance;
		}

		/// <summary>
		/// Sets the property <see cref="StepGatherer.Options"/> with the target value.
		/// </summary>
		/// <param name="options">The value to be set or updated.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector WithUserDefinedOptions(StepGathererOptions options)
		{
			instance.Options = options;
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <typeparam name="TStepSearcher">The type of step searcher.</typeparam>
		/// <param name="setter">The value to be added.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Collector ApplySetter<TStepSearcher>(Action<TStepSearcher> setter) where TStepSearcher : StepSearcher
		{
			instance.Setters.Add(
				s =>
				{
					if (s is TStepSearcher target)
					{
						setter(target);
					}
				}
			);
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <param name="setters">The value to be added.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector ApplySetter(Action<StepSearcher> setters)
		{
			instance.Setters.Add(setters);
			return instance;
		}

		/// <summary>
		/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
		/// </summary>
		/// <param name="setters">A list of values to be added.</param>
		/// <returns>The value same as <see cref="Collector"/>.</returns>
		public Collector ApplySetters(params ReadOnlySpan<Action<StepSearcher>> setters)
		{
			foreach (var element in setters)
			{
				instance.Setters.Add(element);
			}
			return instance;
		}
	}
}
