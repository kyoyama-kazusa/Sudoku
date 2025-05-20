namespace Sudoku.Inferring;

/// <summary>
/// Indicates the result value after <see cref="DeadlyPatternInferrer.TryInfer(in Grid, out DeadlyPatternInferredResult)"/> called.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="isDeadlyPattern"><inheritdoc cref="IsDeadlyPattern" path="/summary"/></param>
/// <param name="failedCases"><inheritdoc cref="FailedCases" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc cref="PatternCandidates" path="/summary"/></param>
/// <seealso cref="DeadlyPatternInferrer.TryInfer(in Grid, out DeadlyPatternInferredResult)"/>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct DeadlyPatternInferredResult(
	in Grid grid,
	bool isDeadlyPattern,
	ReadOnlySpan<Grid> failedCases,
	scoped in CandidateMap patternCandidates
)
{
	/// <summary>
	/// Indicates the grid used.
	/// </summary>
	public readonly ref readonly Grid Grid = ref grid;


	/// <summary>
	/// Indicates the pattern is a real deadly pattern.
	/// </summary>
	public bool IsDeadlyPattern { get; } = isDeadlyPattern;

	/// <summary>
	/// Indicates all possible failed cases. The value can be an empty sequence if the pattern is a real deadly pattern,
	/// or not a deadly pattern but containing obvious invalid candidates (like containing given or modifiable cells).
	/// </summary>
	public ReadOnlySpan<Grid> FailedCases { get; } = failedCases;

	/// <summary>
	/// Indicates the candidates the pattern used.
	/// </summary>
	public CandidateMap PatternCandidates { get; } = patternCandidates;
}
