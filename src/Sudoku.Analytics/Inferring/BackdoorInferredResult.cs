namespace Sudoku.Inferring;

/// <summary>
/// Indicates the result value after <see cref="BackdoorInferrer.TryInfer(in Grid, out BackdoorInferredResult)"/> called.
/// </summary>
/// <param name="candidates"><inheritdoc cref="Candidates" path="/summary"/></param>
/// <seealso cref="BackdoorInferrer.TryInfer(in Grid, out BackdoorInferredResult)"/>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct BackdoorInferredResult(ReadOnlySpan<Conclusion> candidates)
{
	/// <summary>
	/// Indicates the found candidates.
	/// </summary>
	public ReadOnlySpan<Conclusion> Candidates { get; } = candidates;
}
