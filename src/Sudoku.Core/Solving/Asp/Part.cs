namespace Sudoku.Solving.Asp;

/// <summary>
/// Corresponds to C's <c>clingo_part_t</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Part
{
	/// <summary>
	/// The name of the program part, e.g. <c>"base"</c>.
	/// </summary>
	[MarshalAs(UnmanagedType.LPStr)]
	public string? Name;

	/// <summary>
	/// The pointer to the parameter array (<c>clingo_symbol_t const*</c>). Pass 0 if there are no parameters.
	/// </summary>
	public nint Parameters;

	/// <summary>
	/// The number of parameters.
	/// </summary>
	public nuint Size;
}
