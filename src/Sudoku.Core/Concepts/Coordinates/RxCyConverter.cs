namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents a coordinate converter using RxCy notation.
/// </summary>
/// <param name="MakeLettersUpperCase">
/// <para>Indicates whether we make the letters <c>'r'</c>, <c>'c'</c> and <c>'b'</c> to be upper-cased.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="MakeDigitBeforeCell">
/// <para>Indicates whether digits will be displayed before the cell coordinates.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="AlwaysOutputBracket">
/// <para>Indicates whether brackets will be always included in output text.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="HouseNotationOnlyDisplayCapitals">
/// <para>Indicates whether the houses will be displayed its capitals only.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="DefaultSeparator"><inheritdoc/></param>
/// <param name="DigitsSeparator"><inheritdoc/></param>
/// <param name="AssignmentToken"><inheritdoc/></param>
/// <param name="EliminationToken"><inheritdoc/></param>
/// <param name="NotationBracket"><inheritdoc/></param>
/// <param name="DigitBracketInCandidateGroups">
/// <para>Indicates the bracket surrounding digits in candidate output notation.</para>
/// <para>The value is <see cref="NotationBracket.None"/> by default.</para>
/// </param>
/// <param name="CurrentCulture"><inheritdoc/></param>
public sealed record RxCyConverter(
	bool MakeLettersUpperCase = false,
	bool MakeDigitBeforeCell = false,
	bool AlwaysOutputBracket = false,
	bool HouseNotationOnlyDisplayCapitals = false,
	string DefaultSeparator = ", ",
	string? DigitsSeparator = null,
	string AssignmentToken = " = ",
	string EliminationToken = " <> ",
	NotationBracket NotationBracket = NotationBracket.None,
	NotationBracket DigitBracketInCandidateGroups = NotationBracket.None,
	CultureInfo? CurrentCulture = null
) : CoordinateConverter(DefaultSeparator, DigitsSeparator, AssignmentToken, EliminationToken, NotationBracket, CurrentCulture)
{
	/// <inheritdoc/>
	public override CellMapFormatter CellConverter
		=> (in cells) =>
		{
			return cells switch
			{
				[] => AlwaysOutputBracket switch
				{
					true => $"{NotationBracket.OpenBracket}{NotationBracket.ClosedBracket}",
					_ => string.Empty
				},
				[var p] when (MakeLettersUpperCase ? $"R{p / 9 + 1}C{p % 9 + 1}" : $"r{p / 9 + 1}c{p % 9 + 1}") is var result => AlwaysOutputBracket switch
				{
					true => $"{NotationBracket.OpenBracket}{result}{NotationBracket.ClosedBracket}",
					_ => result
				},
				_ => r(cells)
			};


			string r(in CellMap cells)
			{
				var sb = new StringBuilder(50);
				var output = CoordinateSimplifier.Simplify(cells);
				var needAddingBrackets = AlwaysOutputBracket
					|| output.Length != 1 && Enum.IsDefined(NotationBracket) && NotationBracket != NotationBracket.None;
				if (needAddingBrackets)
				{
					sb.Append(NotationBracket.OpenBracket);
				}
				foreach (var (rows, columns) in output)
				{
					sb
						.Append(MakeLettersUpperCase ? 'R' : 'r')
						.AppendRange(rows, d => DigitConverter((Mask)(1 << d)))
						.Append(MakeLettersUpperCase ? 'C' : 'c')
						.AppendRange(columns, d => DigitConverter((Mask)(1 << d)))
						.Append(DefaultSeparator);
				}
				sb.RemoveFromEnd(DefaultSeparator.Length);
				if (needAddingBrackets)
				{
					sb.Append(NotationBracket.ClosedBracket);
				}
				return sb.ToString();
			}
		};

	/// <inheritdoc/>
	public override CandidateMapFormatter CandidateConverter
		=> (in candidates) =>
		{
			if (!candidates)
			{
				return string.Empty;
			}

			var needAddingBrackets = Enum.IsDefined(DigitBracketInCandidateGroups) && DigitBracketInCandidateGroups != NotationBracket.None;
			var sb = new StringBuilder(50);
			foreach (var digitGroup in
				from candidate in candidates
				group candidate by candidate % 9 into digitGroups
				orderby digitGroups.Key
				select digitGroups)
			{
				CellMap cells = [.. from candidate in digitGroup select candidate / 9];
				if (MakeDigitBeforeCell)
				{
					if (needAddingBrackets)
					{
						sb.Append(DigitBracketInCandidateGroups.OpenBracket);
					}
					sb.Append(digitGroup.Key + 1);
					if (needAddingBrackets)
					{
						sb.Append(DigitBracketInCandidateGroups.ClosedBracket);
					}

					sb.Append(CellConverter(cells));
				}
				else
				{
					sb.Append(CellConverter(cells));
					sb.Append(needAddingBrackets ? DigitBracketInCandidateGroups.OpenBracket : "(");
					sb.Append(digitGroup.Key + 1);
					sb.Append(needAddingBrackets ? DigitBracketInCandidateGroups.ClosedBracket : ")");
				}

				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFromEnd(DefaultSeparator.Length).ToString();
		};

	/// <inheritdoc/>
	public override Func<HouseMask, string> HouseConverter
		=> housesMask =>
		{
			if (housesMask == 0)
			{
				return string.Empty;
			}

			if (HouseNotationOnlyDisplayCapitals)
			{
				var sb = new StringBuilder(27);
				for (var (houseIndex, i) = (9, 0); i < 27; i++, houseIndex = (houseIndex + 1) % 27)
				{
					if ((housesMask >> houseIndex & 1) != 0)
					{
						sb.Append(getChar(houseIndex / 9));
					}
				}
				return sb.ToString();
			}

			if (IsPow2(housesMask))
			{
				var house = Log2((uint)housesMask);
				return $"{getChar(house)}{house % 9 + 1}";
			}

			var dic = new Dictionary<HouseType, List<House>>(3);
			foreach (var house in housesMask)
			{
				var houseType = house.HouseType;
				if (!dic.TryAdd(houseType, [house]))
				{
					dic[houseType].Add(house);
				}
			}

			var resultBuilder = new StringBuilder(30);
			foreach (var (houseType, h) in
				from kvp in dic
				orderby kvp.Key switch { HouseType.Row => 0, HouseType.Column => 1, _ => 2 }
				select kvp)
			{
				resultBuilder
					.Append(houseType switch { HouseType.Row => 'r', HouseType.Column => 'c', _ => 'b' })
					.AppendRange(from house in h select house % 9 + 1, static h => h.ToString());
			}
			return resultBuilder.ToString();


			static char getChar(House house)
				=> house switch
				{
					>= 0 and < 9 => 'b',
					>= 9 and < 18 => 'r',
					>= 18 and < 27 => 'c',
					_ => throw new ArgumentOutOfRangeException(nameof(house))
				};
		};

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Conclusion>, string> ConclusionConverter
		=> conclusions =>
		{
			return conclusions switch
			{
				[] => string.Empty,
				[var (t, c, d)] when (t == Assignment ? AssignmentToken : EliminationToken) is var token
					=> $"{CellConverter(c.AsCellMap())}{token}{DigitConverter((Mask)(1 << d))}",
				_ => toString(conclusions)
			};


			string toString(ReadOnlySpan<Conclusion> c)
			{
				var conclusions = new Conclusion[c.Length];
				Unsafe.CopyBlock(
					ref Unsafe.ByteRef(ref conclusions[0]),
					in Unsafe.ReadOnlyByteRef(in c[0]),
					(uint)(sizeof(Conclusion) * c.Length)
				);

				var sb = new StringBuilder(50);

				Array.Sort(conclusions);
				var selection = from conclusion in conclusions orderby conclusion.Digit group conclusion by conclusion.ConclusionType;
				var hasOnlyOneType = selection.Length == 1;
				foreach (var typeGroup in selection)
				{
					var token = typeGroup.Key == Assignment ? AssignmentToken : EliminationToken;
					foreach (var digitGroup in from conclusion in typeGroup group conclusion by conclusion.Digit)
					{
						sb.Append(CellConverter([.. from conclusion in digitGroup select conclusion.Cell]));
						sb.Append(token);
						sb.Append(digitGroup.Key + 1);
						sb.Append(DefaultSeparator);
					}

					sb.RemoveFromEnd(DefaultSeparator.Length);
					if (!hasOnlyOneType)
					{
						sb.Append(DefaultSeparator);
					}
				}

				if (!hasOnlyOneType)
				{
					sb.RemoveFromEnd(DefaultSeparator.Length);
				}

				return sb.ToString();
			}
		};

	/// <inheritdoc/>
	public override Func<Mask, string> DigitConverter
		=> new LiteralCoordinateConverter(DefaultSeparator: DefaultSeparator).DigitConverter;

	/// <inheritdoc/>
	public override Func<SegmentCollection, string> SegmentConverter
		=> segments =>
		{
			var sb = new StringBuilder();
			foreach (var segment in segments)
			{
				var line = segment.Line;
				var block = segment.Block;
				sb.Append($"{HouseConverter(1 << line)}{HouseConverter(1 << block)} ");
			}
			return sb.RemoveFromEnd(1).ToString();
		};

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Chute>, string> ChuteConverter
		=> chutes =>
		{
			var megalines = new Dictionary<bool, byte>(2);
			foreach (var (index, isRow, _) in chutes)
			{
				if (!megalines.TryAdd(isRow, (byte)(1 << index % 3)))
				{
					megalines[isRow] |= (byte)(1 << index % 3);
				}
			}

			var sb = new StringBuilder(12);
			if (megalines.TryGetValue(true, out var megaRows))
			{
				sb.Append(MakeLettersUpperCase ? "MR" : "mr");
				foreach (var megaRow in megaRows)
				{
					sb.Append(megaRow + 1);
				}

				sb.Append(DefaultSeparator);
			}
			if (megalines.TryGetValue(false, out var megaColumns))
			{
				sb.Append(MakeLettersUpperCase ? "MC" : "mc");
				foreach (var megaColumn in megaColumns)
				{
					sb.Append(megaColumn + 1);
				}
			}

			return sb.ToString();
		};

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Conjugate>, string> ConjugateConverter
		=> conjugatePairs =>
		{
			if (conjugatePairs.Length == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder(20);
			foreach (var conjugatePair in conjugatePairs)
			{
				var fromCellString = CellConverter(conjugatePair.From.AsCellMap());
				var toCellString = CellConverter(conjugatePair.To.AsCellMap());
				sb.Append(
					MakeDigitBeforeCell
						? $"{DigitConverter((Mask)(1 << conjugatePair.Digit))}{fromCellString} == {toCellString}"
						: $"{fromCellString} == {toCellString}({DigitConverter((Mask)(1 << conjugatePair.Digit))})"
				);
				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFromEnd(DefaultSeparator.Length).ToString();
		};

	/// <summary>
	/// The converter method that creates a <see cref="string"/> via the specified spaces.
	/// </summary>
	public Func<SpaceSet, string> SpaceConverter
		=> static spaces =>
		{
			return spaces.Count == 0 ? "<Empty>" : toPartString(spaces);


			static string toPartString(in SpaceSet sets)
			{
				Dictionary<Digit, Mask> rn = [], cn = [], bn = [], rc = [];
				foreach (var set in sets)
				{
					switch (set)
					{
						case { Cell: var cell and not -1 }:
						{
							if (!rc.TryAdd(cell / 9, (Mask)(1 << cell % 9)))
							{
								rc[cell / 9] |= (Mask)(1 << cell % 9);
							}
							break;
						}
						case { HouseDigit: var (house, digit) }:
						{
							var houseType = (HouseType)(house / 9);
							var index = house % 9;
							var dic = houseType switch { HouseType.Block => bn, HouseType.Row => rn, _ => cn };
							if (!dic.TryAdd(index, (Mask)(1 << digit)))
							{
								dic[index] |= (Mask)(1 << digit);
							}
							break;
						}
					}
				}

				var t1 = (
					(
						from pair in rc.ToArray()
						let r = pair.Key
						let values = pair.Value
						select $"{r + 1}n{string.Concat([.. from c in values select c + 1])}"
					) is { Length: not 0 } rcParts ? string.Join(' ', rcParts) : null) is { } w ? $"{w} " : string.Empty;
				var t2 = (
					(
						from pair in bn.ToArray()
						let n = pair.Key
						let values = pair.Value
						select $"{string.Concat([.. from b in values select b + 1])}b{n + 1}"
					) is { Length: not 0 } bnParts ? string.Join(' ', bnParts) : null) is { } x ? $"{x} " : string.Empty;
				var t3 = (
					(
						from pair in rn.ToArray()
						let n = pair.Key
						let values = pair.Value
						select $"{string.Concat([.. from r in values select r + 1])}r{n + 1}"
					) is { Length: not 0 } rnParts ? string.Join(' ', rnParts) : null) is { } y ? $"{y} " : string.Empty;
				var t4 = (
					(
						from pair in cn.ToArray()
						let n = pair.Key
						let values = pair.Value
						select $"{string.Concat([.. from c in values select c + 1])}c{n + 1}"
					) is { Length: not 0 } cnParts ? string.Join(' ', cnParts) : null) is { } z ? $"{z} " : string.Empty;
				return $"{t1}{t2}{t3}{t4}".TrimEnd();
			}
		};


	/// <summary>
	/// Get the label of each house.
	/// </summary>
	/// <param name="houseIndex">The house index.</param>
	/// <returns>The label.</returns>
	private char GetLabel(byte houseIndex)
		=> (houseIndex, MakeLettersUpperCase) switch
		{
			(0, true) => 'B',
			(0, _) => 'b',
			(1, true) => 'R',
			(1, _) => 'r',
			(2, true) => 'C',
			_ => 'c'
		};
}
