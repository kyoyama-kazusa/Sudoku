namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of candidate digits is changed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class ChangedCandidateGridDifference(CandidateMap candidates) : ChangedGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "C^";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Given;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.ChangedGiven;

	/// <inheritdoc/>
	protected override string CellTypeString => "Candidate";


	/// <inheritdoc/>
	public override ChangedCandidateGridDifference Clone() => new(Candidates);
}
