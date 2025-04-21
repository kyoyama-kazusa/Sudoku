namespace SudokuStudio.Interaction.ValueConverters;

/// <summary>
/// Represents a value converter that can convert a <see cref="List{T}"/> of <see cref="Technique"/> supported by <see cref="DisorderedIttoryuFinder"/> instances
/// into a <see cref="bool"/> control determining whether the corresponding control is set as "on" value.
/// </summary>
/// <seealso cref="Technique"/>
/// <seealso cref="DisorderedIttoryuFinder"/>
internal sealed class IttoryuSupportedTechniquesValueConverter : IValueConverter
{
	/// <inheritdoc/>
	public object Convert(object value, Type targetType, object parameter, string language)
		=> (value, parameter) is (List<Technique> item, string rawTechniqueName)
		&& Technique.TryParse(rawTechniqueName, out var techniqueName) && item.Contains(techniqueName);

	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">Throws when <paramref name="parameter"/> or <paramref name="value"/> is invalid.</exception>
	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		const string error_Value = $"The target value '{nameof(value)}' is invalid - it must be a boolean value (true or false).";
		const string error_Parameter = $"The target value '{nameof(parameter)}' is invalid - it must be the string representation a field in type '{nameof(Technique)}'.";

		if (value is not bool isOn)
		{
			throw new InvalidOperationException(error_Value);
		}

		var result = Application.Current.AsApp().Preference.AnalysisPreferences.IttoryuSupportedTechniques;
		if (parameter is not string rawTechniqueName || !Technique.TryParse(rawTechniqueName, out var technique))
		{
			throw new InvalidOperationException(error_Parameter);
		}

		(isOn ? new Action<Technique>(result.Add) : t => result.Remove(t))(technique);
		return result;
	}
}
