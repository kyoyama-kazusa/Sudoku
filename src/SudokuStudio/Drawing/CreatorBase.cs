namespace SudokuStudio.Drawing;

/// <summary>
/// Extracted type that creates <see cref="Shape"/>-related instances.
/// </summary>
/// <typeparam name="TInput">The type of input values.</typeparam>
/// <typeparam name="TOutput">The type of output values.</typeparam>
/// <param name="pane"><inheritdoc cref="Pane" path="/summary"/></param>
/// <param name="converter"><inheritdoc cref="Converter" path="/summary"/></param>
/// <seealso cref="Shape"/>
internal abstract class CreatorBase<TInput, TOutput>(SudokuPane pane, SudokuPanePositionConverter converter)
{
	/// <summary>
	/// Indicates the square root of 2.
	/// </summary>
	protected const double SqrtOf2 = 1.4142135623730951;

	/// <summary>
	/// Indicates the rotate angle (45 degrees).
	/// </summary>
	protected const double RotateAngle = PI / 4;


	/// <summary>
	/// Indicates the sudoku pane control.
	/// </summary>
	public SudokuPane Pane { get; } = pane;

	/// <summary>
	/// Indicates the position converter.
	/// </summary>
	public SudokuPanePositionConverter Converter { get; } = converter;


	/// <summary>
	/// Creates a list of <see cref="Shape"/> instances representing grouped nodes.
	/// </summary>
	/// <param name="nodes">The link view nodes.</param>
	/// <returns>A <see cref="Shape"/> instance.</returns>
	public abstract ReadOnlySpan<TOutput> CreateShapes(ReadOnlySpan<TInput> nodes);
}
