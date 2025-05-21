namespace Sudoku.Runtime.InteropServices.SudokuExplainer;

/// <summary>
/// Defines a range of difficulty rating value that is applied to a technique implemented by Sudoku Explainer.
/// </summary>
/// <param name="min"><inheritdoc cref="Min" path="/summary"/></param>
/// <param name="max"><inheritdoc cref="Max" path="/summary"/></param>
public readonly struct SudokuExplainerRatingRange(Half min, Half max)
{
	/// <summary>
	/// Initializes a <see cref="SudokuExplainerRatingRange"/> instance via the specified difficulty rating value.
	/// </summary>
	/// <param name="min">The difficulty rating value.</param>
	public SudokuExplainerRatingRange(Half min) : this(min, min)
	{
	}


	/// <summary>
	/// Indicates whether the current range is a real range, i.e. property <see cref="Max"/> holds different value
	/// with <see cref="Min"/>.
	/// </summary>
	public bool IsRange => Max != Min;

	/// <summary>
	/// Indicates the minimum possible value.
	/// </summary>
	public Half Min { get; } = min;

	/// <summary>
	/// Indicates the maximum possible value.
	/// </summary>
	public Half Max { get; } = max;
}
