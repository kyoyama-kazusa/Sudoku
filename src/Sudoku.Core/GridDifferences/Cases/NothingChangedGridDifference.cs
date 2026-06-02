namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a result that describes two grids are same.
/// </summary>
[method: JsonConstructor]
public sealed class NothingChangedGridDifference() : GridDifference
{
	/// <inheritdoc/>
	public override string Notation => "N";

	/// <inheritdoc/>
	public override GridDifferenceType Type => GridDifferenceType.NothingChanged;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] GridDifference? other) => other is NothingChangedGridDifference;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(typeof(NothingChangedGridDifference));

	/// <inheritdoc/>
	public override NothingChangedGridDifference Clone() => new();
}
