namespace Sudoku.GridDifferences.Cases;

/// <summary>
/// Represents a difference that describes a type of digits (given, value or candidate) is added.
/// </summary>
/// <param name="candidates">
/// <inheritdoc cref="UpdatedGridDifference(CandidateMap)" path="/param[@name='candidates']"/>
/// </param>
/// <param name="areCorrect"><inheritdoc cref="AreCorrect" path="/summary"/></param>
public closed class AddGridDifference(CandidateMap candidates, bool areCorrect) : UpdatedGridDifference(candidates)
{
	/// <summary>
	/// Indicates whether the digits are correct to be added.
	/// </summary>
	public bool AreCorrect { get; } = areCorrect;


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] GridDifference? other)
		=> other is AddGridDifference comparer
		&& Candidates == comparer.Candidates && AreCorrect == comparer.AreCorrect
		&& EqualityContract == comparer.EqualityContract;

	/// <inheritdoc/>
	public sealed override int GetHashCode() => HashCode.Combine(EqualityContract, Candidates, AreCorrect);
}
