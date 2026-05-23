namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridAnalysisExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
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

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(in Grid @this)
	{
		/// <summary>
		/// Determines whether the puzzle is a minimal puzzle, which means the puzzle will become multiple solution
		/// if arbitrary one given digit will be removed from the grid.
		/// </summary>
		/// <returns>A <see cref="bool"/> result.</returns>
		public bool IsMinimal => @this.CheckMinimal(out _);

		/// <summary>
		/// Checks whether the puzzle can be solved using only full house.
		/// </summary>
		public bool CanPrimaryFullHouse
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true })
				.WithOptions(new() { PrimarySingle = SingleTechniqueFlag.FullHouse })
				.Analyze(@this)
				.IsSolved;

		/// <summary>
		/// Checks whether the puzzle can be solved using only naked single.
		/// </summary>
		public bool CanPrimaryNakedSingle
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true })
				.WithOptions(new() { IsDirectMode = true, PrimarySingle = SingleTechniqueFlag.NakedSingle })
				.Analyze(@this)
				.IsSolved;


		/// <summary>
		/// Checks whether the puzzle can be solved using only full house and hidden single.
		/// </summary>
		/// <param name="allowHiddenSingleInLine">
		/// A <see cref="bool"/> value indicating whether hidden single includes line types.
		/// </param>
		public bool CanPrimaryHiddenSingle(bool allowHiddenSingleInLine)
			=> Analyzer.Default
				.WithStepSearchers(new SingleStepSearcher { EnableFullHouse = true, EnableLastDigit = true })
				.WithOptions(new() { IsDirectMode = true, PrimarySingle = SingleTechniqueFlag.HiddenSingleColumn })
				.Analyze(@this)
				.IsSolved;


		/// <summary>
		/// Determines whether the puzzle is a minimal puzzle, which means the puzzle will become multiple solution
		/// if arbitrary one given digit will be removed from the grid.
		/// </summary>
		/// <param name="firstCandidateMakePuzzleNotMinimal">
		/// <para>
		/// Indicates the first found candidate that can make the puzzle not minimal, which means
		/// if we remove the digit in the cell, the puzzle will still keep unique.
		/// </para>
		/// <para>If the return value is <see langword="true"/>, this argument will be -1.</para>
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <exception cref="InvalidOperationException">Throws when the puzzle is invalid (i.e. not unique).</exception>
		public bool CheckMinimal(out Candidate firstCandidateMakePuzzleNotMinimal)
		{
			if (!@this.IsValid)
			{
				throw new InvalidOperationException(SR.ExceptionMessage("GridMultipleSolutions"));
			}

			switch (@this)
			{
				case { IsSolved: true, GivenCells.Count: 81 }:
				{
					// Very special case: all cells are givens.
					// The puzzle is considered not a minimal puzzle, because any digit in the grid can be removed.
					firstCandidateMakePuzzleNotMinimal = @this.GetDigit(0);
					return false;
				}
				default:
				{
					var gridCopied = @this.UnfixedGrid;
					foreach (var cell in gridCopied.ModifiableCells)
					{
						var newGrid = gridCopied;
						newGrid.SetDigit(cell, -1);
						newGrid.Fix();

						if (newGrid.IsValid)
						{
							firstCandidateMakePuzzleNotMinimal = cell * 9 + @this.GetDigit(cell);
							return false;
						}
					}

					firstCandidateMakePuzzleNotMinimal = -1;
					return true;
				}
			}
		}

		/// <summary>
		/// Try to find backdoors of the grid.
		/// </summary>
		/// <returns>All backdoors found.</returns>
		/// <remarks>
		/// <para>
		/// A backdoor is a placement of one or more candidates in their respective cells,
		/// which allows the player to complete the puzzle with relatively simple solving techniques.
		/// </para>
		/// <para>
		/// Please visit <see href="http://sudopedia.enjoysudoku.com/Backdoor.html">Backdoor</see>
		/// to learn more information about this concept.
		/// </para>
		/// </remarks>
		public ReadOnlySpan<Conclusion> GetBackdoors()
		{
			if (!@this.IsStandard || @this.IsSolved || !@this.IsValid)
			{
				return default;
			}

			var sstsChecker = Analyzer.SstsOnly;
			return sstsChecker.Analyze(@this).IsSolved && @this.SolutionGrid is var solution
				?
				from candidate in @this
				let digit = solution.GetDigit(candidate / 9)
				where digit != -1
				select new Conclusion(digit == candidate % 9 ? Assignment : Elimination, candidate)
				: g(@this);


			ReadOnlySpan<Conclusion> g(in Grid grid)
			{
				var (assignment, elimination, solution) = (new List<Conclusion>(81), new List<Conclusion>(729), grid.SolutionGrid);
				foreach (var cell in grid.EmptyCells)
				{
					// Case 1: Assignments.
					var case1Playground = grid;
					case1Playground.SetDigit(cell, solution.GetDigit(cell));

					if (sstsChecker.Analyze(case1Playground).IsSolved)
					{
						assignment.Add(new(Assignment, cell, solution.GetDigit(cell)));

						// Case 2: Eliminations.
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << solution.GetDigit(cell))))
						{
							var case2Playground = grid;
							case2Playground.SetExistence(cell, digit, false);
							if (sstsChecker.Analyze(case2Playground).IsSolved)
							{
								elimination.Add(new(Elimination, cell, digit));
							}
						}
					}
				}
				return (Conclusion[])[.. assignment, .. elimination];
			}
		}
	}
}
