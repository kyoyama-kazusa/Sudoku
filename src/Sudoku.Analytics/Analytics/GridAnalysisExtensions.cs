namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridAnalysisExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Applies for all conclusions into the current <see cref="Grid"/> instance.
		/// </summary>
		/// <param name="step">A conclusion-provider <see cref="Step"/> instance.</param>
		public void Apply(Step step)
		{
			foreach (var conclusion in step.Conclusions)
			{
				@this.Apply(conclusion);
			}
		}
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Checks whether the puzzle can be solved using only full house.
		/// </summary>
		public bool CanPrimaryFullHouse
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true })
				.WithUserDefinedOptions(new() { PrimarySingle = SingleTechniqueFlag.FullHouse })
				.Analyze(in @this)
				.IsSolved;

		/// <summary>
		/// Checks whether the puzzle can be solved using only naked single.
		/// </summary>
		public bool CanPrimaryNakedSingle
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true })
				.WithUserDefinedOptions(new() { IsDirectMode = true, PrimarySingle = SingleTechniqueFlag.NakedSingle })
				.Analyze(in @this)
				.IsSolved;


		/// <summary>
		/// Checks whether the puzzle can be solved using only full house and hidden single.
		/// </summary>
		/// <param name="allowHiddenSingleInLine">
		/// A <see cref="bool"/> value indicating whether hidden single includes line types.
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanPrimaryHiddenSingle(bool allowHiddenSingleInLine)
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true, EnableLastDigit = true })
				.WithUserDefinedOptions(
					new()
					{
						IsDirectMode = true,
						PrimarySingle = SingleTechniqueFlag.HiddenSingle,
						PrimaryHiddenSingleAllowsLines = allowHiddenSingleInLine
					}
				)
				.Analyze(in @this)
				.IsSolved;
	}
}
