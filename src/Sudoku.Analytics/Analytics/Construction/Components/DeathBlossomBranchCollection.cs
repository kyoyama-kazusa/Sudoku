namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a collection that stores a list of branches, grouped by its key specified as type parameter <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <typeparam name="TKey">The type of the distinction key.</typeparam>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode, GetHashCodeBehavior = GetHashCodeBehavior.MakeAbstract)]
public abstract partial class DeathBlossomBranchCollection<TSelf, TKey> :
	Dictionary<TKey, AlmostLockedSetPattern>,
	IComponent,
	IEquatable<TSelf>
	where TSelf : DeathBlossomBranchCollection<TSelf, TKey>, IEquatable<TSelf>, IEqualityOperators<TSelf, TSelf, bool>, new()
	where TKey : notnull, IAdditiveIdentity<TKey, TKey>, IEquatable<TKey>, IEqualityOperators<TKey, TKey, bool>, new()
{
	/// <summary>
	/// Indicates all digits used.
	/// </summary>
	public Mask DigitsMask
	{
		get
		{
			var result = (Mask)0;
			foreach (var branch in Values)
			{
				result |= branch.DigitsMask;
			}
			return result;
		}
	}

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.DeathBlossomBranch;


	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] TSelf? other);
}
