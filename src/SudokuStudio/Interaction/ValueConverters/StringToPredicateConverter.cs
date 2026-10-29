namespace SudokuStudio.Interaction.ValueConverters;

/// <summary>
/// Converts a <see cref="string"/> value into a predicate that can return a <see cref="bool"/> result.
/// </summary>
public sealed class StringToPredicateConverter : IValueConverter
{
	/// <summary>
	/// Indicates whether the value conversion is reverted.
	/// </summary>
	public bool IsInverted { get; set; }

	/// <summary>
	/// Indicates whether the converter will handle for whitespaces.
	/// </summary>
	public bool EnforceNonWhiteSpaceString { get; set; }


	/// <inheritdoc/>
	public object Convert(object? value, Type? targetType, object? parameter, string? language)
		=> value is string target
		&& unsafe (
			(EnforceNonWhiteSpaceString ? &string.IsNullOrWhiteSpace : (delegate*<string, bool>)&string.IsNullOrEmpty) is var checkerFunc
				? IsInverted ? !checkerFunc(target) : checkerFunc(target)
				: IsInverted ? value is not null : value is null
		);

	/// <inheritdoc/>
	[DoesNotReturn]
	public object ConvertBack(object? value, Type? targetType, object? parameter, string? language) => throw new NotImplementedException();
}
