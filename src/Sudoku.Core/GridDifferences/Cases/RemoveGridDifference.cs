namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a type of digits (given, value or candidate) is removed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
public closed class RemoveGridDifference(CandidateMap candidates) : UpdatedGridDifference(candidates)
{
	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] GridDifference? other)
		=> other is RemoveGridDifference comparer
		&& Candidates == comparer.Candidates && EqualityContract == comparer.EqualityContract;

	/// <inheritdoc/>
	public sealed override int GetHashCode() => HashCode.Combine(EqualityContract, Candidates);
}
