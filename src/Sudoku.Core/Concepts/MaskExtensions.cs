namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension methods on <see cref="Mask"/>.
/// </summary>
/// <seealso cref="Mask"/>
public static class MaskExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Mask"/>.
	/// </summary>
	extension(Mask @this)
	{
		/// <summary>
		/// Try to convert the current mask value into a valid <see cref="string"/> representation of binary format.
		/// </summary>
		/// <param name="upperCasedPrefix">
		/// Indicates whether the prefix <c>"0b"</c> will become upper-cased (i.e. <c>"0B"</c>).
		/// The default value is <see langword="false"/>.
		/// </param>
		/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToBinaryString(bool upperCasedPrefix = false)
			=> $"0{(upperCasedPrefix ? 'B' : 'b')}{@this.ToStringBase(2, 9)}";

		/// <summary>
		/// Try to convert the current mask value into a valid <see cref="string"/> representation of octal format.
		/// </summary>
		/// <param name="upperCasedPrefix">
		/// Indicates whether the prefix <c>"0o"</c> will become upper-cased (i.e. <c>"0O"</c>).
		/// The default value is <see langword="false"/>.
		/// </param>
		/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToOctalString(bool upperCasedPrefix = false)
			=> $"0{(upperCasedPrefix ? 'O' : 'o')}{@this.ToStringBase(8, 3)}";

		/// <summary>
		/// Try to convert the current mask value into a valid <see cref="string"/> representation of hexadecimal format.
		/// </summary>
		/// <param name="upperCasedPrefix">
		/// Indicates whether the prefix <c>"0x"</c> will become upper-cased (i.e. <c>"0X"</c>).
		/// The default value is <see langword="false"/>.
		/// </param>
		/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToHexadecimalString(bool upperCasedPrefix = false)
			=> $"0{(upperCasedPrefix ? 'X' : 'x')}{@this.ToStringBase(16, 3)}";

		/// <summary>
		/// Convert the current instance into a <see cref="string"/> representation in numeral literal,
		/// with the specified base (2, 8, 10 or 16).
		/// </summary>
		/// <param name="base">The base value. The value must be 2, 8, 10 or 16.</param>
		/// <returns>The string.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToStringBase(int @base) => Convert.ToString(@this, @base);

		/// <summary>
		/// Convert the current instance into a <see cref="string"/> representation in numeral literal,
		/// with the specified base (2, 8, 10 or 16), padding some zeros.
		/// </summary>
		/// <param name="base">The base value. The value must be 2, 8, 10 or 16.</param>
		/// <param name="totalWidth">The total width.</param>
		/// <returns>The string.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToStringBase(int @base, int totalWidth) => Convert.ToString(@this, @base).PadLeft(totalWidth, '0');
	}

	/// <summary>
	/// Provides extension members on <see cref="Mask"/>.
	/// </summary>
	extension(Mask)
	{
		/// <summary>
		/// Creates for a <see cref="Mask"/> instance via the specified digits.
		/// </summary>
		/// <param name="digits">
		/// <para>Indicates the digits to assign.</para>
		/// <include file="../../global-doc-comments.xml" path="//g/csharp12/feature[@name='params-collections']/target[@name='parameter']"/>
		/// </param>
		/// <returns>A <see cref="Mask"/> instance.</returns>
		public static Mask Create(params ReadOnlySpan<Digit> digits)
		{
			var result = (Mask)0;
			foreach (var digit in digits)
			{
				result |= (Mask)(1 << digit);
			}
			return result;
		}

		/// <inheritdoc cref="Create(ReadOnlySpan{Digit})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Mask Create(Digit[] digits) => Create(digits.AsReadOnlySpan());

		/// <typeparam name="TDigits">The type of the enumerable sequence.</typeparam>
		/// <inheritdoc cref="Create(ReadOnlySpan{Digit})"/>
		public static Mask Create<TDigits>(TDigits digits) where TDigits : IEnumerable<Digit>, allows ref struct
		{
			var result = (Mask)0;
			foreach (var digit in digits)
			{
				result |= (Mask)(1 << digit);
			}
			return result;
		}
	}
}
