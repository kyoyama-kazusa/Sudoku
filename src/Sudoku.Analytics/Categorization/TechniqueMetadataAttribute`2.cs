namespace Sudoku.Categorization;

/// <summary>
/// <inheritdoc cref="TechniqueMetadataAttribute" path="/summary"/>
/// </summary>
/// <typeparam name="TStepSearcher">The type of step searcher.</typeparam>
/// <typeparam name="TStep">The type of step.</typeparam>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class TechniqueMetadataAttribute<TStepSearcher, TStep> : TechniqueMetadataAttribute
	where TStepSearcher : StepSearcher
	where TStep : Step
{
	/// <inheritdoc/>
	public override Type StepSearcherType => typeof(TStepSearcher);

	/// <inheritdoc/>
	public override Type StepType => typeof(TStep);
}
