namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents a coordinate converter using K9 notation.
/// </summary>
/// <param name="MakeLettersUpperCase">
/// <para>Indicates whether we make the letters be upper-casing.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="MakeDigitBeforeCell">
/// <para>Indicates whether digits will be displayed before the cell coordinates.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="AlwaysOutputBracket">
/// <para>Indicates whether brackets will always be included in output text.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="FinalRowLetter">
/// <para>
/// Indicates the character that displays for the last row. Generally it uses <c>'I'</c> to be the last row,
/// but sometimes it may produce much difficulty on distinct with digit 1 and i (They are nearly same by its shape).
/// This option will change the last row letter if you want to change it.
/// </para>
/// <para>The value is <c>'I'</c> by default. You can set the value to <c>'J'</c> or <c>'K'</c>; other letters are not suggested.</para>
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
public sealed record K9Converter(
	bool MakeLettersUpperCase = false,
	bool MakeDigitBeforeCell = false,
	bool AlwaysOutputBracket = false,
	char FinalRowLetter = 'I',
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
			switch (cells)
			{
				case []:
				{
					return AlwaysOutputBracket
						? $"{NotationBracket.GetOpenBracket()}{NotationBracket.GetClosedBracket()}"
						: string.Empty;
				}
				case [var p]:
				{
					var row = p / 9;
					var column = p % 9;
					var rowCharacter = row == 8
						? MakeLettersUpperCase ? char.ToUpper(FinalRowLetter) : char.ToLower(FinalRowLetter)
						: (char)((MakeLettersUpperCase ? 'A' : 'a') + row);
					var result = $"{rowCharacter}{DigitConverter((Mask)(1 << column))}";
					return AlwaysOutputBracket
						? $"{NotationBracket.GetOpenBracket()}{result}{NotationBracket.GetClosedBracket()}"
						: result;
				}
				default:
				{
					return r(cells);
				}
			}


			string r(in CellMap cells)
			{
				var sb = new StringBuilder(18);
				var output = CoordinateSimplifier.Simplify(cells);
				var needAddingBrackets = AlwaysOutputBracket
					|| output.Length != 1 && Enum.IsDefined(NotationBracket) && NotationBracket != NotationBracket.None;
				if (needAddingBrackets)
				{
					sb.Append(NotationBracket.GetOpenBracket());
				}
				foreach (var (rows, columns) in output)
				{
					sb.AppendRange(
						d => (
							d == 8
								? MakeLettersUpperCase ? char.ToUpper(FinalRowLetter) : char.ToLower(FinalRowLetter)
								: (char)((MakeLettersUpperCase ? 'A' : 'a') + d)
						).ToString(),
						elements: rows
					);
					sb.AppendRange(d => DigitConverter((Mask)(1 << d)), elements: columns);
					sb.Append(DefaultSeparator);
				}
				sb.RemoveFrom(^DefaultSeparator.Length);
				if (needAddingBrackets)
				{
					sb.Append(NotationBracket.GetClosedBracket());
				}
				return sb.ToString();
			}
		};

	/// <inheritdoc/>
	public override CandidateMapFormatter CandidateConverter
		=> (in candidates) =>
		{
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
						sb.Append(DigitBracketInCandidateGroups.GetOpenBracket());
					}
					sb.Append(digitGroup.Key + 1);
					if (needAddingBrackets)
					{
						sb.Append(DigitBracketInCandidateGroups.GetClosedBracket());
					}

					sb.Append(CellConverter(cells));
				}
				else
				{
					sb.Append(CellConverter(cells));
					sb.Append(needAddingBrackets ? DigitBracketInCandidateGroups.GetOpenBracket() : "(");
					sb.Append(digitGroup.Key + 1);
					sb.Append(needAddingBrackets ? DigitBracketInCandidateGroups.GetClosedBracket() : ")");
				}

				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFrom(^DefaultSeparator.Length).ToString();
		};

	/// <inheritdoc/>
	public override Func<HouseMask, string> HouseConverter
		=> housesMask =>
		{
			if (housesMask == 0)
			{
				return string.Empty;
			}

			if (BitOperations.IsPow2(housesMask))
			{
				var house = BitOperations.Log2(housesMask);
				var houseType = house.ToHouseType();
				return string.Format(
					SR.Get(
						houseType switch
						{
							HouseType.Row => "RowLabel",
							HouseType.Column => "ColumnLabel",
							HouseType.Block => "BlockLabel",
							_ => throw new InvalidOperationException($"The specified house value '{nameof(house)}' is invalid.")
						},
						TargetCurrentCulture
					),
					house % 9 + 1
				);
			}

			var dic = new Dictionary<HouseType, List<House>>(3);
			foreach (var house in housesMask)
			{
				var houseType = house.ToHouseType();
				if (!dic.TryAdd(houseType, [house]))
				{
					dic[houseType].Add(house);
				}
			}

			var sb = new StringBuilder(30);
			foreach (var (houseType, h) in from kvp in dic orderby kvp.Key.ProgramOrder select kvp)
			{
				sb.Append(
					string.Format(
						SR.Get(
							houseType switch
							{
								HouseType.Row => "RowLabel",
								HouseType.Column => "ColumnLabel",
								HouseType.Block => "BlockLabel",
								_ => throw new InvalidOperationException($"The specified house value '{nameof(houseType)}' is invalid.")
							},
							TargetCurrentCulture
						),
						string.Concat(from house in h select (house % 9 + 1).ToString())
					)
				);
			}
			return sb.ToString();
		};

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Conclusion>, string> ConclusionConverter
		=> conclusions =>
		{
			return conclusions switch
			{
				[] => string.Empty,
				[var (t, c, d)] when (t == Assignment ? AssignmentToken : EliminationToken) is var token
					=> $"{CellConverter(in c.AsCellMap())}{token}{DigitConverter((Mask)(1 << d))}",
				_ => toString(conclusions)
			};


			unsafe string toString(ReadOnlySpan<Conclusion> c)
			{
				var conclusions = new Conclusion[c.Length];
				Unsafe.CopyBlock(
					ref @ref.ByteRef(ref conclusions[0]),
					in @ref.ReadOnlyByteRef(in c[0]),
					(uint)(sizeof(Conclusion) * c.Length)
				);

				var sb = new StringBuilder(50);

				Array.Sort(conclusions, static (left, right) => left.CompareTo(right));
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

					sb.RemoveFrom(^DefaultSeparator.Length);
					if (!hasOnlyOneType)
					{
						sb.Append(DefaultSeparator);
					}
				}

				if (!hasOnlyOneType)
				{
					sb.RemoveFrom(^DefaultSeparator.Length);
				}

				return sb.ToString();
			}
		};

	/// <inheritdoc/>
	public override Func<Mask, string> DigitConverter
		=> new LiteralCoordinateConverter(DigitsSeparator: DigitsSeparator).DigitConverter;

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Miniline>, string> IntersectionConverter
		=> intersections => DefaultSeparator switch
		{
			null or [] => string.Concat(
				from intersection in intersections
				let baseSet = intersection.Base.Line
				let coverSet = intersection.Base.Block
				select string.Format(
					SR.Get("LockedCandidatesLabel", TargetCurrentCulture),
					[
						((House)baseSet).ToHouseType() switch
						{
							HouseType.Block => string.Format(SR.Get("BlockLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString()),
							HouseType.Row => string.Format(SR.Get("RowLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString()),
							HouseType.Column => string.Format(SR.Get("ColumnLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString())
						},
						((House)coverSet).ToHouseType() switch
						{
							HouseType.Block => string.Format(SR.Get("BlockLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString()),
							HouseType.Row => string.Format(SR.Get("RowLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString()),
							HouseType.Column => string.Format(SR.Get("ColumnLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString())
						}
					]
				)
			),
			_ => string.Join(
				DefaultSeparator,
				from intersection in intersections
				let baseSet = intersection.Base.Line
				let coverSet = intersection.Base.Block
				select string.Format(
					SR.Get("LockedCandidatesLabel", TargetCurrentCulture),
					[
						((House)baseSet).ToHouseType() switch
						{
							HouseType.Block => string.Format(SR.Get("BlockLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString()),
							HouseType.Row => string.Format(SR.Get("RowLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString()),
							HouseType.Column => string.Format(SR.Get("ColumnLabel", TargetCurrentCulture), (baseSet % 9 + 1).ToString())
						},
						((House)coverSet).ToHouseType() switch
						{
							HouseType.Block => string.Format(SR.Get("BlockLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString()),
							HouseType.Row => string.Format(SR.Get("RowLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString()),
							HouseType.Column => string.Format(SR.Get("ColumnLabel", TargetCurrentCulture), (coverSet % 9 + 1).ToString())
						}
					]
				)
			)
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
				sb.Append(MakeLettersUpperCase ? "Mega Row" : "mega row");
				foreach (var megaRow in megaRows)
				{
					sb.Append(megaRow + 1);
				}

				sb.Append(DefaultSeparator);
			}
			if (megalines.TryGetValue(false, out var megaColumns))
			{
				sb.Append(MakeLettersUpperCase ? "Mega Column" : "mega column");
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
				var fromCellString = CellConverter(in conjugatePair.From.AsCellMap());
				var toCellString = CellConverter(in conjugatePair.To.AsCellMap());
				sb.Append($"{fromCellString} == {toCellString}.{DigitConverter((Mask)(1 << conjugatePair.Digit))}");
				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFrom(^DefaultSeparator.Length).ToString();
		};


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override object? GetFormat(Type? formatType) => formatType == typeof(CoordinateConverter) ? this : null;
}
