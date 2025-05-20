namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a bindable source for a <see cref="Technique"/>.
/// </summary>
/// <param name="techniqueField"><inheritdoc cref="TechniqueField" path="/summary"/></param>
/// <seealso cref="Technique"/>
public sealed class TechniqueViewBindableSource(Technique techniqueField)
{
	/// <summary>
	/// Indicates the name of the technique.
	/// </summary>
	public string TechniqueName => TechniqueField.GetName(App.CurrentCulture);

	/// <summary>
	/// Indicates the technique field.
	/// </summary>
	public Technique TechniqueField { get; init; } = techniqueField;

	/// <summary>
	/// Indicates the containing group for the current technique.
	/// </summary>
	public TechniqueGroup ContainingGroup => TechniqueField.Group;
}
