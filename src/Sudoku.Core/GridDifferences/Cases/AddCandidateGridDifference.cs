namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of candidate digits are added.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
/// <param name="areCorrect"><inheritdoc path="/param[@name='areCorrect']"/></param>
[method: JsonConstructor]
public sealed class AddCandidateGridDifference(CandidateMap candidates, bool areCorrect) : AddGridDifference(candidates, areCorrect)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "C+";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Empty;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.AddCandidate;

	/// <inheritdoc/>
	protected override string CellTypeString => nameof(Candidate);


	/// <inheritdoc/>
	public override AddCandidateGridDifference Clone() => new(Candidates, AreCorrect);
}
