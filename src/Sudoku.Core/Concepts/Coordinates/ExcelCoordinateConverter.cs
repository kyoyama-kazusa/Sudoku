namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents for Excel coordinate-based converter.
/// </summary>
/// <param name="MakeLettersUpperCase">
/// <para>Indicates whether we make the letters be upper-cased.</para>
/// <para>The value is <see langword="false"/> by default.</para>
/// </param>
/// <param name="DefaultSeparator"><inheritdoc/></param>
/// <param name="DigitsSeparator"><inheritdoc/></param>
/// <param name="AssignmentToken"><inheritdoc/></param>
/// <param name="EliminationToken"><inheritdoc/></param>
/// <param name="NotationBracket"><inheritdoc/></param>
/// <param name="CurrentCulture"><inheritdoc/></param>
public sealed record ExcelCoordinateConverter(
	bool MakeLettersUpperCase = false,
	string DefaultSeparator = ", ",
	string? DigitsSeparator = null,
	string AssignmentToken = " = ",
	string EliminationToken = " <> ",
	NotationBracket NotationBracket = NotationBracket.None,
	CultureInfo? CurrentCulture = null
) : CoordinateConverter(DefaultSeparator, DigitsSeparator, AssignmentToken, EliminationToken, NotationBracket, CurrentCulture)
{
	/// <summary>
	/// The fallback converter.
	/// </summary>
	private readonly K9Converter _fallbackConverter = new(MakeLettersUpperCase, DefaultSeparator: DefaultSeparator, DigitsSeparator: DigitsSeparator);


	/// <inheritdoc/>
	public override CellMapFormatter CellConverter
		=> (in cells) =>
		{
			switch (cells)
			{
				case []: { return string.Empty; }
				case [var p]:
				{
					var row = p / 9;
					var column = p % 9;
					var columnCharacter = (char)((MakeLettersUpperCase ? 'A' : 'a') + column);
					return $"{DigitConverter((Mask)(1 << row))}{columnCharacter}";
				}
				default: { return r(cells); }
			}


			string r(in CellMap cells)
			{
				var sb = new StringBuilder(18);
				var output = CoordinateSimplifier.Simplify(cells);
				var needAddingBrackets = output.Length != 1 && Enum.IsDefined(NotationBracket) && NotationBracket != NotationBracket.None;
				if (needAddingBrackets)
				{
					sb.Append(NotationBracket.OpenBracket);
				}
				foreach (var (rows, columns) in output)
				{
					sb
						.AppendRange(columns, d => ((char)((MakeLettersUpperCase ? 'A' : 'a') + d)).ToString())
						.AppendRange(rows, d => DigitConverter((Mask)(1 << d)))
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
			var sb = new StringBuilder(50);
			foreach (var digitGroup in
				from candidate in candidates
				group candidate by candidate % 9 into digitGroups
				orderby digitGroups.Key
				select digitGroups)
			{
				sb.Append(CellConverter([.. from candidate in digitGroup select candidate / 9]));
				sb.Append('.');
				sb.Append(digitGroup.Key + 1);
				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFromEnd(DefaultSeparator.Length).ToString();
		};

	/// <inheritdoc/>
	public override Func<HouseMask, string> HouseConverter
		=> new K9Converter(MakeLettersUpperCase, DefaultSeparator: DefaultSeparator, DigitsSeparator: DigitsSeparator).HouseConverter;

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
		=> new LiteralCoordinateConverter(DigitsSeparator: DigitsSeparator).DigitConverter;

	/// <inheritdoc/>
	public override Func<SegmentCollection, string> SegmentConverter => _fallbackConverter.SegmentConverter;

	/// <inheritdoc/>
	public override Func<ReadOnlySpan<Chute>, string> ChuteConverter => _fallbackConverter.ChuteConverter;

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
				sb.Append($"{fromCellString} == {toCellString}.{DigitConverter((Mask)(1 << conjugatePair.Digit))}");
				sb.Append(DefaultSeparator);
			}
			return sb.RemoveFromEnd(DefaultSeparator.Length).ToString();
		};
}
