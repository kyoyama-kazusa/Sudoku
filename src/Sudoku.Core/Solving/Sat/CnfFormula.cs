namespace Sudoku.Solving.Sat;

/// <summary>
/// Represents a Boolean formula in Conjunctive Normal Form (CNF).
/// Stores a list of clauses, where each clause is an array of <see cref="int"/> literals.
/// Positive integers represent a variable assigned <see langword="true"/>; negative integers represent the
/// negation of that variable (i.e. the literal is <see langword="false"/>).
/// </summary>
/// <param name="VariablesCount">Indicates total number of variables.</param>
/// <remarks>
/// Each clause is a disjunction of literals; the whole formula is the conjunction of clauses.
/// This type is a compact, immutable-ish container for CNF clauses used by the SAT solver.
/// For background on CNF, see <see href="https://en.wikipedia.org/wiki/Conjunctive_normal_form">Wikipedia</see>.
/// </remarks>
/// <seealso href="https://en.wikipedia.org/wiki/Conjunctive_normal_form">Wikipedia - Conjunctive Normal Form</seealso>
[DebuggerDisplay($$"""{{{nameof(ToDebuggerDisplayString)}}(60),nq}""")]
public sealed record CnfFormula(int VariablesCount) : IEnumerable<ReadOnlyMemory<int>>
{
	/// <summary>
	/// Represents character <c>"&#172;"</c>.
	/// </summary>
	public const char Not = '\u00AC';

	/// <summary>
	/// Represents character <c>"&#8744;"</c>.
	/// </summary>
	public const char Or = '\u2228';

	/// <summary>
	/// Represents character <c>"&#8743;"</c>.
	/// </summary>
	public const char And = '\u2227';

	/// <summary>
	/// Represents character <c>"&#8868;"</c>, indicating "always true".
	/// </summary>
	public const char Top = '\u22A4';

	/// <summary>
	/// Represents character <c>"&#8869;"</c>, indicating "contradiction".
	/// </summary>
	public const char Bottom = '\u22A5';


	/// <summary>
	/// Number of clauses currently stored in the formula.
	/// </summary>
	public int ClauseCount => Clauses.Count;

	/// <summary>
	/// Internal storage for clauses. Each clause is stored as a <see cref="ReadOnlyMemory{T}"/>
	/// to avoid unnecessary allocations when passing clauses around.
	/// </summary>
	/// <seealso cref="ReadOnlyMemory{T}"/>
	internal List<ReadOnlyMemory<int>> Clauses { get; } = [];


	/// <summary>
	/// Get clause at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The clause.</returns>
	public ReadOnlyMemory<int> this[int index] => Clauses[index];


	/// <summary>
	/// Add a new clause (disjunction of literals) to the expression.
	/// </summary>
	/// <param name="literals">The literals that form the clause.</param>
	public void AddClause(ReadOnlyMemory<int> literals) => Clauses.Add(literals);

	/// <inheritdoc/>
	public override string ToString() => ToDebuggerDisplayString(Array.MaxLength);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<ReadOnlyMemory<int>> GetEnumerator() => new(Clauses.AsSpan());

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Clauses.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ReadOnlyMemory<int>> IEnumerable<ReadOnlyMemory<int>>.GetEnumerator() => Clauses.GetEnumerator();

	private string ToDebuggerDisplayString(int cutLength)
	{
		if (ClauseCount == 0)
		{
			return Top.ToString();
		}

		var sb = new StringBuilder();
	loop_i:
		for (var i = 0; i < Clauses.Count; i++)
		{
			var clause = Clauses[i].Span;
			if (!appendOrFailed(sb, '('))
			{
				break loop_i;
			}
			if (clause.Length == 0)
			{
				if (!appendOrFailed(sb, Bottom))
				{
					break loop_i;
				}
			}
			else
			{
				for (var j = 0; j < clause.Length; j++)
				{
					var literal = clause[j];
					if (literal < 0)
					{
						if (!appendOrFailed(sb, Not))
						{
							break loop_i;
						}
					}

					if (!appendOrFailed(sb, getVariableName(Math.Abs(literal))))
					{
						break loop_i;
					}

					if (j + 1 < clause.Length)
					{
						if (!appendOrFailed(sb, Or))
						{
							break loop_i;
						}
					}
				}
			}
			if (!appendOrFailed(sb, ')'))
			{
				break loop_i;
			}

			if (i + 1 < Clauses.Count)
			{
				if (!appendOrFailed(sb, And))
				{
					break loop_i;
				}
			}
		}

		return sb.ToString();


		static string getVariableName(int variableIndex)
		{
			if (variableIndex <= 0)
			{
				return variableIndex.ToString();
			}

			var sb = new StringBuilder();
			while (variableIndex > 0)
			{
				variableIndex--;
				sb.Insert(0, (char)('A' + variableIndex % 26));
				variableIndex /= 26;
			}
			return sb.ToString();
		}


		bool appendOrFailed<T>(StringBuilder sb, T c) where T : notnull
		{
			if (sb.Length < cutLength)
			{
				sb.Append(c.ToString());
				return true;
			}
			sb.Append("...");
			return false;
		}
	}
}
