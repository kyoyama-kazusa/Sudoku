namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="TechniqueViewCurrentSelectedTechniqueChangedEventHandler"/>.
/// </summary>
/// <param name="technique"><inheritdoc cref="Technique" path="/summary"/></param>
/// <param name="isSelected"><inheritdoc cref="IsSelected" path="/summary"/></param>
/// <seealso cref="TechniqueViewCurrentSelectedTechniqueChangedEventHandler"/>
public sealed class TechniqueViewCurrentSelectedTechniqueChangedEventArgs(Technique technique, bool isSelected) : EventArgs
{
	/// <summary>
	/// Indicates whether the field is selected.
	/// </summary>
	public bool IsSelected { get; } = isSelected;

	/// <summary>
	/// The technique to be assigned.
	/// </summary>
	public Technique Technique { get; } = technique;
}
