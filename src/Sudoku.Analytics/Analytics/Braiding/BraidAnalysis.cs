namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Provides an entry to analyze braiding of a chute in a pattern.
/// </summary>
public static class BraidAnalysis
{
	/// <summary>
	/// Indicates top-3 cells defined in the specified chute, sequence index and type.
	/// </summary>
	private static readonly CellMap[] TopThreeCellsMap;

	/// <summary>
	/// Represents the map of all rotation patterns, grouped by sequence index (0..3) and type.
	/// </summary>
	private static readonly FrozenDictionary<Strand, ChuteStrandMap> StrandsMap;


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static BraidAnalysis()
	{
		var strandsMap = new Dictionary<Strand, ChuteStrandMap>();
		TopThreeCellsMap = new CellMap[Chute.MaxChuteIndex * 3];

		// Iterate on each chute.
		foreach (var (chuteIndex, _, housesMask) in Chute.Chutes)
		{
			// Get three houses of the chute.
			var house1 = BitOperations.TrailingZeroCount(housesMask);
			var house2 = housesMask.GetNextSet(house1);
			var house3 = housesMask.GetNextSet(house2);

			// Starts with the specified segment.
			for (var sequenceIndex = 0; sequenceIndex < 3; sequenceIndex++)
			{
				// Try to get the first 3 cells from the top-left segment.
				ref readonly var cellsFromHouse1 = ref HousesMap[sequenceIndex switch { 0 => house1, 1 => house2, _ => house3 }];
				var cells1 = cellsFromHouse1[..3];
				var otherCells1 = cellsFromHouse1 & ~cells1;
				var globalIndex = ProjectGlobalIndex(chuteIndex, sequenceIndex);
				TopThreeCellsMap[globalIndex] = cells1;

				// Then do rotate-shifting with N or Z mode.
				foreach (var mode in (StrandType.Downside, StrandType.Upside))
				{
					// Get the second segment.
					ref readonly var cellsFromHouse2 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, StrandType.Downside) => house2,
							(0, _) => house3,
							(1, StrandType.Downside) => house3,
							(1, _) => house1,
							(2, StrandType.Downside) => house1,
							_ => house2
						}
					];
					var cells2 = cellsFromHouse2[3..6];
					var otherCells2 = cellsFromHouse2 & ~cells2;

					// Get the third segment.
					ref readonly var cellsFromHouse3 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, StrandType.Downside) => house3,
							(0, _) => house2,
							(1, StrandType.Downside) => house1,
							(1, _) => house3,
							(2, StrandType.Downside) => house2,
							_ => house1
						}
					];
					var cells3 = cellsFromHouse3[6..];
					var otherCells3 = cellsFromHouse3 & ~cells3;

					// Merge them up.
					var otherCellsFromChute = otherCells1 | otherCells2 | otherCells3;

					// Add value into the dictionary.
					strandsMap.Add(new(chuteIndex, sequenceIndex, mode), new([cells1, cells2, cells3], otherCellsFromChute));
				}
			}
		}

		StrandsMap = strandsMap.ToFrozenDictionary();
	}


	/// <summary>
	/// Projects global index from chute index (0..6) and sequence index (0..3).
	/// </summary>
	/// <param name="chuteIndex">The chute index.</param>
	/// <param name="sequenceIndex">The sequence index.</param>
	/// <returns>The global index.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ProjectGlobalIndex(int chuteIndex, int sequenceIndex)
		=> (chuteIndex / 3 * 3 + chuteIndex % 3) * 3 + sequenceIndex;

	/// <summary>
	/// Get cells at the specified chute, sequence index and type.
	/// </summary>
	/// <param name="chuteIndex">The chute index (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <param name="type">The type.</param>
	/// <returns>The map of the strand.</returns>
	public static ref readonly ChuteStrandMap GetCellsAt(int chuteIndex, Digit sequenceIndex, StrandType type)
		=> ref GetCellsAt(new(chuteIndex, sequenceIndex, type));

	/// <summary>
	/// Get cells at the specified strand.
	/// </summary>
	/// <param name="strand">The strand.</param>
	/// <returns>The map of the strand.</returns>
	public static ref readonly ChuteStrandMap GetCellsAt(Strand strand) => ref StrandsMap[strand];

	/// <summary>
	/// Gets the pattern type of three digits in the specified chute.
	/// </summary>
	/// <param name="solutionGrid">The solution to a certain grid.</param>
	/// <param name="chuteIndex">The chute (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <returns>The first three digits from the segment, specified as <paramref name="sequenceIndex"/>.</returns>
	/// <exception cref="ArgumentException">Throws when the argument must be solved.</exception>
	public static BraidingType GetPattern(in Grid solutionGrid, int chuteIndex, int sequenceIndex)
	{
		ArgumentException.Assert(solutionGrid.IsSolved);

		var globalIndex = ProjectGlobalIndex(chuteIndex, sequenceIndex);
		ref readonly var topThreeCells = ref TopThreeCellsMap[globalIndex];
		var valuesMap = solutionGrid.ValuesMap;

		var result = new List<StrandType>(3);

		// Iterate on each cell.
		foreach (var cell in topThreeCells)
		{
			var digit = solutionGrid.GetDigit(cell);

			// Check for two types of rotation.
			foreach (var type in (StrandType.Downside, StrandType.Upside))
			{
				var strand = new Strand(chuteIndex, sequenceIndex, type);
				ref readonly var cells = ref StrandsMap[strand].Included;
				if ((valuesMap[digit] & cells).Count == 3)
				{
					// Valid.
					result.Add(type);
					break;
				}
			}
		}
		return BraidingType.Create(result[0], result[1], result[2]);
	}

	/// <summary>
	/// Maps all digits in the specified grid that can be categorized as N or Z mode in the specified chute.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="chuteIndex">The chute index (0..6).</param>
	/// <returns>A dictionary of strands and the digits that can be categorized as this strand.</returns>
	public static IReadOnlyDictionary<Strand, Mask> MapStrands(in Grid grid, int chuteIndex)
	{
		var result = new Dictionary<Strand, Mask>();
		MapStrandsCore(grid, chuteIndex, result);
		return result;
	}

	/// <summary>
	/// Maps all digits in the specified grid that can be categorized as N or Z mode.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>A dictionary of strands and the digits that can be categorized as this strand.</returns>
	public static IReadOnlyDictionary<Strand, Mask> MapStrands(in Grid grid)
	{
		var result = new Dictionary<Strand, Mask>();
		MapStrandsCore(grid, -1, result);
		return result;
	}

	/// <summary>
	/// Reduces the full grid of digit distribution on braid analysis.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="chuteMaskThatCanBeReduced">
	/// The result chute indices that can be reduced. The result value is a 6-bit mask indicating chute indices that can be reduced.
	/// </param>
	/// <param name="lookupThatCanReducedTo">The reduced dictionary (if can).</param>
	/// <returns>A <see cref="bool"/> result indicating whether it can be reduced.</returns>
	/// <remarks>
	/// Usage:
	/// <code><![CDATA[
	/// var grid = Grid.Parse("<puzzle-string-here>");
	/// if (BraidAnalysis.TryReduce(grid, out var reducedChutesMask, out var reducedLookup))
	/// {
	///     var chuteStrings = from chute in reducedChutesMask.AllSets select (chute + 1).ToString();
	///     var reducedChuteIndicesString = string.Join(", ", chuteStrings);
	///     Console.WriteLine($"Reduced chute indices (0..6): [{reducedChuteIndicesString}]");
	///     foreach (var (strand, (type, mask)) in reducedLookup)
	///     {
	///         var typeString = type.ToSimpleString();
	///         var maskString = string.Join(", ", from digit in mask.AllSets select $"{digit + 1}");
	///         Console.WriteLine($"{strand} ({typeString}) -> [{maskString}]");
	///     }
	/// }
	/// ]]></code>
	/// </remarks>
	public static bool TryReduce(
		in Grid grid,
		out int chuteMaskThatCanBeReduced,
		out IReadOnlyDictionary<Strand, (BraidingType Type, Mask Mask)> lookupThatCanReducedTo
	)
	{
		var originalMappedStrands = MapStrands(grid);
		var tempReducedLookup = new Dictionary<Strand, (BraidingType, Mask)>();
		chuteMaskThatCanBeReduced = 0;
		for (var chute = 0; chute < 6; chute++)
		{
			if (TryInferType(grid, chute, out var braidingType, out var reduced)
				|| !((Dictionary<Strand, Mask>)MapStrands(grid, chute)).DictionaryEquals(reduced))
			{
				foreach (var kvp in reduced)
				{
					tempReducedLookup.Add(kvp.Key, (braidingType, kvp.Value));
				}
				chuteMaskThatCanBeReduced |= 1 << chute;
			}
			else
			{
				for (var sequenceIndex = 0; sequenceIndex < 3; sequenceIndex++)
				{
					foreach (var type in (StrandType.Downside, StrandType.Upside))
					{
						var strand = new Strand(chute, sequenceIndex, type);
						tempReducedLookup.Add(strand, (BraidingType.Unknown, originalMappedStrands[strand]));
					}
				}
			}
		}

		lookupThatCanReducedTo = tempReducedLookup;
		return chuteMaskThatCanBeReduced != 0;
	}

	/// <summary>
	/// Try to infer braiding type of the specified chute.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="chuteIndex">The chute index (0..6).</param>
	/// <param name="result">The type inferred. If none found, <see cref="BraidingType.Unknown"/> will be returned.</param>
	/// <param name="resultLookup">The result distribution of digits must be appeared in the specified strands.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the type can be inferred with unique value.</returns>
	/// <seealso cref="BraidingType.Unknown"/>
	public static bool TryInferType(
		in Grid grid,
		int chuteIndex,
		out BraidingType result,
		out IReadOnlyDictionary<Strand, Mask> resultLookup
	)
	{
		result = BraidingType.Unknown;

		// Define a result dictionary and initialize it with original values.
		var resultDictionary = new Dictionary<Strand, Mask>(MapStrands(grid, chuteIndex));
		var candidateBraidingTypes = BraidingType.NRope | BraidingType.NBraid | BraidingType.ZBraid | BraidingType.ZRope;
		var previousResultDictionary = default(Dictionary<Strand, Mask>);
		while (true)
		{
			// Refresh result dictionary via inferred braiding type.
			if (candidateBraidingTypes.IsFlag)
			{
				var (nCount, zCount) = (candidateBraidingTypes.NCount, candidateBraidingTypes.ZCount);
				bool localHasAnyChanges;
				do
				{
					localHasAnyChanges = false;

					// Handle naked rule.
					foreach (var strand in resultDictionary.Keys)
					{
						var (_, _, strandType) = strand;
						var mask = resultDictionary[strand];
						if (strandType == StrandType.Downside && BitOperations.PopCount((uint)mask) == nCount
							|| strandType == StrandType.Upside && BitOperations.PopCount((uint)mask) == zCount)
						{
							// Eliminates other occurrences of such digits.
							foreach (var otherStrand in resultDictionary.Keys)
							{
								if (strand != otherStrand)
								{
									// Clears digits appeared in 'strand'.
									var original = resultDictionary[otherStrand];
									localHasAnyChanges = localHasAnyChanges || original != (resultDictionary[otherStrand] &= (Mask)~mask);
								}
							}
						}
					}
				} while (localHasAnyChanges);
			}

			// Find hidden rules appeared in either N part or Z part.
			var hiddenDictionary = new Dictionary<Strand, Mask>();

			// Now find for hidden rules.
			for (var digit = 0; digit < 9; digit++)
			{
				var lastAppearedStrand = default(Strand?);
				foreach (var strand in resultDictionary.Keys)
				{
					// Check whether this strand contains such digit or not.
					if ((resultDictionary[strand] >> digit & 1) != 0)
					{
						if (lastAppearedStrand is not null)
						{
							// The digit can be appeared in at least 2 strands.
							// We cannot determine which one is correct.
							goto NextDigit;
						}

						// Otherwise, assign it into temporary variable.
						lastAppearedStrand = strand;
					}
				}

				// If here, we know that the digit can only be appeared in one strand.
				if (lastAppearedStrand is not { } onlyStrand)
				{
					throw new InvalidOperationException("Why here?!");
				}

				// Add it into dictionary as "hidden single" rule.
				var mask = (Mask)(1 << digit);
				if (!hiddenDictionary.TryAdd(onlyStrand, mask))
				{
					hiddenDictionary[onlyStrand] |= mask;
				}

			NextDigit:
				;
			}

			// Infer braiding types.
			// Check each strand in 'tempDictionary' to know whether any conclusions here.
			foreach (var ((_, _, strandType), mask) in hiddenDictionary)
			{
				switch (BitOperations.PopCount((uint)mask))
				{
					// Must be NNN or ZZZ.
					case 3:
					{
						candidateBraidingTypes = strandType == StrandType.Downside ? BraidingType.NRope : BraidingType.ZRope;
						break;
					}

					// N mode with popcount == 2 <=> Not NZZ.
					// Z mode with popcount == 2 <=> Not NNZ.
					case 2:
					{
						candidateBraidingTypes &= ~(strandType == StrandType.Downside ? BraidingType.ZBraid : BraidingType.NBraid);
						goto case 1;
					}

					// N mode with popcount >= 0 <=> Not ZZZ.
					// Z mode with popcount >= 0 <=> Not NNN.
					case 1:
					{
						candidateBraidingTypes &= ~(strandType == StrandType.Downside ? BraidingType.ZRope : BraidingType.NRope);
						break;
					}
				}
			}

			// Handle hidden rule.
			if (candidateBraidingTypes.IsFlag)
			{
				var (nCount, zCount) = (candidateBraidingTypes.NCount, candidateBraidingTypes.ZCount);
				foreach (var (strand, mask) in hiddenDictionary)
				{
					// Add an extra check here: we can safely eliminate other digits
					// if and only if the number of N's and Z's digits have already been inferred.
					// For example, if a chute has the following digit distribution:
					//   * T4N (NNZ) -> [5, 8]
					//   * T4Z (NNZ) -> [2, 3]
					//   * T5N (NNZ) -> [7, 9]
					//   * T5Z (NNZ) -> [4]
					//   * T6N (NNZ) -> [1, 2, 3]
					//   * T6Z (NNZ) -> [6]
					// In T6N, digits 2 and 3 in [1, 2, 3] cannot be eliminated
					// because we cannot keep the second digit appeared in strand T6N because of NNZ having been inferred,
					// meaning there're 2 N's digits.
					if (BitOperations.PopCount((uint)mask) == (strand.Type == StrandType.Downside ? nCount : zCount))
					{
						// Clears all the other digits appeared in the current strand.
						resultDictionary[strand] &= mask;
					}
				}
			}

			// Compare equality of result dictionary.
			if (resultDictionary.DictionaryEquals(previousResultDictionary))
			{
				break;
			}

			// Replace variable.
			previousResultDictionary = [with(resultDictionary)];
		}

		// Get values and return.
		result = candidateBraidingTypes.IsFlag ? candidateBraidingTypes : BraidingType.Unknown;
		resultLookup = resultDictionary;
		return result != BraidingType.Unknown;
	}

	/// <summary>
	/// The core method of mapping strands.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="chuteIndex">The chute index (0..6). -1 for all chutes checking.</param>
	/// <param name="value">The value.</param>
	private static void MapStrandsCore(in Grid grid, int chuteIndex, Dictionary<Strand, Mask> value)
	{
		var digitsMap = grid.DigitsMap;
		foreach (ref readonly var strand in Strand.Strands)
		{
			var ((currentChuteIndex, sequenceIndex, _), mask) = (strand, (Mask)0);
			if (chuteIndex != -1 && currentChuteIndex != chuteIndex)
			{
				continue;
			}

			var includedSegments = StrandsMap[strand].IncludedSegments;

			// Iterate on each digit appeared in this group of cells.
			var globalIndex = ProjectGlobalIndex(currentChuteIndex, sequenceIndex);
			foreach (var digit in grid[TopThreeCellsMap[globalIndex], true])
			{
				var allSegmentsSatisfied = true;
				foreach (ref readonly var segmentCells in includedSegments)
				{
					if (!(digitsMap[digit] & segmentCells))
					{
						allSegmentsSatisfied = false;
						break;
					}
				}
				if (allSegmentsSatisfied)
				{
					mask |= (Mask)(1 << digit);
				}
			}

			// Add the target mask into dictionary.
			value.Add(strand, mask);
		}
	}


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The dictionary.</param>
	extension(Dictionary<Strand, Mask> @this)
	{
		/// <summary>
		/// Compare two <see cref="Dictionary{TKey, TValue}"/> instances.
		/// </summary>
		/// <typeparam name="TDictionary">The type of dictionary to compare.</typeparam>
		/// <param name="other">The other instance to be compared.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		public bool DictionaryEquals<TDictionary>(TDictionary? other) where TDictionary : IReadOnlyDictionary<Strand, Mask>
		{
			if (other is null)
			{
				return false;
			}
			if (@this.Count != other.Count)
			{
				return false;
			}
			foreach (var key in other.Keys)
			{
				if (@this[key] != (other.TryGetValue(key, out var m) ? m : -1))
				{
					return false;
				}
			}
			return true;
		}
	}
}
