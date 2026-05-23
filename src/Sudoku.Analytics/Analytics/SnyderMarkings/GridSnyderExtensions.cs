namespace Sudoku.Analytics.SnyderMarkings;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>, calculating with Snyder's rules checking.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridSnyderExtensions
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
		.WithOptions(new() { IsDirectMode = true });


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(in Grid @this)
	{
		/// <summary>
		/// Determine whether the grid lacks some candidates that are included in a grid,
		/// through basic elimination rule (Naked Single checking) and specified Snyder's techniques.
		/// </summary>
		/// <param name="techniques">A list of techniques to be checked.</param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when the argument <paramref name="techniques"/> is greater than the maximum value of enumeration field defined.
		/// </exception>
		public bool IsMissingCandidates(SnyderTechnique techniques)
		{
			switch (techniques)
			{
				case SnyderTechnique.None:
				{
					return @this.IsMissingCandidates;
				}
				case < SnyderTechnique.None or > SnyderTechnique.LockedHiddenTriple:
				{
					throw new ArgumentOutOfRangeException(nameof(techniques));
				}
				default:
				{
					var gridResetCandidates = @this.ResetCandidatesGrid;
					foreach (var step in Collector.Collect(gridResetCandidates))
					{
						if (step.Code.ToString() | &SnyderTechnique.Parse | (f => techniques.HasFlag(f)))
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
