namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Implements a simple SAT solver using the DPLL algorithm with unit propagation.
/// For more information about DPLL algorithm, please visit
/// <see href="https://en.wikipedia.org/wiki/DPLL_algorithm">this link</see>.
/// </summary>
/// <param name="formula">Indicates the formula.</param>
public sealed partial class BacktrackingSolver([Field] ConjunctiveNormalFormFormula formula)
{
	/// <summary>
	/// Represents the assignment values. The result value only represents for 3 values:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>Represents <c>true</c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>Represents <c>false</c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Leaves unassigned</description>
	/// </item>
	/// </list>
	/// This array starts at index 1. Please use 1-based indexing to operate variables.
	/// </summary>
	private bool?[] _assignment = new bool?[formula.NumVars + 1];


	/// <summary>
	/// Try to find a satisfying assignment.
	/// </summary>
	public bool Solve() => Backtracking();

	/// <summary>
	/// Performs DPLL recursive method. DPLL recursive routine:
	/// <list type="number">
	/// <item>Perform unit propagation to simplify.</item>
	/// <item>If conflict, backtrack (return false).</item>
	/// <item>If all vars assigned, formula is satisfied.</item>
	/// <item>Otherwise, pick an unassigned variable and branch on true/false.</item>
	/// </list>
	/// </summary>
	private bool Backtracking()
	{
		if (!UnitPropagation())
		{
			return false; // Conflict detected.
		}

		var variable = GetUnassignedVar();
		if (variable == -1)
		{
			return true; // All variables assigned without conflict => SAT.
		}

		// Save state for backtracking.
		var snapshot = (bool?[])_assignment.Clone();

		// Try assigning 'variable' = true.
		_assignment[variable] = true;
		if (Backtracking())
		{
			return true;
		}

		// Backtrack and try 'variable' = false.
		_assignment = snapshot;
		_assignment[variable] = false;
		if (Backtracking())
		{
			return true;
		}

		// Both assignments led to conflict => unsatisfiable under current partial assignment.
		_assignment = snapshot;
		return false;
	}

	/// <summary>
	/// Unit propagation:
	/// Repeatedly scan for clauses where only one literal is unassigned and all others <see langword="false"/>,
	/// then assign that literal to <see langword="true"/> (to satisfy the clause).
	/// Returns <see langword="false"/> if a clause becomes unsatisfiable (all literals <see langword="false"/>).
	/// </summary>
	private bool UnitPropagation()
	{
		bool isChanged;
		do
		{
			isChanged = false;
			foreach (var clause in _formula)
			{
				var (unassignedCount, unassignedLit, clauseSatisfied) = (0, 0, false);

				// Check each literal in the clause.
				foreach (var literal in clause)
				{
					var variable = Math.Abs(literal);
					var sign = literal > 0;
					if (_assignment[variable] == sign)
					{
						clauseSatisfied = true; // Clause is already satisfied.
						break;
					}
					if (_assignment[variable] is null)
					{
						unassignedCount++;
						unassignedLit = literal;
					}
				}

				if (clauseSatisfied) continue;

				// If no unassigned lits left and none true => conflict.
				if (unassignedCount == 0)
				{
					return false;
				}

				// Exactly one unassigned literal => must be set to satisfy the clause.
				if (unassignedCount == 1)
				{
					var variable = Math.Abs(unassignedLit);
					var sign = unassignedLit > 0;
					_assignment[variable] = sign;
					isChanged = true;
				}
			}
		} while (isChanged);
		return true;
	}

	/// <summary>
	/// Find a variable index that has not been assigned yet (0).
	/// Returns -1 if all variables are assigned.
	/// </summary>
	private int GetUnassignedVar()
	{
		for (var i = 1; i <= _formula.NumVars; i++)
		{
			if (_assignment[i] is null)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// After solving, retrieve the assignment array
	/// (index: <c>variable</c> is either <see langword="true"/> or <see langword="false"/>).
	/// </summary>
	public bool?[] GetAssignmentStates() => _assignment;
}
