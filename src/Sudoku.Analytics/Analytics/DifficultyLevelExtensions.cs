namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="DifficultyLevel"/>.
/// </summary>
/// <seealso cref="DifficultyLevel"/>
public static class DifficultyLevelExtensions
{
	/// <summary>
	/// Gets the name of the current value, with specified culture.
	/// </summary>
	/// <param name="this">The value.</param>
	/// <param name="formatProvider">The culture.</param>
	/// <returns>The string value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetName(this DifficultyLevel @this, IFormatProvider? formatProvider)
		=> BitOperations.PopCount((int)@this) < 2
			? SR.Get(@this.ToString(), formatProvider as CultureInfo)
			: throw new InvalidOperationException(SR.ExceptionMessage("MultipleFlagsExist"));
}
