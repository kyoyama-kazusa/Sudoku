namespace Sudoku.Generating;

/// <summary>
/// Represents a generator that is based on pattern.
/// </summary>
/// <param name="missingDigit">Indicates the missing digit that can be used.</param>
/// <param name="seedPattern">Indicates the predefind pattern used.</param>
[StructLayout(LayoutKind.Auto)]
public ref partial struct PatternBasedPuzzleGenerator(
	[Field] ref readonly CellMap seedPattern,
	[Property] Digit missingDigit = -1
) : IGenerator<Grid>
{
	/// <summary>
	/// Indicates whether the generator is cancelled.
	/// </summary>
	private bool? _isCancelled;


	/// <summary>
	/// Indicates the pattern of cells.
	/// </summary>
	public readonly ref readonly CellMap SeedPattern => ref _seedPattern;


	/// <inheritdoc cref="IGenerator{TResult}.Generate(IProgress{GeneratorProgress}, CancellationToken)"/>
	public Grid Generate(IProgress<GeneratorProgress>? progress = null, CancellationToken cancellationToken = default)
	{
		// Reset the state.
		_isCancelled = false;

		try
		{
			if (_seedPattern.Count < 17)
			{
				return Grid.Undefined;
			}

			var resultGrid = Grid.Undefined;
			try
			{
				var (playground, count, cellsOrdered) = (Grid.Empty, 0, OrderCellsViaConnectionComplexity());
				GenerateCore(cellsOrdered, ref playground, ref resultGrid, 0, ref count, progress, cancellationToken);
			}
			catch
			{
				throw;
			}
			return resultGrid;
		}
		finally
		{
			_isCancelled = null;
		}
	}

	/// <summary>
	/// The back method to generate a valid sudoku grid puzzle.
	/// </summary>
	/// <param name="patternCellsSorted">The cells ordered by the number of related cells.</param>
	/// <param name="playground">The playground to be operated with.</param>
	/// <param name="resultGrid">The result grid to be returned.</param>
	/// <param name="i">The index that the current searching is on.</param>
	/// <param name="count">The number of puzzles generated.</param>
	/// <param name="progress">The progress instance.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the operation.</param>
	/// <returns>A <see cref="bool"/> value indicating whether a puzzle was generated or a user cancelled the operation.</returns>
	private bool GenerateCore(
		Cell[] patternCellsSorted,
		ref Grid playground,
		ref Grid resultGrid,
		int i,
		ref int count,
		IProgress<GeneratorProgress>? progress,
		CancellationToken cancellationToken
	)
	{
		if (i == patternCellsSorted.Length)
		{
			var result = playground.FixedGrid;
			if (result.GetUniqueness() == Uniqueness.Unique)
			{
				resultGrid = result;
				return true;
			}

			progress?.Report(new(count++));
			return false;
		}

		var cell = patternCellsSorted[i];
		var digitsMask = playground.GetCandidates(cell);
		var digits = ((Mask)(MissingDigit != -1 ? digitsMask & ~(1 << MissingDigit) : digitsMask)).AllSets;
		var indexArray = Digits[..digits.Length];
		Random.Shared.Shuffle(indexArray);
		foreach (var randomizedIndex in indexArray)
		{
			playground.ReplaceDigit(cell, digits[randomizedIndex]);

			if (playground.FixedGrid.GetUniqueness() == Uniqueness.Bad)
			{
				playground.SetDigit(cell, -1);
				continue;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				_isCancelled = true;
				return true;
			}

			if (GenerateCore(patternCellsSorted, ref playground, ref resultGrid, i + 1, ref count, progress, cancellationToken))
			{
				return true;
			}
		}

		playground.SetDigit(cell, -1);
		return false;
	}

	/// <summary>
	/// Order the pattern cells via connection complexity.
	/// </summary>
	/// <returns>The cells ordered.</returns>
	private readonly Cell[] OrderCellsViaConnectionComplexity()
	{
		var (isOrdered, result) = (CellMap.Empty, new Cell[_seedPattern.Count]);
		for (var index = 0; index < _seedPattern.Count; index++)
		{
			var (maxRating, best) = (0, -1);
			for (var i = 0; i < 81; i++)
			{
				if (!_seedPattern.Contains(i) || isOrdered.Contains(i))
				{
					continue;
				}

				var rating = 0;
				for (var j = 0; j < 81; j++)
				{
					if (!_seedPattern.Contains(j) || i == j)
					{
						continue;
					}

					if (i.ToHouse(HouseType.Block) == j.ToHouse(HouseType.Block)
						|| i.ToHouse(HouseType.Row) == j.ToHouse(HouseType.Row)
						|| i.ToHouse(HouseType.Column) == j.ToHouse(HouseType.Column))
					{
						rating += isOrdered.Contains(j) ? 10000 : 100;
					}

					if (!isOrdered.Contains(j)
						&& (i.ToBand() == j.ToBand() || i.ToTower() == j.ToTower())
						&& i.ToHouse(HouseType.Block) == j.ToHouse(HouseType.Block))
					{
						rating++;
					}
				}

				if (maxRating < rating)
				{
					(maxRating, best) = (rating, i);
				}
			}

			(_, result[index]) = (isOrdered.Add(best), best);
		}

		return result;
	}
}
