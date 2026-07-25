namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a type of digits (given, value or candidate) is updated.
/// </summary>
/// <param name="candidates"><inheritdoc cref="Candidates" path="/summary"/></param>
public closed class UpdatedGridDifference(CandidateMap candidates) : GridDifference
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
	protected string CandidatesRawString => +(from element in Candidates select element.ToString("000"));

	/// <summary>
	/// Indicates the target cell type string.
	/// </summary>
	protected virtual string CellTypeString => CellType.ToString();
}
