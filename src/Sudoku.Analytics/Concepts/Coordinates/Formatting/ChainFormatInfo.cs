namespace Sudoku.Concepts.Coordinates.Formatting;

/// <summary>
/// Represents a type that can format a <see cref="Chain"/> instance.
/// </summary>
/// <seealso cref="Chain"/>
public sealed partial class ChainFormatInfo : FormatInfo<Chain>
{
	/// <summary>
	/// Initializes a <see cref="ChainFormatInfo"/> instance.
	/// </summary>
	public ChainFormatInfo()
	{
	}

	/// <summary>
	/// Copies converter options into the current instance.
	/// </summary>
	/// <param name="baseConverter">The base converter.</param>
	public ChainFormatInfo(CoordinateConverter baseConverter)
		=> _ = baseConverter switch
		{
			RxCyConverter
			{
				MakeDigitBeforeCell: var makeDigitBeforeCell,
				MakeLettersUpperCase: var makeLettersUpperCase,
				AlwaysOutputBracket: var alwaysOutputBracket,
				DefaultSeparator: var defaultSeparator,
				NotationBracket: var notationBracket,
				DigitBracketInCandidateGroups: var digitBracketInCandidateGroups
			} => (
				AlwaysOutputBracket = alwaysOutputBracket,
				NodeFormatType = CoordinateType.RxCy,
				MakeDigitBeforeCell = makeDigitBeforeCell,
				MakeLettersUpperCase = makeLettersUpperCase,
				DefaultSeparator = defaultSeparator,
				NotationBracket = notationBracket,
				DigitBracketInCandidateGroups = digitBracketInCandidateGroups
			),
			K9Converter
			{
				MakeLettersUpperCase: var makeLettersUpperCase,
				AlwaysOutputBracket: var alwaysOutputBracket,
				MakeDigitBeforeCell: var makeDigitBeforeCell,
				FinalRowLetter: var finalRowLetter,
				DefaultSeparator: var defaultSeparator,
				NotationBracket: var notationBracket,
				DigitBracketInCandidateGroups: var digitBracketInCandidateGroups
			} => (
				NodeFormatType = CoordinateType.K9,
				MakeLettersUpperCase = makeLettersUpperCase,
				MakeDigitBeforeCell = makeDigitBeforeCell,
				AlwaysOutputBracket = alwaysOutputBracket,
				FinalRowLetter = finalRowLetter,
				DefaultSeparator = defaultSeparator,
				NotationBracket = notationBracket,
				DigitBracketInCandidateGroups = digitBracketInCandidateGroups
			),
			ExcelCoordinateConverter
			{
				MakeLettersUpperCase: var makeLettersUpperCase,
				DefaultSeparator: var defaultSeparator,
				NotationBracket: var notationBracket
			} => (
				MakeLettersUpperCase = makeLettersUpperCase,
				NodeFormatType = CoordinateType.Excel,
				DefaultSeparator = defaultSeparator,
				NotationBracket = notationBracket
			),
			LiteralCoordinateConverter
			{
				DefaultSeparator: var defaultSeparator,
				NotationBracket: var notationBracket
			} => (
				NodeFormatType = CoordinateType.Literal,
				DefaultSeparator = defaultSeparator,
				NotationBracket = notationBracket
			),
			_ => default(object?)
		};


	/// <inheritdoc cref="RxCyConverter.MakeDigitBeforeCell"/>
	public bool MakeDigitBeforeCell { get; init; } = false;

	/// <inheritdoc cref="RxCyConverter.MakeLettersUpperCase"/>
	public bool MakeLettersUpperCase { get; init; } = false;

	/// <summary>
	/// Indicates whether strong and weak links inside a cell will be folded. By default it's <see langword="false"/>.
	/// </summary>
	public bool FoldLinksInCell { get; init; } = false;

	/// <summary>
	/// Indicates whether digits are inlined in links. By default it's <see langword="false"/>.
	/// </summary>
	public bool InlineDigitsInLink { get; init; } = false;

	/// <inheritdoc cref="RxCyConverter.AlwaysOutputBracket"/>
	public bool AlwaysOutputBracket { get; init; } = false;

	/// <inheritdoc cref="K9Converter.FinalRowLetter"/>
	public char FinalRowLetter { get; init; } = 'I';

	/// <summary>
	/// Indicates the default separator. By default it's a comma <c>", "</c>.
	/// </summary>
	public string DefaultSeparator { get; init; } = ", ";

	/// <summary>
	/// Indicates inlined digits separator. By default it's pipe operator <c>"|"</c>.
	/// </summary>
	public string InlinedDigitsSeparator { get; init; } = "|";

	/// <summary>
	/// Indicates the connector text for strong links. By default it's a double equal sign <c>" == "</c>.
	/// </summary>
	public string? StrongLinkConnector { get; init; } = " == ";

	/// <summary>
	/// Indicates the connector text for weak links. By default it's a double minus sign <c>" -- "</c>.
	/// </summary>
	public string? WeakLinkConnector { get; init; } = " -- ";

	/// <summary>
	/// Represents a pair of fixes that describes on or off state of a chain node respectively.
	/// By default it's <see langword="null"/>.
	/// </summary>
	public (string OnFix, string OffFix)? OnOffStateFixes { get; init; }

	/// <inheritdoc cref="CoordinateConverter.NotationBracket"/>
	public NotationBracket NotationBracket { get; init; } = NotationBracket.None;

	/// <inheritdoc cref="RxCyConverter.DigitBracketInCandidateGroups"/>
	public NotationBracket DigitBracketInCandidateGroups { get; init; } = NotationBracket.None;

	/// <summary>
	/// Indicates the bracket of same-cell digit groups, e.g. the kind of bracket of <c>(2=3)r1c1</c>.
	/// By default it's <see cref="NotationBracket.Round"/>.
	/// </summary>
	public NotationBracket DigitGroupSameCellBracket { get; init; } = NotationBracket.Round;

	/// <summary>
	/// Indicates the prefix or suffix style to describe on/off state of each chain node.
	/// By default it's <see cref="OnOffNotationFix.None"/>.
	/// </summary>
	public OnOffNotationFix OnOffNotationFix { get; init; } = OnOffNotationFix.None;

	/// <summary>
	/// Indicates a type that formats each node (a group of candidates) in the chain pattern.
	/// </summary>
	public CoordinateType NodeFormatType { get; init; } = CoordinateType.RxCy;


	/// <summary>
	/// Indicates the standard format. This format supports for both parsing and formatting.
	/// </summary>
	/// <remarks>
	/// Example output:<br/><c><![CDATA[r4c4(6) == r4c1(6) -- r4c1(8) == r4c9(8) -- r9c9(8) == r9c4(8)]]></c>
	/// </remarks>
	public static IFormatProvider Standard => new ChainFormatInfo();

	/// <summary>
	/// <para>Indicates Eureka chain format. This format supports for both parsing and formatting.</para>
	/// <para>
	/// Visit <see href="http://sudopedia.enjoysudoku.com/Eureka.html">this link</see> to learn more information
	/// about Eureka Notation.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Example output:<br/><c><![CDATA[6r4c4=(6-8)r4c1=8r4c9-8r9c9=8r9c4]]></c>
	/// </remarks>
	public static IFormatProvider Eureka
		=> new ChainFormatInfo
		{
			MakeDigitBeforeCell = true,
			FoldLinksInCell = true,
			DefaultSeparator = "|",
			StrongLinkConnector = "=",
			WeakLinkConnector = "-",
			DigitBracketInCandidateGroups = NotationBracket.None,
			DigitGroupSameCellBracket = NotationBracket.Round
		};

	/// <summary>
	/// <para>Indicates B/B plot (Bilocation/Bivalue Plot) notation format. This format only supports for formatting.</para>
	/// <para>
	/// Visit <see href="http://forum.enjoysudoku.com/the-notation-used-in-nice-loops-and-sins-t3628.html">this link</see>
	/// to learn more information about this notation.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Example output:<br/><c><![CDATA[[r4c4]=6=[r4c1]-6|8-[r4c1]=8=[r4c9]-8-[r9c9]=8=[r9c4]]]></c>
	/// </remarks>
	public static IFormatProvider BivalueBilocationPlot
		=> new ChainFormatInfo
		{
			InlineDigitsInLink = true,
			DefaultSeparator = "|",
			InlinedDigitsSeparator = "|",
			StrongLinkConnector = "=",
			WeakLinkConnector = "-",
			NotationBracket = NotationBracket.Square
		};

	/// <summary>
	/// <para>Indicates On/Off plot notation format. This format only supports for formatting.</para>
	/// <para>
	/// I may miss the main page of introduction about this notation,
	/// but you can visit <see href="https://www.sudokuwiki.org/Alternating_Inference_Chains">this link</see> to see such usages.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Example output:<br/><c><![CDATA[+6[D4]-6[D1]+8[D1]-8[D9]+8[I9]-8[I4]]]></c>
	/// </remarks>
	public static IFormatProvider OnOffPlot
		=> new ChainFormatInfo
		{
			MakeLettersUpperCase = true,
			MakeDigitBeforeCell = true,
			AlwaysOutputBracket = true,
			DefaultSeparator = "|",
			StrongLinkConnector = null,
			WeakLinkConnector = null,
			NodeFormatType = CoordinateType.K9,
			NotationBracket = NotationBracket.Square,
			DigitBracketInCandidateGroups = NotationBracket.None,
			OnOffNotationFix = OnOffNotationFix.Prefix,
			OnOffStateFixes = ("+", "-")
		};

	[GeneratedRegex("""r[1-9]+c[1-9]+\([1-9]+\)\s*==?\s*r[1-9]+c[1-9]+\([1-9]+\)(\s*--?\s*r[1-9]+c[1-9]+\([1-9]+\)\s*==?\s*r[1-9]+c[1-9]+\([1-9]+\))+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex StandardFormatPattern { get; }

	[GeneratedRegex("""(\([1-9]+\)|\([1-9]+\s*[-=]\s*[1-9]+\)|[1-9])r[1-9]+c[1-9]+(\s*[-=]\s*(\([1-9]+\)|\([1-9]+\s*[-=]\s*[1-9]+\)|[1-9])r[1-9]+c[1-9]+)+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex EurekaFormatPattern { get; }

	[GeneratedRegex("""==?|--?""", RegexOptions.Compiled)]
	private static partial Regex StrongOrWeakLinkPattern { get; }

	[GeneratedRegex("""(\([1-9]+([=-][1-9]+)?\)|[1-9]+)r[1-9]+c[1-9]+|r[1-9]+c[1-9]+\([1-9]+\)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex CandidatePattern { get; }


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override IFormatProvider? GetFormat(Type? formatType) => formatType == typeof(ChainFormatInfo) ? this : null;

	/// <inheritdoc/>
	public override ChainFormatInfo Clone()
		=> new()
		{
			MakeDigitBeforeCell = MakeDigitBeforeCell,
			MakeLettersUpperCase = MakeLettersUpperCase,
			FoldLinksInCell = FoldLinksInCell,
			AlwaysOutputBracket = AlwaysOutputBracket,
			InlineDigitsInLink = InlineDigitsInLink,
			InlinedDigitsSeparator = InlinedDigitsSeparator,
			FinalRowLetter = FinalRowLetter,
			DefaultSeparator = DefaultSeparator,
			StrongLinkConnector = StrongLinkConnector,
			WeakLinkConnector = WeakLinkConnector,
			OnOffStateFixes = OnOffStateFixes,
			OnOffNotationFix = OnOffNotationFix,
			NodeFormatType = NodeFormatType,
			NotationBracket = NotationBracket,
			DigitBracketInCandidateGroups = DigitBracketInCandidateGroups,
			DigitGroupSameCellBracket = DigitGroupSameCellBracket
		};


	/// <inheritdoc/>
	protected override string FormatCore(in Chain obj)
	{
		var originalConverter = NodeFormatType.GetConverter() switch
		{
			RxCyConverter c => c with
			{
				MakeLettersUpperCase = MakeLettersUpperCase,
				MakeDigitBeforeCell = MakeDigitBeforeCell,
				AlwaysOutputBracket = AlwaysOutputBracket,
				DigitBracketInCandidateGroups = DigitBracketInCandidateGroups
			},
			K9Converter c => c with
			{
				MakeLettersUpperCase = MakeLettersUpperCase,
				MakeDigitBeforeCell = MakeDigitBeforeCell,
				AlwaysOutputBracket = AlwaysOutputBracket,
				FinalRowLetter = FinalRowLetter,
				DigitBracketInCandidateGroups = DigitBracketInCandidateGroups
			},
			ExcelCoordinateConverter c => c with { MakeLettersUpperCase = MakeLettersUpperCase },
			{ } tempConverter => tempConverter,
			_ => throw new InvalidOperationException()
		};
		var candidateConverter = originalConverter with { DefaultSeparator = DefaultSeparator, NotationBracket = NotationBracket };
		var needAddingBrackets_Digits = Enum.IsDefined(DigitGroupSameCellBracket) && DigitGroupSameCellBracket != NotationBracket.None;
		var needAddingBrackets_Cells = Enum.IsDefined(NotationBracket) && NotationBracket != NotationBracket.None;
		var span = obj.ValidNodes;
		var sb = new StringBuilder();
		for (var (linkIndex, i) = (obj.WeakStartIdentity, 0); i < span.Length; linkIndex++, i++)
		{
			var inference = Chain.Inferences[linkIndex & 1];
			ref readonly var nodeCandidates = ref span[i].Map;
			var nodeCells = nodeCandidates.Cells;
			var nodeDigits = nodeCandidates.Digits;
			ref readonly var nextNodeCandidates = ref i + 1 >= span.Length ? ref CandidateMap.Empty : ref span[i + 1].Map;
			var nextNodeCells = nextNodeCandidates.Cells;
			var nextNodeDigits = nextNodeCandidates.Digits;
			if (FoldLinksInCell && i != span.Length - 1 && nodeCells == nextNodeCells)
			{
				// (1)a=(2)a-(2)b=(3)b => (1=2)a-(2=3)b
				if (MakeDigitBeforeCell)
				{
					_ = needAddingBrackets_Digits ? sb.Append(DigitGroupSameCellBracket.GetOpenBracket()) : sb;
					sb.Append(candidateConverter.DigitConverter(nodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					sb.Append(candidateConverter.DigitConverter(nextNodeDigits));
					_ = needAddingBrackets_Digits ? sb.Append(DigitGroupSameCellBracket.GetClosedBracket()) : sb;
					sb.Append(nodeCells.ToString(candidateConverter));
					i++;
				}
				else
				{
					sb.Append(nodeCells.ToString(candidateConverter));
					sb.Append(needAddingBrackets_Digits ? DigitGroupSameCellBracket.GetOpenBracket() : "(");
					sb.Append(candidateConverter.DigitConverter(nodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					sb.Append(candidateConverter.DigitConverter(nextNodeDigits));
					sb.Append(needAddingBrackets_Digits ? DigitGroupSameCellBracket.GetClosedBracket() : ")");
				}

				inference = Chain.Inferences[++linkIndex & 1];
				goto AppendNextLinkToken;
			}

			// Append prefix of node, e.g. r3c3(2) is on => +2r3c3
			if ((OnOffNotationFix, OnOffStateFixes) is (OnOffNotationFix.Prefix, var (onPrefix, offPrefix)))
			{
				sb.Append(span[i].IsOn ? onPrefix : offPrefix);
			}

			if (InlineDigitsInLink)
			{
				// (1)a=(2)b => [a]=1|2=[b]
				_ = needAddingBrackets_Cells ? sb.Append(NotationBracket.GetOpenBracket()) : sb;
				sb.Append(nodeCells.ToString(candidateConverter));
				_ = needAddingBrackets_Cells ? sb.Append(NotationBracket.GetClosedBracket()) : sb;
			}
			else
			{
				sb.Append(nodeCandidates.ToString(candidateConverter));
			}

			// Append suffix of node, e.g. r3c3(2) is off => 2r3c3-
			if ((OnOffNotationFix, OnOffStateFixes) is (OnOffNotationFix.Suffix, var (onSuffix, offSuffix)))
			{
				sb.Append(span[i].IsOn ? onSuffix : offSuffix);
			}

		AppendNextLinkToken:
			if (i != span.Length - 1)
			{
				if (InlineDigitsInLink)
				{
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
					_ = nodeDigits == nextNodeDigits
						? sb.Append(candidateConverter.DigitConverter(nodeDigits))
						: sb
							.Append(candidateConverter.DigitConverter(nodeDigits))
							.Append(InlinedDigitsSeparator)
							.Append(candidateConverter.DigitConverter(nextNodeDigits));
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
				}
				else
				{
					sb.Append(inference == Inference.Strong ? StrongLinkConnector : WeakLinkConnector);
				}
			}
		}
		return sb.ToString();
	}

	/// <inheritdoc/>
	protected override Chain ParseCore(string str) => ParseCore(str, null);

	/// <inheritdoc cref="FormatInfo{T}.ParseCore(string)"/>
	/// <remarks>
	/// Now only supports for standard format and Eureka format. Plot views are not supported.
	/// </remarks>
	private Chain ParseCore(string str, CoordinateParser? coordinateParser)
	{
		coordinateParser ??= new RxCyParser();
		if (StandardFormatPattern.IsMatch(str) || EurekaFormatPattern.IsMatch(str))
		{
			return parseAsStandardOrEureka(str);
		}
		throw new FormatException();


		Chain parseAsStandardOrEureka(string str)
		{
			var tokens = from match in StrongOrWeakLinkPattern.Matches(str) select match.Value[0];
			for (var i = 0; i < tokens.Length - 1; i++)
			{
				if (i == 0 && tokens[i] != '=')
				{
					throw new FormatException();
				}

				if ((tokens[i], tokens[i + 1]) is not (('=', '-') or ('-', '=')))
				{
					throw new FormatException();
				}
			}

			var candidates = new List<CandidateMap>();
			foreach (var candidateMatch in from match in CandidatePattern.Matches(str) select match.Value)
			{
				// Valid candidate pattern examples:
				//   * r1c1(1)
				//   * 1r1c1
				//   * (1)r1c1
				//   * (1-2)r1c1

				var letterRIndex = candidateMatch.IndexOf('r', StringComparison.OrdinalIgnoreCase);
				if (letterRIndex == 0)
				{
					// 'r' is at the first position.

					// Find for brace indices (open and closed).
					var openBraceIndex = candidateMatch.IndexOf('(');
					var closedBraceIndex = candidateMatch.IndexOf(')');
					if (openBraceIndex == -1 || closedBraceIndex == -1 || closedBraceIndex <= openBraceIndex)
					{
						throw new FormatException();
					}

					var cellsString = candidateMatch[..openBraceIndex];
					var cells = coordinateParser.CellParser(cellsString);

					// Check for candidate values.
					var targetCandidates = CandidateMap.Empty;
					var digitsString = candidateMatch[(openBraceIndex + 1)..closedBraceIndex];
					foreach (var ch in digitsString)
					{
						foreach (var cell in cells)
						{
							targetCandidates.Add(cell * 9 + ch - '1');
						}
					}
					candidates.AddRef(targetCandidates);
				}
				else
				{
					// Digits are at the first position.

					var openBraceIndex = candidateMatch.IndexOf('(');
					var closedBraceIndex = candidateMatch.IndexOf(')');
					var digitsString = (openBraceIndex, closedBraceIndex) switch
					{
						(-1, -1) => candidateMatch[..letterRIndex],
						(not -1, not -1) when closedBraceIndex > openBraceIndex => candidateMatch[(openBraceIndex + 1)..closedBraceIndex],
						_ => throw new FormatException()
					};

					var cellsString = candidateMatch[letterRIndex..];
					var cells = coordinateParser.CellParser(cellsString);

					// Determine whether there's a token '=' or '-' split digits.
					// If so, we should treat them as two different nodes.
					if (digitsString.IndexOfAny('=', '-') is var splitIndex and not -1)
					{
						if (splitIndex + 1 >= digitsString.Length || digitsString[splitIndex + 1] is not (>= '1' and <= '9'))
						{
							throw new FormatException();
						}

						addElement(digitsString[..splitIndex], cells);
						addElement(digitsString[(splitIndex + 1)..], cells);
					}
					else
					{
						addElement(digitsString, cells);
					}


					void addElement(string digitsString, in CellMap cells)
					{
						var result = CandidateMap.Empty;
						foreach (var ch in digitsString)
						{
							foreach (var cell in cells)
							{
								result.Add(cell * 9 + ch - '1');
							}
						}
						candidates.AddRef(result);
					}
				}
			}

			var candidatesSpan = candidates.AsSpan().Reverse();
			var isLoop = candidatesSpan[^1] == candidatesSpan[0] && tokens[0] == '=';
			var (isOn, previousNode, nodesStored) = (false, default(Node), new List<Node>());
			for (var index = 1; index < candidatesSpan.Length; index++, isOn = !isOn)
			{
				ref readonly var nextNodeMap = ref candidatesSpan[index];
				ref readonly var currentNodeMap = ref candidatesSpan[index - 1];
				var currentNode = new Node(currentNodeMap, isOn, previousNode);
				var nextNode = new Node(nextNodeMap, !isOn, currentNode);

				if (index == 1)
				{
					nodesStored.Add(currentNode);
				}
				nodesStored.Add(nextNode);
				previousNode = currentNode;
			}
			if (isLoop)
			{
				nodesStored.Add(new(nodesStored[1].Map, nodesStored[1].IsOn, nodesStored[^1]));
			}

			var lastNode = nodesStored[^1];
			return isLoop ? new ContinuousNiceLoop(lastNode) : new AlternatingInferenceChain(lastNode);
		}
	}


	/// <inheritdoc cref="FormatCore(in Chain)"/>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(FormatCore))]
	internal static extern string FormatCoreUnsafeAccessor(ChainFormatInfo @this, in Chain obj);

	/// <inheritdoc cref="ParseCore(string)"/>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ParseCore))]
	internal static extern Chain ParseCoreUnsafeAccessor(ChainFormatInfo @this, string str);
}
