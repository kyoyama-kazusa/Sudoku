namespace Sudoku.Theories.SetTheory;

/// <summary>
/// Represents an encapsulated type that describes how many assigned values are filled into a certain pattern,
/// created by method <see cref="LogicReasoner.GetAssignedCount(ref readonly Logic)"/>.
/// </summary>
/// <param name="Min">Indicates the minimum number of a permutation.</param>
/// <param name="Max">Indicates the maximum number of a permutation.</param>
/// <seealso cref="LogicReasoner.GetAssignedCount(ref readonly Logic)"/>
[Union]
public readonly partial record struct AssignedCount(int Min, int Max) :
	AssignedCount.IUnionMembers,
	IEqualityOperators<AssignedCount, AssignedCount, bool>
{
	/// <summary>
	/// Indicates whether the pattern is stable.
	/// </summary>
	public bool IsStable => Min == Max;

	/// <summary>
	/// Indicates the delta value.
	/// </summary>
	public int Delta => Max - Min;

	/// <inheritdoc/>
	object? IUnion.Value => IsStable ? Min : (Min, Max);


	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => IsStable ? Min.ToString() : (Min, Max).ToString();

	/// <inheritdoc/>
	bool IUnionMembers.TryGetValue(out int value)
	{
		if (IsStable)
		{
			value = Min;
			return true;
		}
		value = default;
		return false;
	}

	/// <inheritdoc/>
	bool IUnionMembers.TryGetValue(out (int Min, int Max) value)
	{
		if (IsStable)
		{
			value = default;
			return false;
		}
		value = (Min, Max);
		return true;
	}
}
