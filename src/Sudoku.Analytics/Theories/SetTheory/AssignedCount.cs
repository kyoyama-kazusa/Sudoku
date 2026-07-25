namespace Sudoku.Theories.SetTheory;

/// <summary>
/// Represents an encapsulated type that describes how many assigned values are filled into a certain pattern,
/// created by method <see cref="LogicReasoner.GetAssignedCount(ref readonly Logic)"/>.
/// </summary>
/// <param name="Min">Indicates the minimum number of a permutation.</param>
/// <param name="Max">Indicates the maximum number of a permutation.</param>
/// <seealso cref="LogicReasoner.GetAssignedCount(ref readonly Logic)"/>
[Union]
public readonly record struct AssignedCount(int Min, int Max) : IEqualityOperators<AssignedCount, AssignedCount, bool>, IUnion
{
	/// <summary>
	/// Initializes a <see cref="AssignedCount"/> instance via an <see cref="int"/> value.
	/// </summary>
	/// <param name="value">The value.</param>
	public AssignedCount(int value) : this(value, value)
	{
	}

	/// <summary>
	/// Initializes a <see cref="AssignedCount"/> instance via a pair of <see cref="int"/> values.
	/// </summary>
	/// <param name="value">The pair of <see cref="int"/> values.</param>
	public AssignedCount((int Min, int Max) value) : this(value.Min, value.Max)
	{
	}


	/// <inheritdoc/>
	public object? Value => IsStable ? Min : (Min, Max);

	/// <summary>
	/// Indicates whether the pattern is stable.
	/// </summary>
	public bool IsStable => Min == Max;

	/// <summary>
	/// Indicates the delta value.
	/// </summary>
	public int Delta => Max - Min;


	/// <summary>
	/// Try to get an <see cref="int"/> value if available.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool TryGetValue(out int value)
	{
		if (IsStable)
		{
			value = Min;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Try to get a pair of <see cref="int"/> values if available.
	/// </summary>
	/// <param name="value">The pair of values.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool TryGetValue(out (int Min, int Max) value)
	{
		if (IsStable)
		{
			value = default;
			return false;
		}
		value = (Min, Max);
		return true;
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => IsStable ? Min.ToString() : (Min, Max).ToString();
}
