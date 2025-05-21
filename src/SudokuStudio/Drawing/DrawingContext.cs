namespace SudokuStudio.Drawing;

/// <summary>
/// Represents drawing context.
/// </summary>
/// <param name="sudokuPane"><inheritdoc cref="SudokuPane" path="/summary"/></param>
/// <param name="controlAddingActions"><inheritdoc cref="ControlAddingActions" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
internal readonly ref struct DrawingContext(SudokuPane sudokuPane, AnimatedResultCollection controlAddingActions)
{
	/// <summary>
	/// Indicates the sudoku pane.
	/// </summary>
	public SudokuPane SudokuPane { get; } = sudokuPane;

	/// <summary>
	/// Indicates the control adding actions. The collection can be used by playing animation.
	/// </summary>
	public AnimatedResultCollection ControlAddingActions { get; } = controlAddingActions;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out SudokuPane pane, out AnimatedResultCollection actions)
		=> (pane, actions) = (SudokuPane, ControlAddingActions);
}
