namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension methods on <see cref="HouseMask"/>.
/// </summary>
public static class HouseMaskExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="HouseMask"/>.
	/// </summary>
	extension(HouseMask)
	{
		/// <summary>
		/// Creates for a <see cref="HouseMask"/> instance via the specified houses.
		/// </summary>
		/// <param name="houses">The houses.</param>
		/// <returns>A <see cref="HouseMask"/> instance.</returns>
		public static HouseMask Create(ReadOnlySpan<House> houses)
		{
			var result = 0;
			foreach (var house in houses)
			{
				result |= 1 << house;
			}
			return result;
		}
	}
}
