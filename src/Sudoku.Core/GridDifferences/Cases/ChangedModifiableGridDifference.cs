namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a list of modifiable digits is changed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
[method: JsonConstructor]
public sealed class ChangedModifiableGridDifference(CandidateMap candidates) : ChangedGridDifference(candidates)
{
	/// <inheritdoc/>
	public override string NotationPrefix => "M^";

	/// <inheritdoc/>
	public override CellState CellType => CellState.Modifiable;

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.ChangedModifiable;


	/// <inheritdoc/>
	public override ChangedModifiableGridDifference Clone() => new(Candidates);
}
