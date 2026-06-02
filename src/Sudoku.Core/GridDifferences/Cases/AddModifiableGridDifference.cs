namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of modifiable digits are added.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
/// <param name="areCorrect"><inheritdoc path="/param[@name='areCorrect']"/></param>
[method: JsonConstructor]
public sealed class AddModifiableGridDifference(CandidateMap candidates, bool areCorrect) : AddGridDifference(candidates, areCorrect)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "M+";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Modifiable;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.AddModifiable;


	/// <inheritdoc/>
	public override AddModifiableGridDifference Clone() => new(Candidates, AreCorrect);
}
