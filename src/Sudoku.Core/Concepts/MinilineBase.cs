namespace Sudoku.Concepts;

/// <summary>
/// Defines a pair of houses that means the target pair can form an intersection by the specified line and block.
/// </summary>
/// <param name="Line">The index of the line.</param>
/// <param name="Block">The index of the block.</param>
public readonly record struct MinilineBase(byte Line, byte Block) : IEqualityOperators<MinilineBase, MinilineBase, bool>;
