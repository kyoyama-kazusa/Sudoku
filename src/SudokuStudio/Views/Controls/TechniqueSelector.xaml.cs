namespace SudokuStudio.Views.Controls;

/// <summary>
/// Represents a technique selector control.
/// </summary>
public sealed partial class TechniqueSelector : UserControl
{
	/// <summary>
	/// Initializes a <see cref="TechniqueSelector"/> instance.
	/// </summary>
	public TechniqueSelector() => InitializeComponent();


	/// <summary>
	/// Indicates the selected index.
	/// </summary>
	[DependencyProperty]
	public partial int SelectedIndex { get; set; }

	/// <summary>
	/// Indicates the base items source.
	/// </summary>
	internal TechniqueBindableSource[] ItemsSource
		=>
		from @field in Technique.Values.ToArray()
		let feature = @field.Features
		where feature is 0 or TechniqueFeatures.HardToBeGenerated or TechniqueFeatures.DirectTechniques
		let displayName = @field == 0 ? SR.Get("TechniqueSelector_NoTechniqueSelected", App.CurrentCulture) : @field.GetName(App.CurrentCulture)
		select new TechniqueBindableSource { DisplayName = displayName, Technique = @field, Feature = feature };


	/// <inheritdoc cref="Selector.SelectionChanged"/>
	public event SelectionChangedEventHandler? SelectionChanged;


	private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => SelectionChanged?.Invoke(this, e);
}
