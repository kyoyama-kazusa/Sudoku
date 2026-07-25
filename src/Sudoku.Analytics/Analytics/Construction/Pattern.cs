namespace Sudoku.Analytics.Construction;

/// <summary>
/// Represents a pattern that describes a technique, describing cells and digits used in a puzzle.
/// </summary>
public closed class Pattern :
	ICloneable,
	IConstructible<PatternType>,
	IEquatable<Pattern>,
	IEqualityOperators<Pattern, Pattern, bool>
{
	/// <summary>
	/// Indicates whether the current pattern can be used as a node inside a chain pattern <see cref="Chain"/>,
	/// represented as a relation in a link, stored in <see cref="Link.GroupedLinkPattern"/>.
	/// </summary>
	/// <seealso cref="Chain"/>
	/// <seealso cref="Link.GroupedLinkPattern"/>
	public abstract bool IsChainingCompatible { get; }

	/// <inheritdoc/>
	public abstract PatternType Type { get; }


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Pattern);

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] Pattern? other);

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract Pattern Clone();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <inheritdoc/>
	public static bool operator ==(Pattern? left, Pattern? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(Pattern? left, Pattern? right) => !(left == right);
}
