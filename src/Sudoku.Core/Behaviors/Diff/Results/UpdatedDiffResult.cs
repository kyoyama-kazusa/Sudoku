namespace Sudoku.Behaviors.Diff.Results;

/// <summary>
/// Represents a difference that describes a type of digits (given, value or candidate) is updated.
/// </summary>
/// <param name="candidates"><inheritdoc cref="Candidates" path="/summary"/></param>
public abstract class UpdatedDiffResult(CandidateMap candidates) : DiffResult
{
	/// <summary>
	/// Indicates the candidates to be updated.
	/// </summary>
	public CandidateMap Candidates { get; } = candidates;


	/// <inheritdoc/>
	public abstract override string NotationPrefix { get; }

	/// <inheritdoc/>
	public sealed override string Notation => $"{NotationPrefix}{CandidatesRawString}";

	/// <summary>
	/// Indicates the target cell type to be added.
	/// </summary>
	public abstract CellState CellType { get; }

	/// <summary>
	/// Indicates the candidates string.
	/// </summary>
	protected string CandidatesRawString => string.Concat(from element in Candidates select element.ToString("000"));

	/// <summary>
	/// Indicates the target cell type string.
	/// </summary>
	protected virtual string CellTypeString => CellType.ToString();


	/// <inheritdoc/>
	public sealed override string ToString() => ToString(default(IFormatProvider));
}
