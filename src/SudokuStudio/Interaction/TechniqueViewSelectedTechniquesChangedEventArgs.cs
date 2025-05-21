namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="TechniqueViewSelectedTechniquesChangedEventHandler"/>.
/// </summary>
/// <param name="techniqueSet"><inheritdoc cref="TechniqueSet" path="/summary"/></param>
/// <seealso cref="TechniqueViewSelectedTechniquesChangedEventHandler"/>
public sealed class TechniqueViewSelectedTechniquesChangedEventArgs(params TechniqueSet techniqueSet) : EventArgs
{
	/// <summary>
	/// The technique set to be assigned.
	/// </summary>
	public TechniqueSet TechniqueSet { get; } = techniqueSet;
}
