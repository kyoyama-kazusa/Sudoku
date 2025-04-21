namespace Sudoku.Categorization;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>, calculating with partial-marking rules checking.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridPartialMarkingExtensions
{
	/// <summary>
	/// Indicates the backing collector.
	/// </summary>
	internal static readonly Collector Collector = new Collector()
		.WithStepSearchers(
			new SingleStepSearcher { EnableFullHouse = true, EnableLastDigit = true, HiddenSinglesInBlockFirst = true },
			new DirectIntersectionStepSearcher { AllowDirectClaiming = true, AllowDirectPointing = true },
			new DirectSubsetStepSearcher
			{
				AllowDirectHiddenSubset = true,
				AllowDirectLockedHiddenSubset = true,
				AllowDirectLockedSubset = true,
				AllowDirectNakedSubset = true,
				DirectHiddenSubsetMaxSize = 4,
				DirectNakedSubsetMaxSize = 4
			}
		)
		.WithUserDefinedOptions(new() { IsDirectMode = true });


	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Determine whether the grid lacks some candidates that are included in a grid,
		/// through basic elimination rule (Naked Single checking) and specified partial-marking techniques.
		/// </summary>
		/// <param name="techniques">A list of techniques to be checked.</param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when the argument <paramref name="techniques"/> is greater than the maximum value of enumeration field defined.
		/// </exception>
		public bool IsMissingCandidates(PartialMarkingTechniques techniques)
		{
			switch (techniques)
			{
				case PartialMarkingTechniques.None:
				{
					return @this.IsMissingCandidates;
				}
				case < PartialMarkingTechniques.None or > PartialMarkingTechniques.LockedHiddenTriple:
				{
					throw new ArgumentOutOfRangeException(nameof(techniques));
				}
				default:
				{
					var gridResetCandidates = @this.ResetCandidatesGrid;
					foreach (var step in Collector.Collect(gridResetCandidates))
					{
						if (techniques.HasFlag(Enum.Parse<PartialMarkingTechniques>(step.Code.ToString())))
						{
							gridResetCandidates.Apply(step);
						}
					}
					return @this != gridResetCandidates;
				}
			}
		}
	}
}
