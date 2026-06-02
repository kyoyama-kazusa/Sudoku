namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of given digits is changed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class ChangedGivenGridDifference(CandidateMap candidates) : ChangedGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "G^";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Given;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.ChangedGiven;


	/// <inheritdoc/>
	public override ChangedGivenGridDifference Clone() => new(Candidates);
}
