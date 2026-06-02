namespace Sudoku.GridDifferences;

/// <summary>
/// Represents a kind of difference between grids.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(NothingChangedGridDifference), (int)GridDifferenceType.NothingChanged)]
[JsonDerivedType(typeof(ResetGridDifference), (int)GridDifferenceType.Reset)]
[JsonDerivedType(typeof(AddGivenGridDifference), (int)GridDifferenceType.AddGiven)]
[JsonDerivedType(typeof(AddModifiableGridDifference), (int)GridDifferenceType.AddModifiable)]
[JsonDerivedType(typeof(AddCandidateGridDifference), (int)GridDifferenceType.AddCandidate)]
[JsonDerivedType(typeof(RemoveGivenGridDifference), (int)GridDifferenceType.RemoveGiven)]
[JsonDerivedType(typeof(RemoveModifiableGridDifference), (int)GridDifferenceType.RemoveModifiable)]
[JsonDerivedType(typeof(RemoveCandidateGridDifference), (int)GridDifferenceType.RemoveCandidate)]
[JsonDerivedType(typeof(ChangedGivenGridDifference), (int)GridDifferenceType.ChangedGiven)]
[JsonDerivedType(typeof(ChangedModifiableGridDifference), (int)GridDifferenceType.ChangedModifiable)]
[JsonDerivedType(typeof(ChangedCandidateGridDifference), (int)GridDifferenceType.ChangedCandidate)]
public abstract class GridDifference :
	ICloneable,
	IEquatable<GridDifference>,
	IEqualityOperators<GridDifference, GridDifference, bool>
{
	/// <summary>
	/// Indicates the notation prefix.
	/// </summary>
	public virtual string NotationPrefix => Notation[0].ToString();

	/// <summary>
	/// Indicates the simplified notation of the current difference result.
	/// </summary>
	public abstract string Notation { get; }

	/// <summary>
	/// Indicates the type of the difference.
	/// </summary>
	public abstract GridDifferenceType Type { get; }

	/// <summary>
	/// Indicates the target type.
	/// </summary>
	protected Type EqualityContract => GetType();


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as GridDifference);

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] GridDifference? other);

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract GridDifference Clone();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <inheritdoc/>
	public static bool operator ==(GridDifference? left, GridDifference? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(GridDifference? left, GridDifference? right) => !(left == right);
}
