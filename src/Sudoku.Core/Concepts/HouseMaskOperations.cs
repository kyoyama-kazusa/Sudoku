namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of methods handling with <see cref="HouseMask"/> instances.
/// </summary>
/// <seealso cref="HouseMask"/>
public static class HouseMaskOperations
{
	/// <summary>
	/// Indicates the mask that means all blocks.
	/// </summary>
	public const HouseMask AllBlocksMask = Grid.MaxCandidatesMask;

	/// <summary>
	/// Indicates the mask that means all rows.
	/// </summary>
	public const HouseMask AllRowsMask = Grid.MaxCandidatesMask << 9;

	/// <summary>
	/// Indicates the mask that means all columns.
	/// </summary>
	public const HouseMask AllColumnsMask = Grid.MaxCandidatesMask << 18;

	/// <summary>
	/// Indicates the mask that means all houses.
	/// </summary>
	public const HouseMask AllHousesMask = (1 << 27) - 1;
}
