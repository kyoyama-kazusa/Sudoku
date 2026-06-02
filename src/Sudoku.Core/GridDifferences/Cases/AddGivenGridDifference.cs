namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of given digits are added.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
/// <param name="areCorrect"><inheritdoc path="/param[@name='areCorrect']"/></param>
[method: JsonConstructor]
public sealed class AddGivenGridDifference(CandidateMap candidates, bool areCorrect) : AddGridDifference(candidates, areCorrect)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "G+";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Given;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.AddGiven;


	/// <inheritdoc/>
	public override AddGivenGridDifference Clone() => new(Candidates, AreCorrect);
}
