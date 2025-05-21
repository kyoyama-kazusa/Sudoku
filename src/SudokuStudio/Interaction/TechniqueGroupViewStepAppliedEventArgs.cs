namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="TechniqueGroupViewStepAppliedEventHandler"/>.
/// </summary>
/// <param name="step"><inheritdoc cref="ChosenStep" path="/summary"/></param>
/// <seealso cref="TechniqueGroupViewStepAppliedEventHandler"/>
public sealed class TechniqueGroupViewStepAppliedEventArgs(Step step) : EventArgs
{
	/// <summary>
	/// The step.
	/// </summary>
	public Step ChosenStep { get; } = step;
}
