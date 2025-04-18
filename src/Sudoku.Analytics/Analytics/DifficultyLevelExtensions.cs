namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="DifficultyLevel"/>.
/// </summary>
/// <seealso cref="DifficultyLevel"/>
public static class DifficultyLevelExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="DifficultyLevel"/>.
	/// </summary>
	extension(DifficultyLevel @this)
	{
		/// <summary>
		/// Gets the name of the current value, with specified culture.
		/// </summary>
		/// <param name="formatProvider">The culture.</param>
		/// <returns>The string value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetName(IFormatProvider? formatProvider)
			=> BitOperations.PopCount((int)@this) < 2
				? SR.Get(@this.ToString(), formatProvider as CultureInfo)
				: throw new InvalidOperationException(SR.ExceptionMessage("MultipleFlagsExist"));
	}
}
