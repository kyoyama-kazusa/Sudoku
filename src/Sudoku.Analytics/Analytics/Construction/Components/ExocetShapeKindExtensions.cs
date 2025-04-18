namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods around enumeration type <see cref="ExocetShapeKind"/>.
/// </summary>
internal static class ExocetShapeKindExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="IComplexSeniorExocet"/>.
	/// </summary>
	extension(IComplexSeniorExocet @this)
	{
		/// <summary>
		/// Indicates the shape kind of the current exocet pattern.
		/// </summary>
		public ExocetShapeKind ShapeKind
		{
			get
			{
				var finalMask = @this.CrosslineHousesMask | @this.ExtraHousesMask;
				return (
					finalMask & HouseMaskOperations.AllBlocksMask,
					finalMask & HouseMaskOperations.AllRowsMask,
					finalMask & HouseMaskOperations.AllColumnsMask
				) switch
				{
					(_, not 0, not 0) => ExocetShapeKind.Mutant,
					(not 0, _, _) => ExocetShapeKind.Franken,
					_ => ExocetShapeKind.Basic
				};
			}
		}
	}
}
