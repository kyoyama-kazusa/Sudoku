namespace SudokuStudio.Interaction.ValueConverters;

/// <summary>
/// Represents a value converter that can convert a <see cref="TransformType"/> to a <see cref="bool"/> value.
/// </summary>
internal sealed class TransformTypeToBooleanConverter : IValueConverter
{
	/// <inheritdoc/>
	public object Convert(object value, Type targetType, object parameter, string language)
		=> (value, parameter) is (TransformType items, string rawFlag)
		&& TransformType.TryParse(rawFlag, out var flag) && items.HasFlag(flag);

	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">Throws when <paramref name="parameter"/> or <paramref name="value"/> is invalid.</exception>
	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		const string error_Value = $"The target value '{nameof(value)}' is invalid - it must be a boolean value (true or false).";
		const string error_Parameter = $"The target value '{nameof(parameter)}' is invalid - it must be the string representation a field in type '{nameof(StepTooltipDisplayItems)}'.";

		return (Application.Current.AsApp().Preference.LibraryPreferences.LibraryPuzzleTransformations, parameter) switch
		{
			(var items, string rawFlag) when TransformType.TryParse(rawFlag, out var flag) => value switch
			{
				true => items | flag,
				false => items & ~flag,
				_ => throw new InvalidOperationException(error_Value)
			},
			_ => throw new InvalidOperationException(error_Parameter)
		};
	}
}
