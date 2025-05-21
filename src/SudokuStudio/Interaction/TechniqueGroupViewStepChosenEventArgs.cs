namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="TechniqueGroupViewStepChosenEventHandler"/>.
/// </summary>
/// <param name="step"><inheritdoc cref="ChosenStep" path="/summary"/></param>
/// <seealso cref="TechniqueGroupViewStepChosenEventHandler"/>
public sealed class TechniqueGroupViewStepChosenEventArgs(Step step) : EventArgs
{
	/// <summary>
	/// The step.
	/// </summary>
	public Step ChosenStep { get; } = step;
}
