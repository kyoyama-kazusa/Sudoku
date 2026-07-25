namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that checks with quantifiers (i.e. "any" or "all").
/// </summary>
/// <param name="constraints"><inheritdoc cref="Constraints" path="/summary"/></param>
public closed class QuantifierConstraint(params ConstraintCollection constraints) : Constraint
{
	/// <summary>
	/// Indicates the constraints.
	/// </summary>
	public ConstraintCollection Constraints { get; } = constraints;
}
