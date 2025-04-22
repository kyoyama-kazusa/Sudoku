namespace SudokuStudio.Interaction;

/// <summary>
/// Provides with extension methods on <see cref="SudokuFormatFlags"/>.
/// </summary>
/// <seealso cref="SudokuFormatFlags"/>
internal static class SudokuFormatFlagsExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="SudokuFormatFlags"/>.
	/// </summary>
	extension(SudokuFormatFlags @this)
	{
		/// <summary>
		/// Try to get target <see cref="GridFormatInfo{TGrid}"/> instance of type <see cref="Grid"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public GridFormatInfo<Grid> Converter
			=> @this switch
			{
				SudokuFormatFlags.InitialFormat => new SusserGridFormatInfo<Grid>(),
				SudokuFormatFlags.CurrentFormat => new SusserGridFormatInfo<Grid> { WithCandidates = true, WithModifiables = true },
				SudokuFormatFlags.CurrentFormatIgnoringValueKind
					=> new SusserGridFormatInfo<Grid> { WithModifiables = true, WithCandidates = true, TreatValueAsGiven = true },
				SudokuFormatFlags.MultipleGridFormat => new MultipleLineGridFormatInfo { RemoveGridLines = true },
				SudokuFormatFlags.PencilMarkFormat => new PencilmarkGridFormatInfo { SubtleGridLines = true },
				SudokuFormatFlags.SukakuFormat => new SukakuGridFormatInfo(),
				SudokuFormatFlags.ExcelFormat => new CsvGridFormatInfo(),
				SudokuFormatFlags.OpenSudokuFormat => new OpenSudokuGridFormatInfo(),
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
