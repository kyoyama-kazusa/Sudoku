namespace Sudoku.MinlexOrder;

public partial class MinlexRanker
{
	/// <summary>
	/// Provides interoperability for type <see cref="MinlexRanker"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This type is implemented in external code.
	/// You must copy all <c>*.dll</c> files in folder <c>miscellaneous/minlex-rank</c> in this repository to your device,
	/// before consuming functions defined in this type;
	/// otherwise, <see cref="BadImageFormatException"/> or <see cref="EntryPointNotFoundException"/> will be thrown.
	/// </para>
	/// <para>
	/// For more information you can visit test code:
	/// <see href="https://github.com/GPenet/Virtual-catalog-DLL/blob/main/sktestvcat.cpp"/>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://github.com/GPenet/Virtual-calatog">Virtual Catalog (GitHub repository)</seealso>
	/// <seealso href="http://forum.enjoysudoku.com/virtual-catalog-dll-t45193.html">Virtual catalog DLL (EnjoySudoku forum)</seealso>
	/// <seealso cref="MinlexRanker"/>
	/// <seealso cref="BadImageFormatException"/>
	/// <seealso cref="EntryPointNotFoundException"/>
	private static partial class Interop
	{
		/// <summary>
		/// Init memory via the specified mode.
		/// </summary>
		/// <param name="mode">
		/// The mode. There're 3 possible values to set:
		/// <list type="table">
		/// <listheader>
		/// <term>Mode</term>
		/// <description>Meaning</description>
		/// </listheader>
		/// <item>
		/// <term>0</term>
		/// <description>Sequential process on a virtual file</description>
		/// </item>
		/// <item>
		/// <term>1</term>
		/// <description>Rank to solution grid</description>
		/// </item>
		/// <item>
		/// <term>2</term>
		/// <description>Solution grid to rank</description>
		/// </item>
		/// </list>
		/// </param>
		/// <param name="pe">The memory to exchange.</param>
		/// <returns>-1 for invalid execution and 0 for success.</returns>
		[LibraryImport("skvcat.dll")]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static unsafe partial int SkvcatSetModeGetVCDESK(int mode, out VCDESC* pe);

		/// <summary>
		/// Updates mode value. The mode value can be checked in method <see cref="SkvcatSetModeGetVCDESK(int, out VCDESC*)"/>.
		/// </summary>
		/// <param name="mode">The mode.</param>
		/// <returns>An integer that describes whether the operation is success.</returns>
		/// <remarks>
		/// This method may not be used at present.
		/// </remarks>
		/// <seealso cref="SkvcatSetModeGetVCDESK(int, out VCDESC*)"/>
		[LibraryImport("skvcat.dll")]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static safe partial int SkvcatSetChangeMode(int mode);

		/// <summary>
		/// Try to get rank from a min-lex grid specified as characters.
		/// </summary>
		/// <param name="sgiven">Characters of 81 elements.</param>
		/// <returns>The rank. If failed to find, 0 will be returned; otherwise, a positive integer.</returns>
		[LibraryImport("skvcat.dll")]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static unsafe partial ulong SkvcatGetRankFromSolCharMin(sbyte* sgiven);

		/// <summary>
		/// Try to get min-lex grid via rank value. The result value will be set in specified exchanging buffer variable
		/// defined in method <see cref="SkvcatSetModeGetVCDESK(int, out VCDESC*)"/>.
		/// </summary>
		/// <param name="rank">The rank value.</param>
		/// <returns>0 if success or -1 is failed.</returns>
		/// <seealso cref="SkvcatSetModeGetVCDESK(int, out VCDESC*)"/>
		[LibraryImport("skvcat.dll")]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static safe partial int SkvcatFinSolForRank(ulong rank);
	}
}
