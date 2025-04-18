namespace Sudoku.Concepts;

/// <summary>
/// Provides extension methods on <see cref="HouseType"/>.
/// </summary>
/// <seealso cref="HouseType"/>
public static class HouseTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="HouseType"/>.
	/// </summary>
	extension(HouseType @this)
	{
		/// <summary>
		/// Try to get the label of the specified house type.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public char Label
			=> @this switch
			{
				HouseType.Row => 'r',
				HouseType.Column => 'c',
				HouseType.Block => 'b',
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};

		/// <summary>
		/// Gets the ordering of the house type. The result value will be 0, 1 and 2.
		/// </summary>
		internal int ProgramOrder => @this switch { HouseType.Block => 2, HouseType.Row => 0, HouseType.Column => 1 };
	}
}
