namespace Sudoku.Categorization;

/// <summary>
/// Provides with extension methods on <see cref="SingleTechniqueFlag"/>.
/// </summary>
/// <seealso cref="SingleTechniqueFlag"/>
public static class SingleTechniqueFlagExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="SingleTechniqueFlag"/>.
	/// </summary>
	extension(SingleTechniqueFlag @this)
	{
		/// <summary>
		/// Try to get the name of the current <see cref="SingleTechniqueFlag"/>.
		/// </summary>
		/// <param name="formatProvider">The culture information.</param>
		/// <returns>The name of the current technique.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the target technique is out of range.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetName(IFormatProvider? formatProvider)
			=> @this switch
			{
				SingleTechniqueFlag.FullHouse => Technique.FullHouse.GetName(formatProvider),
				SingleTechniqueFlag.LastDigit => Technique.LastDigit.GetName(formatProvider),
				SingleTechniqueFlag.HiddenSingle => SR.Get("SingleTechnique_HiddenSingle", formatProvider as CultureInfo),
				SingleTechniqueFlag.HiddenSingleBlock => Technique.CrosshatchingBlock.GetName(formatProvider),
				SingleTechniqueFlag.HiddenSingleRow => Technique.CrosshatchingRow.GetName(formatProvider),
				SingleTechniqueFlag.HiddenSingleColumn => Technique.CrosshatchingColumn.GetName(formatProvider),
				SingleTechniqueFlag.NakedSingle => Technique.NakedSingle.GetName(formatProvider),
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
