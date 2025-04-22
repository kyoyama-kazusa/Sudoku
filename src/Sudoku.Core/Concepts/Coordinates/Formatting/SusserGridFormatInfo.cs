namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Represents a <see cref="GridFormatInfo{TGrid}"/> type that supports Susser formatting.
/// </summary>
/// <typeparam name="TGrid">The type of grid.</typeparam>
public sealed partial class SusserGridFormatInfo<TGrid> : GridFormatInfo<TGrid> where TGrid : unmanaged, IGrid<TGrid>
{
	/// <summary>
	/// Indicates the modifiable prefix character.
	/// </summary>
	private const char ModifiablePrefix = '+';

	/// <summary>
	/// Indicates the line separator character used by shortening Susser format.
	/// </summary>
	private const char LineLimit = ',';

	/// <summary>
	/// Indicates the star character used by shortening Susser format.
	/// </summary>
	private const char Star = '*';

	/// <summary>
	/// Indicates the dot character.
	/// </summary>
	private const char Dot = '.';

	/// <summary>
	/// Indicates the zero character.
	/// </summary>
	private const char Zero = '0';

	/// <summary>
	/// Indicates the pre-elimination prefix character.
	/// </summary>
	private const char PreeliminationPrefix = ':';


	[GeneratedRegex("""[\d\.\+]{80,}(\:(\d{3}\s+)*\d{3})?""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridSusserPattern { get; }

	[GeneratedRegex("""[\d\.\*]{1,9}(,[\d\.\*]{1,9}){8}""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridShortenedSusserPattern { get; }

	[GeneratedRegex("""(?<=\:)(\d{3}\s+)*\d{3}""", RegexOptions.Compiled, 5000)]
	internal static partial Regex EliminationPattern { get; }


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override IFormatProvider? GetFormat(Type? formatType) => formatType == typeof(GridFormatInfo<TGrid>) ? this : null;

	/// <inheritdoc/>
	public override SusserGridFormatInfo<TGrid> Clone()
		=> new()
		{
			WithCandidates = WithCandidates,
			WithModifiables = WithModifiables,
			ShortenSusser = ShortenSusser,
			NegateEliminationsTripletRule = NegateEliminationsTripletRule,
			Placeholder = Placeholder,
			TreatValueAsGiven = TreatValueAsGiven,
			OnlyEliminations = OnlyEliminations
		};

	/// <inheritdoc/>
	protected internal override string FormatCore(in TGrid grid)
	{
		return b(grid) is var r && IsCompatibleMode
			? $":0000:x:{r}{new(':', 3)}"
			: OnlyEliminations
				? EliminationPattern.Match(r) is { Success: true, Value: var value } ? value : string.Empty
				: r;


		string b(in TGrid grid)
		{
			var thisCopied = Clone();
			thisCopied.WithCandidates = false;

			var sb = new StringBuilder(162);
			var originalGrid = this switch
			{
				{ WithCandidates: true, ShortenSusser: false } => TGrid.Parse(thisCopied.FormatCore(grid)),
				_ => TGrid.Undefined
			};

			var eliminatedCandidates = CandidateMap.Empty;
			for (var c = 0; c < 81; c++)
			{
				var state = grid.GetState(c);
				if (state == CellState.Empty && !originalGrid.IsUndefined && WithCandidates)
				{
					// Check if the value has been set 'true' and the value has already deleted at the grid
					// with only givens and modifiables.
					foreach (var i in (Mask)(originalGrid[c] & Grid.MaxCandidatesMask))
					{
						if (!grid.GetExistence(c, i))
						{
							// The value is 'false', which means the digit has already been deleted.
							eliminatedCandidates.Add(c * 9 + i);
						}
					}
				}

				switch (state)
				{
					case CellState.Empty:
					{
						sb.Append(Placeholder);
						break;
					}
					case CellState.Modifiable:
					{
						switch (this)
						{
							case { WithModifiables: true, ShortenSusser: false }:
							{
								sb.Append(ModifiablePrefix);
								sb.Append(grid.GetDigit(c) + 1);
								break;
							}
							case { Placeholder: var p }:
							{
								sb.Append(p);
								break;
							}
						}
						break;
					}
					case CellState.Given:
					{
						sb.Append(grid.GetDigit(c) + 1);
						break;
					}
					default:
					{
						throw new InvalidOperationException(SR.ExceptionMessage("InvalidStateOnParsing"));
					}
				}
			}

			var elimsStr = (
				grid is MarkerGrid markerGrid
					? markedCandidates(markerGrid)
					: NegateEliminationsTripletRule ? eliminatedCandidates : negateElims(grid, eliminatedCandidates)
			).ToString(new HodokuTripletCandidateMapFormatInfo());
			var @base = sb.ToString();
			var final = ShortenSusser
				? shorten(@base, Placeholder)
				: $"{@base}{(string.IsNullOrEmpty(elimsStr) ? string.Empty : $"{PreeliminationPrefix}{elimsStr}")}";
			return TreatValueAsGiven ? final.RemoveAll('+') : final;
		}

		static CandidateMap negateElims(in TGrid grid, in CandidateMap eliminatedCandidates)
		{
			var eliminatedCandidatesCellDistribution = eliminatedCandidates.CellDistribution;
			var result = CandidateMap.Empty;
			foreach (var cell in grid.EmptyCells)
			{
				if (eliminatedCandidatesCellDistribution.TryGetValue(cell, out var digitsMask))
				{
					foreach (var digit in digitsMask)
					{
						result.Add(cell * 9 + digit);
					}
				}
			}
			return result;
		}

		static CandidateMap markedCandidates(in MarkerGrid grid)
		{
			var result = CandidateMap.Empty;
			for (var cell = 0; cell < 81; cell++)
			{
				if (grid.GetState(cell) == CellState.Empty)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						result.Add(cell * 9 + digit);
					}
				}
			}
			return result;
		}

		static string shorten(string @base, char placeholder)
		{
			// lang = regex
			var placeholderPattern = placeholder == Dot ? """\.+""" : """0+""";
			var resultSpan = (stackalloc char[81]);
			var spanIndex = 0;
			for (var i = 0; i < 9; i++)
			{
				var characterIndexStart = i * 9;
				var sliced = @base[characterIndexStart..(characterIndexStart + 9)];
				switch (Regex.Matches(sliced, placeholderPattern))
				{
					case []:
					{
						// Can't find any simplifications.
						Unsafe.CopyBlock(
							ref @ref.ByteRef(ref resultSpan[characterIndexStart]),
							in @ref.ReadOnlyByteRef(in sliced.Span[0]),
							sizeof(char) * 9
						);
						spanIndex += 9;
						break;
					}
					case var collection:
					{
						var hashSet = new HashSet<Match>(
							collection,
							EqualityComparer<Match>.Create(static (l, r) => (l?.Length ?? 0) == (r?.Length ?? 0), static v => v.Length)
						);
						switch (hashSet)
						{
							case { Count: 1 } set when set.First() is { Length: var firstLength }:
							{
								// All matches are same-length.
								for (var j = 0; j < 9;)
								{
									if (sliced[j] == placeholder)
									{
										resultSpan[spanIndex++] = Star;
										j += firstLength;
									}
									else
									{
										resultSpan[spanIndex++] = sliced[j];
										j++;
									}
								}
								break;
							}
							case var set:
							{
								var match = set.MaxBy(static m => m.Length)!.Value;
								var pos = sliced.IndexOf(match);
								for (var j = 0; j < 9; j++)
								{
									if (j == pos)
									{
										resultSpan[spanIndex++] = Star;
										j += match.Length;
									}
									else
									{
										resultSpan[spanIndex++] = sliced[j];
										j++;
									}
								}
								break;
							}
						}
						break;
					}
				}

				if (i != 8)
				{
					resultSpan[spanIndex++] = LineLimit;
				}
			}

			return resultSpan[..spanIndex].ToString();
		}
	}

	/// <inheritdoc/>
	protected internal override TGrid ParseCore(string str)
	{
		if (IsCompatibleMode)
		{
			return TGrid.Undefined;
		}

		var match = (ShortenSusser ? GridShortenedSusserPattern : GridSusserPattern).Match(str).Value;
		if (ShortenSusser && (match is not { Length: <= 81 } || !expandCode(match, out match)))
		{
			return TGrid.Undefined;
		}

		// Step 1: fills all digits.
		var (result, i) = (TGrid.Empty, 0);
		if (match.Length is not (var length and not 0))
		{
			return TGrid.Undefined;
		}

		for (var realPos = 0; i < length && match[i] != ':'; realPos++)
		{
			switch (match[i])
			{
				case '+':
				{
					// Plus sign means the character after it is a digit,
					// which is modifiable value in the grid in its corresponding position.
					if (i < length - 1)
					{
						if (match[i + 1] is var nextChar and >= '1' and <= '9')
						{
							// Set value.
							result.SetDigit(realPos, nextChar - '1');

							// Add 2 on iteration variable to skip 2 characters
							// (A plus sign '+' and a digit).
							i += 2;
						}
						else
						{
							// Why isn't the character a digit character?
							return TGrid.Undefined;
						}
					}
					else
					{
						return TGrid.Undefined;
					}

					break;
				}
				case '.' or '0':
				{
					// A placeholder.
					if (typeof(TGrid) == typeof(MarkerGrid))
					{
						result.SetState(realPos, CellState.Empty);
					}

					// Do nothing but only move 1 step forward.
					i++;

					break;
				}
				case var c and >= '1' and <= '9':
				{
					// Is a digit character.
					// Digits are representing given values in the grid.
					// Not the plus sign, but a placeholder '0' or '.'.
					// Set value.
					result.SetDigit(realPos, c - '1');

					// Set the cell state as 'CellState.Given'.
					// If the code below doesn't make sense to you,
					// you can see the comments in method 'OnParsingSusser(string)'
					// to know the meaning also.
					result.SetState(realPos, CellState.Given);

					// Finally moves 1 step forward.
					i++;

					break;
				}
				default:
				{
					// Other invalid characters. Throws an exception.
					//throw Throwing.ParsingError<Grid>(nameof(ParsingValue));
					return TGrid.Undefined;
				}
			}
		}

		// Step 2: eliminates candidates if exist.
		// If we have met the colon sign ':', this loop would not be executed.
		if (EliminationPattern.Match(match) is { Success: true, Value: var elimMatch })
		{
			var candidates = CandidateMap.Parse(elimMatch, new HodokuTripletCandidateMapFormatInfo());

			if (typeof(TGrid) == typeof(MarkerGrid))
			{
				// If the grid is 'MarkerGrid', we should adjust the logic to match candidates and append into target instance.
				ref var instance = ref Unsafe.As<TGrid, MarkerGrid>(ref result);
				foreach (var candidate in candidates)
				{
					instance.AddCandidates(candidate / 9, candidate % 9);
				}
				goto ReturnResult;
			}

			if (!NegateEliminationsTripletRule)
			{
				// This applies for normal rule - removing candidates marked.
				foreach (var candidate in candidates)
				{
					// Set the candidate with false to eliminate the candidate.
					result.SetExistence(candidate / 9, candidate % 9, false);
				}
			}
			else
			{
				// If negate candidates, we should remove all possible candidates from all empty cells, making the grid invalid firstly.
				// Then we should add candidates onto the grid to make the grid valid.
				var distribution = candidates.CellDistribution;
				for (var cell = 0; cell < 81; cell++)
				{
					ref var mask = ref result[cell];
					if (MaskOperations.MaskToCellState(mask) == CellState.Empty)
					{
						mask = distribution.TryGetValue(cell, out var digitsMask)
							? (Mask)((Mask)((Mask)(mask >> 9 & 7) << 9) | digitsMask)
							: Grid.EmptyMask;
					}
				}
			}
		}

	ReturnResult:
		return result;


		static bool expandCode(string? original, [NotNullWhen(true)] out string? result)
		{
			// We must the string code holds 8 ','s and is with no ':' or '+'.
			if (original is null || original.Contains(':') || original.Contains('+') || original.Span.Count(',') != 8)
			{
				result = null;
				return false;
			}

			var lines = original.Split(',');
			if (lines.Length != 9)
			{
				result = null;
				return false;
			}

			// Check per line, and expand it.
			var resultSpan = (stackalloc char[81]);
			var placeholder = original.Contains('0') ? '0' : '.';
			for (var i = 0; i < 9; i++)
			{
				var line = lines[i];
				switch (line.Span.Count('*'))
				{
					case 1 when (9 + 1 - line.Length, 0, 0) is var (empties, j, k):
					{
						foreach (var c in line)
						{
							if (c == '*')
							{
								resultSpan.Slice(i * 9 + k, empties).Fill(placeholder);

								j++;
								k += empties;
							}
							else
							{
								resultSpan[i * 9 + k] = line[j];

								j++;
								k++;
							}
						}

						break;
					}

					case var n when (9 + n - line.Length, 0, 0) is var (empties, j, k):
					{
						var emptiesPerStar = empties / n;
						foreach (var c in line)
						{
							if (c == '*')
							{
								resultSpan.Slice(i * 9 + k, emptiesPerStar).Fill(placeholder);

								j++;
								k += emptiesPerStar;
							}
							else
							{
								resultSpan[i * 9 + k] = line[j];

								j++;
								k++;
							}
						}

						break;
					}
				}
			}

			result = resultSpan.ToString();
			return true;
		}
	}
}
