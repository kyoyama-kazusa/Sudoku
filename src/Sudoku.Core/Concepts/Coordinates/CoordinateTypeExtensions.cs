namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Provides with extension methods on <see cref="CoordinateType"/>.
/// </summary>
/// <seealso cref="CoordinateType"/>
public static class CoordinateTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="CoordinateType"/>.
	/// </summary>
	extension(CoordinateType @this)
	{
		/// <summary>
		/// Gets the <see cref="CoordinateConverter"/> instance via the specified <see cref="CoordinateType"/> instance.
		/// </summary>
		/// <returns>
		/// A valid <see cref="CoordinateConverter"/> instance. You can use cast operators to get the instance of desired type.
		/// </returns>
		public CoordinateConverter? Converter
			=> @this switch
			{
				CoordinateType.Literal => new LiteralCoordinateConverter(),
				CoordinateType.RxCy => new RxCyConverter(),
				CoordinateType.K9 => new K9Converter(),
				CoordinateType.Excel => new ExcelCoordinateConverter(),
				_ => null
			};

		/// <summary>
		/// Gets the <see cref="CoordinateParser"/> instance via the specified <see cref="CoordinateType"/> instance.
		/// </summary>
		/// <returns>
		/// A valid <see cref="CoordinateParser"/> instance. You can use cast operators to get the instance of desired type.
		/// </returns>
		public CoordinateParser? Parser
			=> @this switch { CoordinateType.RxCy => new RxCyParser(), CoordinateType.K9 => new K9Parser(), _ => null };
	}
}
