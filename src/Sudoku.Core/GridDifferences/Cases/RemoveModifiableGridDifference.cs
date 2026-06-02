namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of modifiable digits is removed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class RemoveModifiableGridDifference(CandidateMap candidates) : RemoveGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "M-";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Modifiable;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.RemoveModifiable;


	/// <inheritdoc/>
	public override RemoveModifiableGridDifference Clone() => new(Candidates);
}
