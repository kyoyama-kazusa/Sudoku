namespace Sudoku.GridDifferences;

/// <summary>
/// Provides a way to analyze technique usages on difference between two grids.
/// </summary>
public static class GridDifferenceTechniqueAnalysis
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(GridDifferenceAnalysis)
	{
		/// <summary>
		/// Try to analyze technique used that causes a difference produced from <paramref name="left"/>,
		/// changing into <paramref name="right"/>.
		/// </summary>
		/// <param name="left">The first grid to be checked.</param>
		/// <param name="right">The second grid to be checked.</param>
		/// <param name="collector">The collector instance that is used for collecting found steps.</param>
		/// <param name="step">The step.</param>
		/// <returns>A <see cref="bool"/> result indicating whether such step can be inferred.</returns>
		public static bool TryGetDifferenceTechnique(
			in Grid left,
			in Grid right,
			Collector collector,
			[NotNullWhen(true)] out Step? step
		)
		{
			if (!GridDifferenceAnalysis.TryGetDifference(left, right, out var result)
				|| result.Type is not (GridDifferenceType.AddModifiable or GridDifferenceType.RemoveCandidate)
				|| left.Uniqueness == Uniqueness.Bad || right.Uniqueness == Uniqueness.Bad)
			{
				step = null;
				return false;
			}

			var foundSteps = collector.Collect(left);
			switch (result)
			{
				case AddModifiableGridDifference { Candidates: [var assignment] }:
				{
					foreach (var s in foundSteps)
					{
						if (s.Conclusions.AsSet().Contains(new(Assignment, assignment)))
						{
							step = s;
							return true;
						}
					}
					break;
				}
				case RemoveCandidateGridDifference { Candidates: var eliminations }:
				{
					foreach (var s in foundSteps)
					{
						var possibleEliminations = (
							from conclusion in s.Conclusions
							where conclusion.ConclusionType == Elimination
							select conclusion.Candidate
						).Span.AsCandidateMap();
						if ((possibleEliminations & eliminations) == eliminations)
						{
							step = s;
							return true;
						}
					}
					break;
				}
			}

			step = null;
			return false;
		}
	}
}
