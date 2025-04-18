namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Represents a <see cref="GridFormatInfo{TGrid}"/> type that supports pencilmark grid formatting.
/// </summary>
public sealed partial class PencilmarkGridFormatInfo : GridFormatInfo<Grid>
{
	[GeneratedRegex("""(\<\d\>|\*\d\*|\d*[\+\-]?\d+)""", RegexOptions.Compiled)]
	public static partial Regex GridPencilmarkPattern { get; }

	[GeneratedRegex("""[1-9\+\-]{1,9}""", RegexOptions.Compiled)]
	private static partial Regex ValueCellPattern { get; }


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override IFormatProvider? GetFormat(Type? formatType) => formatType == typeof(GridFormatInfo<Grid>) ? this : null;

	/// <inheritdoc/>
	public override PencilmarkGridFormatInfo Clone()
		=> new() { SubtleGridLines = SubtleGridLines, TreatValueAsGiven = TreatValueAsGiven, IsCompatibleMode = IsCompatibleMode };

	/// <inheritdoc/>
	protected internal override string FormatCore(in Grid grid)
	{
		// Step 1: gets the candidates information grouped by columns.
		var valuesByColumn = createTempDictionary();
		var valuesByRow = createTempDictionary();

		for (var i = 0; i < 81; i++)
		{
			var value = grid[i];
			valuesByRow[i / 9].Add(value);
			valuesByColumn[i % 9].Add(value);
		}

		// Step 2: gets the maximal number of candidates in a cell, used for aligning by columns.
		const int bufferLength = 9;
		var maxLengths = (stackalloc int[bufferLength]);
		maxLengths.Clear();

		foreach (var (i, _) in valuesByColumn)
		{
			ref var maxLength = ref maxLengths[i];

			// Iteration on row index.
			for (var j = 0; j < 9; j++)
			{
				// Gets the number of candidates.
				var candidatesCount = 0;
				var value = valuesByColumn[i][j];

				// Iteration on each candidate.
				// Counts the number of candidates.
				candidatesCount += BitOperations.PopCount(value);

				// Compares the values.
				var comparer = Math.Max(
					candidatesCount,
					MaskOperations.MaskToCellState(value) switch
					{
						// The output will be '<digit>' and consist of 3 characters.
						CellState.Given => Math.Max(candidatesCount, IsCompatibleMode ? 1 : 3),

						// The output will be '*digit*' and consist of 3 characters.
						CellState.Modifiable => Math.Max(candidatesCount, IsCompatibleMode ? 1 : 3),

						// Normal output: 'series' (at least 1 character).
						_ => candidatesCount
					}
				);
				if (comparer > maxLength)
				{
					maxLength = comparer;
				}
			}
		}

		// Step 3: outputs all characters.
		var sb = new StringBuilder();
		for (var i = 0; i < 13; i++)
		{
			switch (i)
			{
				case 0: // Print tabs of the first line.
				{
					if (SubtleGridLines)
					{
						printTabLines(sb, '.', '.', '-', maxLengths);
					}
					else
					{
						printTabLines(sb, '+', '+', '-', maxLengths);
					}
					break;
				}
				case 4 or 8: // Print tabs of mediate lines.
				{
					if (SubtleGridLines)
					{
						printTabLines(sb, ':', '+', '-', maxLengths);
					}
					else
					{
						printTabLines(sb, '+', '+', '-', maxLengths);
					}
					break;
				}
				case 12: // Print tabs of the foot line.
				{
					if (SubtleGridLines)
					{
						printTabLines(sb, '\'', '\'', '-', maxLengths);
					}
					else
					{
						printTabLines(sb, '+', '+', '-', maxLengths);
					}
					break;
				}
				default: // Print values and tabs.
				{
					defaultPrinting(sb, valuesByRow[(int)Math.Floor(3F * i / 4)], '|', '|', maxLengths);
					break;
				}
			}
		}

		// The last step: returns the value.
		sb.RemoveFrom(^Environment.NewLine.Length);
		return sb.ToString();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void defaultPrinting(StringBuilder sb, IList<Mask> valuesByRow, char c1, char c2, Span<int> maxLengths)
		{
			sb.Append(c1);
			printValues(sb, valuesByRow, 0, 2, maxLengths);
			sb.Append(c2);
			printValues(sb, valuesByRow, 3, 5, maxLengths);
			sb.Append(c2);
			printValues(sb, valuesByRow, 6, 8, maxLengths);
			sb.Append(c1);
			sb.AppendLine();
		}

		void printValues(StringBuilder sb, IList<Mask> valuesByRow, int start, int end, Span<int> maxLengths)
		{
			sb.Append(' ');
			for (var i = start; i <= end; i++)
			{
				// Get digit.
				var value = valuesByRow[i];
				var state = MaskOperations.MaskToCellState(value);

				value &= Grid.MaxCandidatesMask;
				var d = value == 0 ? -1 : (state != CellState.Empty ? Mask.TrailingZeroCount(value) : -1) + 1;
				var s = (state, TreatValueAsGiven, IsCompatibleMode) switch
				{
					(CellState.Given or CellState.Modifiable, _, true) => d.ToString(),
					(CellState.Given, _, _) or (CellState.Modifiable, true, _) => $"<{d}>",
					(CellState.Modifiable, false, _) => $"*{d}*",
					_ => appendingMask(value)
				};

				sb.Append(s.PadRight(maxLengths[i]));
				sb.Append(i != end ? "  " : " ");
			}


			static string appendingMask(Mask value)
			{
				var innerSb = new StringBuilder(9);
				foreach (var z in value)
				{
					innerSb.Append(z + 1);
				}
				return innerSb.ToString();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void printTabLines(StringBuilder sb, char c1, char c2, char fillingChar, Span<int> m)
			=> sb
				.Append(c1)
				.Append(string.Empty.PadRight(m[0] + m[1] + m[2] + 6, fillingChar))
				.Append(c2)
				.Append(string.Empty.PadRight(m[3] + m[4] + m[5] + 6, fillingChar))
				.Append(c2)
				.Append(string.Empty.PadRight(m[6] + m[7] + m[8] + 6, fillingChar)).Append(c1)
				.AppendLine();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Dictionary<Candidate, List<Mask>> createTempDictionary()
			=> new() { { 0, new() }, { 1, new() }, { 2, new() }, { 3, new() }, { 4, new() }, { 5, new() }, { 6, new() }, { 7, new() }, { 8, new() } };
	}

	/// <inheritdoc/>
	protected internal override Grid ParseCore(string str)
	{
		// Older regular expression pattern:
		if ((from m in GridPencilmarkPattern.Matches(str) select m.Value) is not { Length: 81 } matches)
		{
			return Grid.Undefined;
		}

		var result = Grid.Empty;
		for (var cell = 0; cell < 81; cell++)
		{
			if (matches[cell] is not { Length: var length and <= 9 } s)
			{
				// More than 9 characters.
				return Grid.Undefined;
			}

			if (s.Contains('<'))
			{
				// All values will be treated as normal characters: '<digit>', '*digit*' and 'candidates'.

				// Givens.
				if (length == 3)
				{
					if (s[1] is var c and >= '1' and <= '9')
					{
						result.SetDigit(cell, c - '1');
						result.SetState(cell, CellState.Given);
					}
					else
					{
						// Illegal characters found.
						return Grid.Undefined;
					}
				}
				else
				{
					// The length is not 3.
					return Grid.Undefined;
				}
			}
			else if (s.Contains('*'))
			{
				// Modifiables.
				if (length == 3)
				{
					if (s[1] is var c and >= '1' and <= '9')
					{
						result.SetDigit(cell, c - '1');
						result.SetState(cell, CellState.Modifiable);
					}
					else
					{
						// Illegal characters found.
						return Grid.Undefined;
					}
				}
				else
				{
					// The length is not 3.
					return Grid.Undefined;
				}
			}
			else if (ValueCellPattern.Match(s) is { Success: true, Value: var possibleStringMatched } && possibleStringMatched == s)
			{
				// Candidates.
				// Here don't need to check the length of the string, and also all characters are digit characters.
				var mask = (Mask)0;
				foreach (var c in s)
				{
					if (c is not ('+' or '-'))
					{
						mask |= (Mask)(1 << c - '1');
					}
				}

				if (mask == 0)
				{
					return Grid.Undefined;
				}

				if ((mask & mask - 1) == 0)
				{
					// Compatibility:
					// If the cell has only one candidate left, we should treat this as given also.
					// This may ignore Sukaku checking, which causes a bug in logic.
					result.SetDigit(cell, Mask.TrailingZeroCount(mask));
					result.SetState(cell, CellState.Given);
				}
				else
				{
					for (var digit = 0; digit < 9; digit++)
					{
						result.SetExistence(cell, digit, (mask >> digit & 1) != 0);
					}
				}
			}
			else
			{
				// All conditions can't match.
				return Grid.Undefined;
			}
		}

		return result;
	}
}
