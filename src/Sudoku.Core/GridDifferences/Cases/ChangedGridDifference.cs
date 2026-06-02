namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a type of digits (given, value or candidate) is changed.
/// </summary>
/// <param name="candidates"><inheritdoc path="/param[@name='candidates']"/></param>
public abstract class ChangedGridDifference(CandidateMap candidates) : UpdatedGridDifference(candidates)
{
	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] GridDifference? other)
		=> other is ChangedGridDifference comparer
		&& Candidates == comparer.Candidates && EqualityContract == comparer.EqualityContract;

	/// <inheritdoc/>
	public sealed override int GetHashCode() => HashCode.Combine(EqualityContract, Candidates);
}
