namespace Sudoku.Analytics;

/// <summary>
/// Provides a list of methods that can be used for construct chaining method invocations for configuration,
/// applying to a <see cref="Collector"/> instance.
/// </summary>
/// <seealso cref="Collector"/>
public static class CollectorFactory
{
	/// <summary>
	/// Sets the property <see cref="Collector.MaxStepsCollected"/> with the target value.
	/// </summary>
	/// <param name="instance">The instance to be set or updated.</param>
	/// <param name="count">The value to be set or updated.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector WithMaxSteps(this Collector instance, int count)
	{
		instance.MaxStepsCollected = count;
		return instance;
	}

	/// <summary>
	/// Sets the property <see cref="Collector.DifficultyLevelMode"/> with the target value.
	/// </summary>
	/// <param name="instance">The instance to be set or updated.</param>
	/// <param name="collectingMode">The value to be set or updated.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector WithSameLevelConfiguration(this Collector instance, CollectorDifficultyLevelMode collectingMode)
	{
		instance.DifficultyLevelMode = collectingMode;
		return instance;
	}

	/// <summary>
	/// Sets the property <see cref="StepGatherer.StepSearchers"/> with the target value.
	/// </summary>
	/// <param name="instance">The instance to be set or updated.</param>
	/// <param name="stepSearchers">The value to be set or updated.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector WithStepSearchers(this Collector instance, params StepSearcher[] stepSearchers)
	{
		instance.StepSearchers = stepSearchers;
		return instance;
	}

	/// <summary>
	/// Sets the property <see cref="StepGatherer.Options"/> with the target value.
	/// </summary>
	/// <param name="instance">The instance to be set or updated.</param>
	/// <param name="options">The value to be set or updated.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector WithUserDefinedOptions(this Collector instance, StepGathererOptions options)
	{
		instance.Options = options;
		return instance;
	}

	/// <summary>
	/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
	/// </summary>
	/// <typeparam name="TStepSearcher">The type of step searcher.</typeparam>
	/// <param name="instance">The instance to be updated.</param>
	/// <param name="setter">The value to be added.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Collector AddStepSearcherSetter<TStepSearcher>(this Collector instance, Action<TStepSearcher> setter)
		where TStepSearcher : StepSearcher
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
	/// <param name="instance">The instance to be updated.</param>
	/// <param name="setters">The value to be added.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector AddStepSearcherSetter(this Collector instance, Action<StepSearcher> setters)
	{
		instance.Setters.Add(setters);
		return instance;
	}

	/// <summary>
	/// Appends an element into the property <see cref="StepGatherer.Setters"/>.
	/// </summary>
	/// <param name="instance">The instance to be set or updated.</param>
	/// <param name="setters">A list of values to be added.</param>
	/// <returns>The value same as <see cref="Collector"/>.</returns>
	public static Collector AddStepSearcherSetter(this Collector instance, params ReadOnlySpan<Action<StepSearcher>> setters)
	{
		foreach (var element in setters)
		{
			instance.Setters.Add(element);
		}
		return instance;
	}
}
