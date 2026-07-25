namespace SudokuStudio.Markup;

/// <summary>
/// Defines a markup extension that can parse a <see cref="string"/> value, converting it into a valid <see cref="int"/>
/// or throwing exceptions when the code is invalid.
/// </summary>
/// <typeparam name="TNumber">The type of the number.</typeparam>
[ContentProperty(Name = nameof(Value))]
public closed class NumberExtension<TNumber> : MarkupExtension where TNumber : unmanaged, INumber<TNumber>
{
	/// <summary>
	/// Indicates the parsed string.
	/// </summary>
	public string? ParseString { get; set; }

	/// <summary>
	/// Indicates the target value input.
	/// </summary>
	public TNumber Value { get; set; }


	/// <inheritdoc/>
	protected sealed override object ProvideValue() => ParseString is not null ? TNumber.Parse(ParseString, null) : Value;
}
