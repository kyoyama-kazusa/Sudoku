namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a bindable source for a <see cref="Technique"/>.
/// </summary>
/// <param name="techniqueField">Indicates the technique field.</param>
/// <seealso cref="Technique"/>
public sealed partial class TechniqueViewBindableSource([Property(Setter = PropertySetters.Init)] Technique techniqueField)
{
	/// <summary>
	/// Indicates the name of the technique.
	/// </summary>
	public string TechniqueName => TechniqueField.GetName(App.CurrentCulture);

	/// <summary>
	/// Indicates the containing group for the current technique.
	/// </summary>
	public TechniqueGroup ContainingGroup => TechniqueField.Group;
}
