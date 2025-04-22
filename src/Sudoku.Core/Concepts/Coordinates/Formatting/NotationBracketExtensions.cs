namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Provides with extension methods on <see cref="NotationBracket"/>.
/// </summary>
/// <seealso cref="NotationBracket"/>
public static class NotationBracketExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="NotationBracket"/>.
	/// </summary>
	extension(NotationBracket @this)
	{
		/// <summary>
		/// Indicates to get open bracket token.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public string? OpenBracket
			=> @this switch
			{
				NotationBracket.None => null,
				NotationBracket.Round => "(",
				NotationBracket.Square => "[",
				NotationBracket.Curly => "{",
				NotationBracket.Angle => "<",
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};

		/// <summary>
		/// Indicates closed bracket token.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public string? ClosedBracket
			=> @this switch
			{
				NotationBracket.None => null,
				NotationBracket.Round => ")",
				NotationBracket.Square => "]",
				NotationBracket.Curly => "}",
				NotationBracket.Angle => ">",
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
