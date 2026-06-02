namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference result that describes candidates are reset.
/// </summary>
[method: JsonConstructor]
public sealed class ResetGridDifference() : GridDifference
{
	/// <inheritdoc/>
	public override string Notation => "R";

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.Reset;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] GridDifference? other) => other is ResetGridDifference;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(typeof(ResetGridDifference));

	/// <inheritdoc/>
	public override ResetGridDifference Clone() => new();
}
