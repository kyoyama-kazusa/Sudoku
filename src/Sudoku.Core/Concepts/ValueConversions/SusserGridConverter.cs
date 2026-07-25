namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents Susser grid converter.
/// </summary>
public abstract partial class SusserGridConverter : IGridConverter
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


	/// <summary>
	/// Creates a <see cref="SusserGridConverter"/> instance.
	/// </summary>
	public SusserGridConverter()
	{
	}

	/// <summary>
	/// Creates a <see cref="SusserGridConverter"/> instance via original one.
	/// </summary>
	/// <param name="original">The original one.</param>
	private SusserGridConverter(SusserGridConverter original)
	{
		IsCompatibleMode = original.IsCompatibleMode;
		WithCandidates = original.WithCandidates;
		WithModifiables = original.WithModifiables;
		NegateEliminationsTripletRule = original.NegateEliminationsTripletRule;
		Placeholder = original.Placeholder;
		TreatValueAsGiven = original.TreatValueAsGiven;
		OnlyEliminations = original.OnlyEliminations;
	}


	/// <inheritdoc/>
	public abstract int ParsingPriority { get; }

	/// <summary>
	/// Indicates whether the formatter will use compatible mode to output grid values.
	/// </summary>
	public bool IsCompatibleMode { get; init; }

	/// <summary>
	/// <para>Indicates whether the formatter will reserve candidates as pre-elimination.</para>
	/// <para>The default value is <see langword="false"/>.</para>
	/// </summary>
	public bool WithCandidates { get; init; }

	/// <summary>
	/// <para>
	/// Indicates whether the formatter will output and distinct modifiable and given digits.
	/// If so, the modifiable digits will be displayed as <c>+digit</c>, where <c>digit</c> will be replaced
	/// with the real digit number (from 1 to 9).
	/// </para>
	/// <para>The default value is <see langword="false"/>.</para>
	/// </summary>
	public bool WithModifiables { get; init; }

	/// <summary>
	/// <para>
	/// Indicates whether the parser will use shorten mode to parse a susser format grid.
	/// If the value is <see langword="true"/>, the parser will omit the continuous empty notation
	/// <c>.</c>s or <c>0</c>s to a <c>*</c>.
	/// </para>
	/// <para>
	/// This option will omit the continuous empty cells to a <c>*</c> in a single line. For example, the code
	/// <code><![CDATA[
	/// 080630040200085009090000081000300800000020000006001000970000030400850007010094050
	/// ]]></code>
	/// will be displayed as
	/// <code><![CDATA[
	/// 08063*40,2*85009,09*81,*300800,*2*,006001*,97*30,40085*7,01*94050
	/// ]]></code>
	/// via this option.
	/// We use the colon <c>,</c> to separate each line of 9 numbers, and then omit the most continuous empty cells to a <c>*</c>.
	/// </para>
	/// <para>The default value is <see langword="false"/>.</para>
	/// </summary>
	public abstract bool ShortenSusser { get; }

	/// <summary>
	/// <para>
	/// Indicates whether the parser will negate the rule, treating all digits as candidates existing in the grid
	/// instead of removed ones.
	/// </para>
	/// <para>The default value is <see langword="false"/>.</para>
	/// </summary>
	public bool NegateEliminationsTripletRule { get; init; }

	/// <summary>
	/// Indicates whether the formatter will treat all values as givens, regardless of its value state.
	/// </summary>
	public bool TreatValueAsGiven { get; init; }

	/// <summary>
	/// Indicates whether the formatting operation will output for eliminations.
	/// </summary>
	public bool OnlyEliminations { get; init; }

	/// <summary>
	/// Indicates the placeholder of the grid text formatter.
	/// </summary>
	/// <value>The new placeholder text character to be set. The value must be <c>'.'</c> or <c>'0'</c>.</value>
	/// <returns>The placeholder text.</returns>
	public char Placeholder { get; init; } = '.';


	[GeneratedRegex("""[\d\.\+]{80,}(\:(\d{3}\s+)*\d{3})?""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridSusserPattern { get; }

	[GeneratedRegex("""[\d\.\*]{1,9}(,[\d\.\*]{1,9}){8}""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridShortenedSusserPattern { get; }

	[GeneratedRegex("""(?<=\:)(\d{3}\s+)*\d{3}""", RegexOptions.Compiled, 5000)]
	internal static partial Regex EliminationPattern { get; }


	/// <inheritdoc cref="IValueConverter{T}.TryFormat(ref readonly T, IFormatProvider?, out string?)"/>
	/// <typeparam name="TGrid">
	/// The type of grid. The supported types are <see cref="Grid"/> and <see cref="MarkerGrid"/>.
	/// </typeparam>
	/// <seealso cref="Grid"/>
	/// <seealso cref="MarkerGrid"/>
	public bool TryFormat<TGrid>(ref readonly TGrid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
		where TGrid : unmanaged, IInlineArrayGrid<TGrid>
	{
		var r = b(value);
		result = this switch
		{
			{ IsCompatibleMode: true } => $":0000:x:{r}{new(':', 3)}",
			{ OnlyEliminations: true } => EliminationPattern.Match(r) switch
			{
				{ Success: true, Value: var str } => str,
				_ => string.Empty
			},
			_ => r
		};
		return true;


		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(WithCandidates)}>k__BackingField")]
		static extern safe ref bool set_WithCandidates(SusserGridConverter @this);

		string b(in TGrid grid)
		{
			var thisCopied = Clone();
			set_WithCandidates(thisCopied) = false;

			var sb = new StringBuilder(162);
			var originalGrid = this switch
			{
				{ WithCandidates: true, ShortenSusser: false } when thisCopied.TryFormat(in grid, provider, out var result)
					=> TGrid.Parse(result),
				_
					=> TGrid.Undefined
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
							eliminatedCandidates += c * 9 + i;
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
			).ToString(new TripletCandidateMapConverter());
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
						result += cell * 9 + digit;
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
						result += cell * 9 + digit;
					}
				}
			}
			return result;
		}

		static string shorten(string @base, char placeholder)
		{
			// lang = regex
			var placeholderPattern = placeholder == Dot ? @"\.+" : @"0+";
			var resultSpan = (stackalloc char[81]);
			var spanIndex = 0;
			for (var i = 0; i < 9; i++)
			{
				var characterIndexStart = i * 9;
				var sliced = @base.Substring(characterIndexStart, 9);
				switch (Regex.Matches(sliced, placeholderPattern))
				{
					case []:
					{
						// Can't find any simplifications.
						Unsafe.CopyBlock(
							ref Unsafe.ByteRef(ref resultSpan[characterIndexStart]),
							in Unsafe.ReadOnlyByteRef(in sliced.Span[0]),
							sizeof(char) * 9
						);
						spanIndex += 9;
						break;
					}
					case var collection:
					{
						var equalityComparer = EqualityComparer<Match>.Create(equals, getHashCode);
						switch (new HashSet<Match>(collection, equalityComparer))
						{
							case [{ Length: var firstLength }]:
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


						bool equals(Match? l, Match? r) => (l?.Length ?? 0) == (r?.Length ?? 0);

						int getHashCode(Match v) => v.Length;
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

	/// <inheritdoc cref="IValueConverter{T}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out T)"/>
	/// <typeparam name="TGrid"><inheritdoc cref="TryFormat" path="/typeparam[@name='TGrid']"/></typeparam>
	public bool TryParse<TGrid>(ReadOnlySpan<char> text, IFormatProvider? provider, out TGrid result)
		where TGrid : unmanaged, IInlineArrayGrid<TGrid>
	{
		if (IsCompatibleMode)
		{
			goto ReturnFalse;
		}

		var match = (ShortenSusser ? GridShortenedSusserPattern : GridSusserPattern).Match(text.ToString()).Value;
		if (ShortenSusser && (match is not { Length: <= 81 } || !expandCode(match, out match)))
		{
			goto ReturnFalse;
		}

		// Step 1: fills all digits.
		(result, var i) = (TGrid.Empty, 0);
		if (match.Length is not (var length and not 0))
		{
			goto ReturnFalse;
		}

		for (var realPos = 0; i < length && match[i] != ':'; realPos++)
		{
			switch (match[i])
			{
				// Modifiable value
				case '+':
				{
					if (i < length - 1)
					{
						if (match[i + 1] is var nextChar and >= '1' and <= '9')
						{
							// Set value.
							result.SetDigit(realPos, nextChar - '1');

							// Add 2 on iteration variable to skip 2 characters (A plus sign '+' and a digit).
							i += 2;
						}
						else
						{
							// Why isn't the character a digit character?
							goto ReturnFalse;
						}
					}
					else
					{
						goto ReturnFalse;
					}

					break;
				}

				// Placeholder
				case '.' or '0':
				{
					if (typeof(TGrid) == typeof(MarkerGrid))
					{
						result.SetState(realPos, CellState.Empty);
					}

					// Do nothing but only move 1 step forward.
					i++;

					break;
				}

				// Normal digit character
				case var c and >= '1' and <= '9':
				{
					// Digits are representing given values in the grid.
					// Not the plus sign, but a placeholder '0' or '.'.
					// Set value.
					result.SetDigit(realPos, c - '1');

					// Set the cell state as 'CellState.Given'.
					// If the code below doesn't make sense to you,
					// you can see the comments in method 'OnParsingSusser(string)' to know the meaning also.
					result.SetState(realPos, CellState.Given);

					// Finally moves 1 step forward.
					i++;

					break;
				}

				// Other invalid characters
				default:
				{
					goto ReturnFalse;
				}
			}
		}

		// Step 2: eliminates candidates if exist.
		// If we have met the colon sign ':', this loop would not be executed.
		if (EliminationPattern.Match(match) is { Success: true, Value: var elimMatch })
		{
			var candidates = CandidateMap.Parse(elimMatch, new TripletCandidateMapConverter());

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
					if (MaskToCellState(mask) == CellState.Empty)
					{
#pragma warning disable CS0675
						mask = distribution.TryGetValue(cell, out var digitsMask)
							? (Mask)(mask >> 9 & 7 << 9 | digitsMask)
							: Grid.EmptyMask;
#pragma warning restore CS0675
					}
				}
			}
		}

	ReturnResult:
		return true;

	ReturnFalse:
		result = TGrid.Undefined;
		return false;


		static bool expandCode(string? original, [NotNullWhen(true)] out string? result)
		{
			// We must the string code holds 8 ','s and is with no ':' or '+'.
			if (original is null || original.ContainsAny(':', '+') || original.Span.Count(',') != 8)
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

	/// <inheritdoc cref="ICloneable.Clone"/>
	protected abstract SusserGridConverter Clone();

	/// <inheritdoc/>
	bool IValueConverter<Grid>.TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
		=> TryFormat(in value, provider, out result);

	/// <inheritdoc/>
	bool IValueConverter<Grid>.TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
		=> TryParse(text, provider, out result);
}
