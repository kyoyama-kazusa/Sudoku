namespace System.Text;

/// <summary>
/// Represents a type that can analyze text difference and fetch the values on different parts, parsing them into variables.
/// </summary>
/// <param name="literalLength"><inheritdoc cref="_literalLength" path="/summary"/></param>
/// <param name="formattedCount"><inheritdoc cref="_formattedCount" path="/summary"/></param>
/// <param name="original"><inheritdoc cref="_original" path="/summary"/></param>
[InterpolatedStringHandler]
public ref struct VariableExtractor(int literalLength, int formattedCount, string original)
{
	/// <summary>
	/// Indicates the placeholder character.
	/// </summary>
	private const char PlaceholderReservedCharacter = '\0';


#pragma warning disable IDE0052
	/// <summary>
	/// Indicates the number of characters in literal.
	/// </summary>
	private readonly int _literalLength = literalLength;

	/// <summary>
	/// Indicates the number of interpolated items (variables).
	/// </summary>
	private readonly int _formattedCount = formattedCount;

	/// <summary>
	/// Indicates the original string to be parsed.
	/// </summary>
	private readonly string _original = original;
#pragma warning restore IDE0052

	/// <summary>
	/// Indicates the string variable pointers.
	/// </summary>
	private readonly safe string?*[] _pointers = new string*[formattedCount]!;

	/// <summary>
	/// Indicates the backing builder object.
	/// </summary>
	private readonly StringBuilder _builder = new();

	/// <summary>
	/// Indicates the current variable index.
	/// </summary>
	private int _index = 0;


	/// <summary>
	/// Appends a list of unchanged characters into the collection.
	/// </summary>
	/// <param name="s">The sequence of characters, unchanged.</param>
	public readonly void AppendLiteral(string s) => _builder.Append(s);

	/// <summary>
	/// Appends a variable into the collection, recording its variable pointer (address).
	/// </summary>
	/// <param name="variable">The reference to the variable.</param>
	public void AppendFormatted(in string? variable)
	{
		_builder.Append(PlaceholderReservedCharacter);
		unsafe (_pointers[_index++]) = (string?*)Unsafe.AsPointer(ref Unsafe.AsRef(in variable));
	}

	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString() => _builder.ToString().Replace(PlaceholderReservedCharacter, '?');


	/// <summary>
	/// Try to analyze the text difference and assign to interpolated variables.
	/// </summary>
	/// <param name="original">Indicates the original text.</param>
	/// <param name="extractor">Indicates the interpolated string that inserts custom variables to be parsed.</param>
	/// <exception cref="InvalidOperationException">Throws when the text is invalid to be checked.</exception>
	public static void Assign(string original, [InterpolatedStringHandlerArgument(nameof(original))] in VariableExtractor extractor)
	{
		var newString = extractor._builder.ToString();
		var parts = newString.Split(PlaceholderReservedCharacter);
		if (parts.Length < 1)
		{
			return;
		}

		var regexParts = new List<string> { Regex.Escape(parts[0]) };
		for (var i = 1; i < parts.Length; i++)
		{
			regexParts.Add(/*lang = regex*/"(.*?)");
			regexParts.Add(Regex.Escape(parts[i]));
		}

		var match = Regex.Match(original, $"^{string.Concat(regexParts)}$");
		if (match is not { Success: true, Groups: var groups })
		{
			throw new InvalidOperationException("Failed to match string.");
		}

		for (var i = 1; i < groups.Count; i++)
		{
			// Assign string to property.
			Unsafe.AsRef<string?>(unsafe (extractor._pointers[i - 1])) = groups[i].Value;
		}
	}
}
