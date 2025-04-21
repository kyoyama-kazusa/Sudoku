namespace SudokuStudio.Interaction.Conversions;

/// <summary>
/// Provides with conversion methods used by XAML designer, about backdrop.
/// </summary>
internal static class BackdropConversion
{
	public static int GetSelectedIndex(ComboBox comboBox)
	{
		var backdropKind = Application.Current.AsApp().Preference.UIPreferences.Backdrop;
		var i = 0;
		foreach (var element in comboBox.Items.Cast<ComboBoxItem>())
		{
			if (element.Tag is string s && BackdropKind.TryParse(s, out var target) && target == backdropKind)
			{
				return i;
			}
			i++;
		}
		return -1;
	}
}
