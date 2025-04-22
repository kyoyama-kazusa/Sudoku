namespace Sudoku.Analytics;

public partial class Hub
{
	/// <summary>
	/// Represents naming rules for some techniques.
	/// </summary>
	public static partial class TechniqueNaming
	{
		/// <summary>
		/// Indicates the digit characters.
		/// </summary>
		private const string ChineseDigitCharacters = "\u4e00\u4e8c\u4e09\u56db\u4e94\u516d\u4e03\u516b\u4e5d\u5341";


		[GeneratedRegex($"[{ChineseDigitCharacters}]", RegexOptions.Compiled)]
		public static partial Regex ChineseDigitsPattern { get; }


		/// <summary>
		/// Returns a character that represents the digit.
		/// </summary>
		/// <param name="culture">The culture.</param>
		/// <param name="digit">The digit value.</param>
		/// <returns>The character that represents the specified digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static char GetDigitCharacter(CultureInfo culture, Digit digit)
			=> SR.IsChinese(culture) ? ChineseDigitCharacters[digit] : (char)(digit + '1');

		/// <summary>
		/// Compares Chinese digit characters of two digits.
		/// </summary>
		/// <param name="left">The left value to be compared.</param>
		/// <param name="right">The right value to be compared.</param>
		/// <returns>An <see cref="int"/> indicating which is bigger.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ChineseDigitCompare(Digit left, Digit right)
			=> Math.Sign(ChineseDigitCharacters[left] - ChineseDigitCharacters[right]);

		/// <summary>
		/// Returns corresponding 0-based digit value by the specified digit character in Chinese.
		/// </summary>
		/// <param name="digitCharacter">The digit character.</param>
		/// <returns>The corresponding digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Digit GetChineseDigit(char digitCharacter) => ChineseDigitCharacters.Span.IndexOf(digitCharacter);
	}
}
