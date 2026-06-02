namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of candidate digits is removed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class RemoveCandidateGridDifference(CandidateMap candidates) : RemoveGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "C-";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Empty;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.RemoveCandidate;

	/// <inheritdoc/>
	protected override string CellTypeString => "Candidate";


	/// <inheritdoc/>
	public override RemoveCandidateGridDifference Clone() => new(Candidates);
}
