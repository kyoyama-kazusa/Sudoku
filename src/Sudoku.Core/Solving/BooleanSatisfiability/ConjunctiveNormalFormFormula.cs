namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Represents a Boolean formula in Conjunctive Normal Form (CNF).
/// Stores a list of clauses, where each clause is an array of <see cref="int"/> literals.
/// Using positive integers to represents a variable assigned <see langword="true"/>; for negative integers, <see langword="false"/>.
/// </summary>
/// <param name="numVars">Indicates total number of variables.</param>
public sealed partial class ConjunctiveNormalFormFormula([Property(Setter = PropertySetters.PrivateSet)] int numVars) :
	IEnumerable<ReadOnlyMemory<int>>
{
	/// <summary>
	/// Indicates the list of clauses.
	/// </summary>
	private readonly List<ReadOnlyMemory<int>> _clauses = [];


	/// <summary>
	/// Add a new clause (disjunction of literals) to the formula.
	/// </summary>
	/// <param name="literals">The literals.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddClause(params int[] literals) => _clauses.Add(literals);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<ReadOnlyMemory<int>> GetEnumerator() => new(_clauses.AsSpan());

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _clauses.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ReadOnlyMemory<int>> IEnumerable<ReadOnlyMemory<int>>.GetEnumerator() => _clauses.GetEnumerator();
}
