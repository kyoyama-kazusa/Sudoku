namespace System.Text;

/// <summary>
/// Represents a printer that can print table grid and return as a string.
/// </summary>
public static class TableGridBuilder
{
	/// <summary>
	/// Builds a table as a string with optional borders, row separators, and customizable border characters.
	/// Handles alignment with proper width calculation for wide characters (CJK, full-width symbols).
	/// </summary>
	/// <param name="table">List of string arrays, where each array represents a row.</param>
	/// <param name="options">The options to build table.</param>
	/// <returns>The formatted table as a string.</returns>
	public static string BuildTable(ReadOnlySpan<string[]> table, TableGridBuilderOptions? options = null)
	{
		options ??= TableGridBuilderOptions.Default;

		var sbResult = new StringBuilder();
		if (table.Length == 0)
		{
			sbResult.AppendLine("(empty table)");
			return sbResult.ToString();
		}

		// Calculate max width for each column considering wide characters.
		var columnCount = table.Max(static row => row.Length);
		var columnWidths = new int[columnCount];
		foreach (var row in table)
		{
			for (var i = 0; i < row.Length; i++)
			{
				var width = GetDisplayWidth(row[i]);
				if (width > columnWidths[i])
				{
					columnWidths[i] = width;
				}
			}
		}

		// Precompute border line considering custom characters.
		var borderLine = string.Empty;
		if (options.PrintBorders)
		{
			var sbBorder = new StringBuilder();
			sbBorder.Append(options.Corner);
			foreach (var width in columnWidths)
			{
				// Adjust for vertical padding.
				sbBorder.Append(options.Horizontal[0], width + options.Vertical.Length * 2);
				sbBorder.Append(options.Corner);
			}
			borderLine = sbBorder.ToString();
		}

		// Append top border.
		if (options.PrintBorders)
		{
			sbResult.AppendLine(borderLine);
		}

		// Append table rows.
		for (var r = 0; r < table.Length; r++)
		{
			var row = table[r];
			var line = new StringBuilder();
			if (options.PrintBorders)
			{
				line.Append(options.Vertical);
			}

			for (var c = 0; c < columnCount; c++)
			{
				var cell = c < row.Length ? row[c] : string.Empty;
				var pad = columnWidths[c] - GetDisplayWidth(cell);

				line.Append(' ').Append(cell).Append(' ', pad).Append(' ');
				if (options.PrintBorders)
				{
					line.Append(options.Vertical);
				}
			}

			sbResult.AppendLine(line.ToString());

			// Append row separator after each row if enabled.
			if (options.PrintRowSeparators && r < table.Length - 1 && options.PrintBorders)
			{
				sbResult.AppendLine(borderLine);
			}
		}

		// Append bottom border.
		if (options.PrintBorders)
		{
			sbResult.AppendLine(borderLine);
		}

		return sbResult.ToString();
	}

	/// <summary>
	/// Calculates the display width of a string, counting wide characters (CJK, full-width) as 2 units.
	/// </summary>
	private static int GetDisplayWidth(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}

		var width = 0;
		foreach (var ch in text)
		{
			width += IsWideChar(ch) ? 2 : 1;
		}
		return width;
	}

	/// <summary>
	/// Determines whether a character is a wide character (full-width or wide)
	/// according to East Asian Width properties.
	/// This includes CJK ideographs, Hangul, Kana, and full-width forms.
	/// </summary>
	private static bool IsWideChar(char ch)
		=> ch is >= '\u1100' and (
			// Hangul Jamo (U+1100–U+115F)
			// Korean alphabet letters (consonants and vowels).
			<= '\u115F'

			// U+2329 and U+232A: Angle brackets (technical symbols).
			or '\u2329' or '\u232A'

			// U+2E80–U+A4CF: Multiple CJK-related blocks
			// Includes: CJK Radicals Supplement, Kangxi Radicals,
			// CJK Symbols and Punctuation, Hiragana, Katakana,
			// Bopomofo, Hangul Compatibility Jamo,
			// CJK Unified Ideographs Extension A, Yi Syllables, etc..
			or >= '\u2E80' and <= '\uA4CF'

			// Hangul Syllables (U+AC00–U+D7A3)
			// Complete Korean syllable blocks.
			or >= '\uAC00' and <= '\uD7A3'

			// CJK Compatibility Ideographs (U+F900–U+FAFF)
			// Variant forms of Han characters for compatibility.
			or >= '\uF900' and <= '\uFAFF'

			// Vertical Forms (U+FE10–U+FE19)
			// Punctuation for vertical text layout.
			or >= '\uFE10' and <= '\uFE19'

			// CJK Compatibility Forms (U+FE30–U+FE6F)
			// Fullwidth punctuation and compatibility symbols.
			or >= '\uFE30' and <= '\uFE6F'

			// Halfwidth and Fullwidth Forms (U+FF00–U+FF60)
			// Fullwidth ASCII variants: letters, digits, punctuation.
			or >= '\uFF00' and <= '\uFF60'

			// Halfwidth and Fullwidth Forms (U+FFE0–U+FFE6)
			// Fullwidth currency symbols and special signs.
			or >= '\uFFE0' and <= '\uFFE6'
		);
}
