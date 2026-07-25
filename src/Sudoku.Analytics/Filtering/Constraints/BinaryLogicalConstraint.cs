namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that checks for two constraints.
/// </summary>
/// <param name="constraint1"><inheritdoc cref="Constraint1" path="/summary"/></param>
/// <param name="constraint2"><inheritdoc cref="Constraint2" path="/summary"/></param>
public closed class BinaryLogicalConstraint(Constraint constraint1, Constraint constraint2) : LogicalConstraint
{
	/// <summary>
	/// Indicates the constraint #1.
	/// </summary>
	public Constraint Constraint1 { get; } = constraint1;

	/// <summary>
	/// Indicates the constraint #2.
	/// </summary>
	public Constraint Constraint2 { get; } = constraint2;
}
