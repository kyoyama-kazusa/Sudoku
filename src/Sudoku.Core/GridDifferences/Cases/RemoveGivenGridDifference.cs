namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of given digits is removed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class RemoveGivenGridDifference(CandidateMap candidates) : RemoveGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "G-";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Given;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.RemoveGiven;


	/// <inheritdoc/>
	public override RemoveGivenGridDifference Clone() => new(Candidates);
}
