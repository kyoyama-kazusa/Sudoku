namespace Sudoku.Behaviors.Ittoryu;

/// <summary>
/// Represents a disordered ittoryu path finder. This finder will find a digit sequence order that makes the puzzle be an ittoryu.
/// This finder only uses single techniques (Hidden Singles and Naked Singles) to solve a puzzle;
/// complex singles won't be supported for now.
/// </summary>
/// <param name="supportedTechniques">Indicates the supported techniques. By default, all singles are included.</param>
public sealed partial class DisorderedIttoryuFinder([Property] params TechniqueSet supportedTechniques)
{
	/// <summary>
	/// Initializes a <see cref="DisorderedIttoryuFinder"/> instance.
	/// </summary>
	public DisorderedIttoryuFinder() : this(TechniqueIttoryuSets.IttoryuTechniques)
	{
	}

	/// <summary>
	/// Initializes a <see cref="DisorderedIttoryuFinder"/> instance via the specified list of techniques.
	/// </summary>
	/// <param name="techniques">A list of techniques.</param>
	public DisorderedIttoryuFinder(IEnumerable<Technique> techniques) : this([.. techniques])
	{
	}


	/// <summary>
	/// Find a suitable ittoryu path.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>
	/// The target digit path. If none found, a longest path will be returned.
	/// </returns>
	public DisorderedIttoryuDigitPath FindPath(in Grid grid, CancellationToken cancellationToken = default)
	{
		var digitsStack = new Stack<Digit>();
		try
		{
			for (var digit = 0; digit < 9; digit++)
			{
				dfs(grid, digit, digitsStack, [], 0, true);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (DisorderedIttoryuModuleAlreadyFinishedException)
		{
			return [.. digitsStack.Reverse()];
		}
		return null;


		void dfs(
			Grid grid,
			Digit digit,
			Stack<Digit> digitsStack,
			ReadOnlySpan<IttoryuPathNode> foundNodes,
			Mask finishedDigits,
			bool skipApplying = false
		)
		{
			if (skipApplying)
			{
				goto StartCheckingWithoutApplying;
			}

			if (foundNodes.Length == 0)
			{
				// No available steps can be applied.
				return;
			}

			// Apply all digits for the currently-found nodes.
			foreach (var node in foundNodes)
			{
				if (grid.GetState(node.Cell) == CellState.Empty)
				{
					grid.SetDigit(node.Cell, node.Digit);
				}
			}

		StartCheckingWithoutApplying:
			if (grid.ValuesMap[digit].Count != 9)
			{
				// If the current digit is not completed, we should continue searching for this digit.
				var tempNodes = new List<IttoryuPathNode>(16);
				fullHouses(grid, tempNodes, digit);
				hiddenSingles(grid, tempNodes, digit);
				nakedSingles(grid, tempNodes, digit);

				dfs(grid, digit, digitsStack, [.. tempNodes], finishedDigits);
			}
			else
			{
				// If the current digit is completed, we should continue searching for the next digit.
				digitsStack.Push(digit);
				finishedDigits |= (Mask)(1 << digit);

				// Here we should check the bit mask. If currently we have found the digits are finished,
				// the last works are not necessary, just throw an exception to escape here.
				if (finishedDigits == Grid.MaxCandidatesMask)
				{
					// Just find one.
					throw new DisorderedIttoryuModuleAlreadyFinishedException();
				}

				// If not, we should search for available path nodes again, and iterate on them.
				var tempNodes = new List<IttoryuPathNode>(16);
				foreach (var anotherDigit in (Mask)(Grid.MaxCandidatesMask & ~finishedDigits))
				{
					fullHouses(grid, tempNodes, anotherDigit);
					hiddenSingles(grid, tempNodes, anotherDigit);
					nakedSingles(grid, tempNodes, anotherDigit);
				}

				// Iterate on found path nodes.
				foreach (var anotherDigit in Mask.Create(from node in tempNodes select node.Digit))
				{
					dfs(
						grid,
						anotherDigit,
						digitsStack,
						from node in tempNodes where node.Digit == anotherDigit select node,
						finishedDigits
					);
				}

				// If all available found path nodes cannot make the path complete, pop it.
				digitsStack.Pop();
			}

			cancellationToken.ThrowIfCancellationRequested();
		}

		void fullHouses(in Grid grid, List<IttoryuPathNode> foundNodes, Digit digit)
		{
			if (!SupportedTechniques.Contains(Technique.FullHouse))
			{
				return;
			}

			var emptyCells = grid.EmptyCells;
			for (var house = 0; house < 27; house++)
			{
				if ((emptyCells & HousesMap[house]) is [var fullHouseCell]
					&& BitOperations.TrailingZeroCount(grid.GetCandidates(fullHouseCell)) == digit)
				{
					foundNodes.Add(new(grid, house, fullHouseCell * 9 + digit));
				}
			}
		}

		void hiddenSingles(in Grid grid, List<IttoryuPathNode> foundNodes, Digit digit)
		{
			var candidatesMap = grid.CandidatesMap;
			for (var house = 0; house < 27; house++)
			{
				var houseCode = house.HouseType switch
				{
					HouseType.Block => Technique.HiddenSingleBlock,
					HouseType.Row => Technique.HiddenSingleRow,
					_ => Technique.HiddenSingleColumn
				};
				if (!SupportedTechniques.Contains(houseCode))
				{
					continue;
				}

				if ((HousesMap[house] & candidatesMap[digit]) / house is var mask && BitOperations.IsPow2(mask))
				{
					foundNodes.Add(new(grid, house, HousesCells[house][BitOperations.Log2(mask)] * 9 + digit));
				}
			}
		}

		void nakedSingles(in Grid grid, List<IttoryuPathNode> foundNodes, Digit digit)
		{
			if (!SupportedTechniques.Contains(Technique.NakedSingle))
			{
				return;
			}

			foreach (var cell in grid.EmptyCells)
			{
				if (grid.GetCandidates(cell) == 1 << digit)
				{
					foundNodes.Add(new(grid, -1, cell * 9 + digit));
				}
			}
		}
	}
}
