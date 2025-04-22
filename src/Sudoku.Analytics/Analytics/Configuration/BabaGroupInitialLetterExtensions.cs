namespace Sudoku.Analytics.Configuration;

/// <summary>
/// Provides with extension methods on <see cref="BabaGroupInitialLetter"/> instances.
/// </summary>
/// <seealso cref="BabaGroupInitialLetter"/>
public static class BabaGroupInitialLetterExtensions
{
	/// <summary>
	/// Indicates the sequences.
	/// </summary>
	private static readonly Dictionary<(BabaGroupInitialLetter Letter, BabaGroupLetterCasing Casing), string> CharSequences = new()
	{
		{ (BabaGroupInitialLetter.Digit_Zero, BabaGroupLetterCasing.Upper), "\u24ea\u2460\u2461\u2462\u2463\u2464\u2465\u2466\u2467" },
		{ (BabaGroupInitialLetter.Digit_Zero, BabaGroupLetterCasing.Lower), "012345678" },
		{ (BabaGroupInitialLetter.Digit_One, BabaGroupLetterCasing.Upper), "\u2460\u2461\u2462\u2463\u2464\u2465\u2466\u2467\u2468" },
		{ (BabaGroupInitialLetter.Digit_One, BabaGroupLetterCasing.Lower), "123456789" },
		{ (BabaGroupInitialLetter.RomanticNumber_One, BabaGroupLetterCasing.Upper), "\u2160\u2161\u2162\u2163\u2164\u2165\u2166\u2167\u2168" },
		{ (BabaGroupInitialLetter.RomanticNumber_One, BabaGroupLetterCasing.Lower), "\u2170\u2171\u2172\u2173\u2174\u2175\u2176\u2177\u2178" },
		{ (BabaGroupInitialLetter.EnglishLetter_A, BabaGroupLetterCasing.Upper), "ABCDEFGHI" },
		{ (BabaGroupInitialLetter.EnglishLetter_A, BabaGroupLetterCasing.Lower), "abcdefghi" },
		{ (BabaGroupInitialLetter.EnglishLetter_X, BabaGroupLetterCasing.Upper), "XYZWVUTSR" },
		{ (BabaGroupInitialLetter.EnglishLetter_X, BabaGroupLetterCasing.Lower), "xyzwvutsr" },
		{ (BabaGroupInitialLetter.GreeceLetter_Alpha, BabaGroupLetterCasing.Upper), "\x391\x392\x393\x394\x395\x396\x397\x398\x399" },
		{ (BabaGroupInitialLetter.GreeceLetter_Alpha, BabaGroupLetterCasing.Lower), "\x3b1\x3b2\x3b3\x3b4\x3b5\x3b6\x3b7\x3b8\x3b9" },
		{ (BabaGroupInitialLetter.ChineseCharacter_One, BabaGroupLetterCasing.Upper), "\u4e00\u4e8c\u4e09\u56db\u4e94\u516d\u4e03\u516b\u4e5d" },
		{ (BabaGroupInitialLetter.ChineseCharacter_One, BabaGroupLetterCasing.Lower), "\u4e00\u4e8c\u4e09\u56db\u4e94\u516d\u4e03\u516b\u4e5d" },
		{ (BabaGroupInitialLetter.ChineseHavenlyStem_Jia, BabaGroupLetterCasing.Upper), "\u7532\u4e59\u4e19\u4e01\u620a\u5df1\u5e9a\u8f9b\u58ec" },
		{ (BabaGroupInitialLetter.ChineseHavenlyStem_Jia, BabaGroupLetterCasing.Lower), "\u7532\u4e59\u4e19\u4e01\u620a\u5df1\u5e9a\u8f9b\u58ec" },
		{ (BabaGroupInitialLetter.ChineseEarthlyBranch_Zi, BabaGroupLetterCasing.Upper), "\u5b50\u4e11\u5bc5\u536f\u8fb0\u5df3\u5348\u672a\u7533" },
		{ (BabaGroupInitialLetter.ChineseEarthlyBranch_Zi, BabaGroupLetterCasing.Lower), "\u5b50\u4e11\u5bc5\u536f\u8fb0\u5df3\u5348\u672a\u7533" },
		{ (BabaGroupInitialLetter.CapitalChineseCharacter_One, BabaGroupLetterCasing.Upper), "\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396" },
		{ (BabaGroupInitialLetter.CapitalChineseCharacter_One, BabaGroupLetterCasing.Lower), "\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396" },
		{ (BabaGroupInitialLetter.Kanji_One, BabaGroupLetterCasing.Upper), "\u58f1\u5f10\u53c2\u8086\u4f0d\u9678\u6f06\u634c\u7396" },
		{ (BabaGroupInitialLetter.Kanji_One, BabaGroupLetterCasing.Lower), "\u58f1\u5f10\u53c2\u8086\u4f0d\u9678\u6f06\u634c\u7396" }
	};


	/// <summary>
	/// Provides extension members on <see cref="BabaGroupInitialLetter"/>.
	/// </summary>
	extension(BabaGroupInitialLetter @this)
	{
		/// <summary>
		/// Try to get character sequence from the specified initial letter.
		/// </summary>
		/// <param name="casing">The letter casing.</param>
		/// <returns>The character sequence.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the specified arguments are not defined.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<char> GetSequence(BabaGroupLetterCasing casing)
			=> Enum.IsDefined(@this) && Enum.IsDefined(casing)
				? CharSequences[(@this, casing)].Span
				: throw new ArgumentOutOfRangeException(nameof(@this));

		/// <summary>
		/// Try to escape the digit.
		/// </summary>
		/// <param name="digit">The digit to be escaped.</param>
		/// <returns>The escaped character. If a digit doesn't need to escape, return character representation of itself.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char EscapeDigit(Digit digit)
			=> @this switch
			{
				BabaGroupInitialLetter.Digit_One or BabaGroupInitialLetter.Digit_Zero => (char)(digit + '\u2474'),
				_ => (char)(digit + '1')
			};
	}
}
