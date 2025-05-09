namespace SudokuStudio.Views.Controls;

/// <summary>
/// Defines a sudoku pane control.
/// </summary>
public sealed partial class SudokuPane : UserControl, INotifyPropertyChanged
{
	[Default]
	private static readonly decimal HighlightCandidateCircleScaleDefaultValue = .9M;

	[Default]
	private static readonly decimal HighlightBackgroundOpacityDefaultValue = .15M;

	[Default]
	private static readonly decimal ChainStrokeThicknessDefaultValue = 2.0M;

	[Default]
	private static readonly decimal GivenFontScaleDefaultValue = 1.0M;

	[Default]
	private static readonly decimal ModifiableFontScaleDefaultValue = 1.0M;

	[Default]
	private static readonly decimal PencilmarkFontScaleDefaultValue = .33M;

	[Default]
	private static readonly decimal BabaGroupLabelFontScaleDefaultValue = .6M;

	[Default]
	private static readonly decimal CoordinateLabelFontScaleDefaultValue = .4M;

	[Default]
	private static readonly Color GivenColorDefaultValue = Colors.Black;

	[Default]
	private static readonly Color DeltaCandidateColorDefaultValue = Color.FromArgb(255, 255, 185, 185);

	[Default]
	private static readonly Color BorderColorDefaultValue = Colors.Black;

	[Default]
	private static readonly Color CursorBackgroundColorDefaultValue = Colors.Blue with { A = 32 };

	[Default]
	private static readonly Color LinkColorDefaultValue = Colors.Red;

	[Default]
	private static readonly Color NormalColorDefaultValue = Color.FromArgb(255, 63, 218, 101);

	[Default]
	private static readonly Color AssignmentColorDefaultValue = Color.FromArgb(255, 63, 218, 101);

	[Default]
	private static readonly Color OverlappedAssignmentColorDefaultValue = Color.FromArgb(255, 0, 255, 204);

	[Default]
	private static readonly Color EliminationColorDefaultValue = Color.FromArgb(255, 255, 118, 132);

	[Default]
	private static readonly Color CannibalismColorDefaultValue = new() { A = 255, R = 235 };

	[Default]
	private static readonly Color ExofinColorDefaultValue = Color.FromArgb(255, 127, 187, 255);

	[Default]
	private static readonly Color EndofinColorDefaultValue = Color.FromArgb(255, 216, 178, 255);

	[Default]
	private static readonly Color GroupedNodeStrokeColorDefaultValue = Colors.Yellow with { A = 64 };

	[Default]
	private static readonly Color GroupedNodeBackgroundColorDefaultValue = Colors.Orange;

	[Default]
	private static readonly Color HouseCompletedFeedbackColorDefaultValue = Colors.HotPink;

	[Default]
	private static readonly DashArray StrongLinkDashStyleDefaultValue = [];

	[Default]
	private static readonly DashArray WeakLinkDashStyleDefaultValue = [3, 1.5];

	[Default]
	private static readonly Thickness CellsInnerPaddingDefaultValue = new(6);

	[Default]
	private static readonly CornerRadius CellsInnerCornerRadiusDefaultValue = new(6);

	[Default]
	private static readonly FontFamily GivenFontDefaultValue = new("Tahoma");

	[Default]
	private static readonly FontFamily ModifiableFontDefaultValue = new("Tahoma");

	[Default]
	private static readonly FontFamily PencilmarkFontDefaultValue = new("Tahoma");

	[Default]
	private static readonly FontFamily CoordinateLabelFontDefaultValue = new("Tahoma");

	[Default]
	private static readonly FontFamily BabaGroupLabelFontDefaultValue = new("Times New Roman");

	[Default]
	private static readonly ColorPalette AuxiliaryColorsDefaultValue = [
		Color.FromArgb(255, 255, 192, 89),
		Color.FromArgb(255, 127, 187, 255),
		Color.FromArgb(255, 216, 178, 255)
	];

	[Default]
	private static readonly ColorPalette DifficultyLevelForegroundsDefaultValue = [
		Color.FromArgb(255, 0, 51, 204),
		Color.FromArgb(255, 0, 102, 0),
		Color.FromArgb(255, 102, 51, 0),
		Color.FromArgb(255, 102, 51, 0),
		Color.FromArgb(255, 102, 0, 0),
		Colors.Black
	];

	[Default]
	private static readonly ColorPalette DifficultyLevelBackgroundsDefaultValue = [
		Color.FromArgb(255, 204, 204, 255),
		Color.FromArgb(255, 100, 255, 100),
		Color.FromArgb(255, 255, 255, 100),
		Color.FromArgb(255, 255, 150, 80),
		Color.FromArgb(255, 255, 100, 100),
		Color.FromArgb(255, 220, 220, 220)
	];

	[Default]
	private static readonly ColorPalette UserDefinedColorPaletteDefaultValue = [
		Color.FromArgb(255, 63, 218, 101), // Green
		Color.FromArgb(255, 255, 192, 89), // Orange
		Color.FromArgb(255, 127, 187, 255), // Sky-blue
		Color.FromArgb(255, 216, 178, 255), // Purple
		Color.FromArgb(255, 197, 232, 140), // Yellow-green
		Color.FromArgb(255, 255, 203, 203), // Light red
		Color.FromArgb(255, 178, 223, 223), // Blue green
		Color.FromArgb(255, 252, 220, 165), // Light orange
		Color.FromArgb(255, 255, 255, 150), // Yellow
		Color.FromArgb(255, 247, 222, 143), // Golden yellow
		Color.FromArgb(255, 220, 212, 252), // Purple
		Color.FromArgb(255, 255, 118, 132), // Red
		Color.FromArgb(255, 206, 251, 237), // Light sky-blue
		Color.FromArgb(255, 215, 255, 215), // Light green
		Color.FromArgb(255, 192, 192, 192) // Gray
	];

	[Default]
	private static readonly ColorPalette AlmostLockedSetsColorsDefaultValue = [
		Color.FromArgb(255, 255, 203, 203),
		Color.FromArgb(255, 178, 223, 223),
		Color.FromArgb(255, 252, 220, 165),
		Color.FromArgb(255, 255, 255, 150),
		Color.FromArgb(255, 247, 222, 143)
	];

	[Default]
	private static readonly ColorPalette RectangleColorsDefaultValue = [
		Color.FromArgb(255, 216, 178, 255), // Purple
		Color.FromArgb(255, 204, 150, 248), // Purple
		Color.FromArgb(255, 114, 82, 170), // Dark purple
	];


	/// <summary>
	/// Indicates the temporary selected cell.
	/// </summary>
	internal Cell _temporarySelectedCell = -1;

	/// <summary>
	/// Defines a pair of stacks that stores undo steps.
	/// This field can be used when <see cref="EnableUndoRedoStacking"/> is <see langword="true"/>.
	/// </summary>
	/// <seealso cref="EnableUndoRedoStacking"/>
	internal ObservableStack<Grid>? _undoStack;

	/// <summary>
	/// Defines a pair of stacks that stores redo steps.
	/// This field can be used when <see cref="EnableUndoRedoStacking"/> is <see langword="true"/>.
	/// </summary>
	/// <seealso cref="EnableUndoRedoStacking"/>
	internal ObservableStack<Grid>? _redoStack;

	/// <summary>
	/// The easy entry to visit children <see cref="SudokuPaneCell"/> instances. This field contains 81 elements,
	/// indicating controls being displayed as 81 cells in a sudoku grid respectively.
	/// </summary>
	/// <remarks>
	/// Although this field is not marked as <see langword="readonly"/>, it will only be initialized during initialization.
	/// <b>Please do not modify any elements in this array.</b>
	/// </remarks>
	internal SudokuPaneCell[] _children;

	/// <summary>
	/// Indicates the internal compositor.
	/// </summary>
	private readonly Compositor _compositor = CompositionTarget.GetCompositorForCurrentThread();

	/// <summary>
	/// The backing field of property <see cref="Puzzle"/>.
	/// </summary>
	private Grid _puzzle;

	/// <summary>
	/// Indicates the spring animation.
	/// </summary>
	private SpringVector3NaturalMotionAnimation? _springAnimation;


	/// <summary>
	/// Initializes a <see cref="SudokuPane"/> instance.
	/// </summary>
	public SudokuPane()
	{
		InitializeComponent();
		InitializeChildrenControls();

		_puzzle = Grid.Empty;

		UpdateCellData(_puzzle);
		InitializeEvents();
	}


	/// <summary>
	/// Indicates whether the pane displays for candidates.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool DisplayCandidates { get; set; }

	/// <summary>
	/// Indicates whether the pane displays cursors that uses different colors to highlight some cells as peers
	/// of the target cell that is the one your mouse points to.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool DisplayCursors { get; set; }

	/// <summary>
	/// Indicates whether the pane displays for delta Digits using different colors.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool UseDifferentColorToDisplayDeltaDigits { get; set; }

	/// <summary>
	/// Indicates whether the pane disable flyout open.
	/// </summary>
	[DependencyProperty]
	public partial bool DisableFlyout { get; set; }

	/// <summary>
	/// Indicates whether the pane prevent the simple conflict, which means,
	/// if you input a digit that is conflict with the Digits in its containing houses,
	/// this pane will do nothing by this value being <see langword="true"/>.
	/// If not, the pane won't check for any conflict and always allow you inputting the digit regardless of possible conflict.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool PreventConflictingInput { get; set; }

	/// <summary>
	/// Indicates whether the pane enables for undoing and redoing operation.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	[MemberNotNullWhen(true, nameof(_redoStack), nameof(_undoStack))]
	public partial bool EnableUndoRedoStacking { get; set; }

	/// <summary>
	/// Indicates whether the digit will be automatically input by double tapping a candidate.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableDoubleTapFilling { get; set; }

	/// <summary>
	/// Indicates whether the digit will be removed (eliminated) from the containing cell by tapping a candidate using right mouse button.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableRightTapRemoving { get; set; }

	/// <summary>
	/// Indicates whether sudoku pane enables for animation feedback.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool EnableAnimationFeedback { get; set; }

	/// <summary>
	/// Indicates whether sudoku pane does not use background color to display a sudoku puzzle.
	/// </summary>
	[DependencyProperty]
	public partial bool TransparentBackground { get; set; }

	/// <summary>
	/// Indicates the scale of highlighted candidate circles. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal HighlightCandidateCircleScale { get; set; }

	/// <summary>
	/// Indicates the opacity of the background highlighted elements. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal HighlightBackgroundOpacity { get; set; }

	/// <summary>
	/// Indicates the chain stroke thickness.
	/// </summary>
	[DependencyProperty]
	public partial decimal ChainStrokeThickness { get; set; }

	/// <summary>
	/// Indicates the font scale of given Digits. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal GivenFontScale { get; set; }

	/// <summary>
	/// Indicates the font scale of modifiable Digits. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal ModifiableFontScale { get; set; }

	/// <summary>
	/// Indicates the font scale of pencilmark Digits (candidates). The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal PencilmarkFontScale { get; set; }

	/// <summary>
	/// Indicates the font scale of baba group characters. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal BabaGroupLabelFontScale { get; set; }

	/// <summary>
	/// Indicates the coordinate label font scale. The value should generally be below 1.0.
	/// </summary>
	[DependencyProperty]
	public partial decimal CoordinateLabelFontScale { get; set; }

	/// <summary>
	/// Indicates the duration of feedback when a house is completed.
	/// </summary>
	[DependencyProperty(DefaultValue = 800)]
	public partial int HouseCompletedFeedbackDuration { get; set; }

	/// <summary>
	/// Indicates the displaying kind of coordinate labels.
	/// </summary>
	[DependencyProperty(DefaultValue = CoordinateType.RxCy)]
	public partial CoordinateType CoordinateLabelDisplayKind { get; set; }

	/// <summary>
	/// Indicates the displaying mode of coordinate labels.
	/// </summary>
	/// <remarks>
	/// For more information please visit <see cref="CoordinateLabelDisplay"/>.
	/// </remarks>
	/// <seealso cref="CoordinateLabelDisplay"/>
	[DependencyProperty(DefaultValue = CoordinateLabelDisplay.UpperAndLeft)]
	public partial CoordinateLabelDisplay CoordinateLabelDisplayMode { get; set; }

	/// <summary>
	/// Indicates the displaying mode of candidate view nodes.
	/// </summary>
	[DependencyProperty(DefaultValue = CandidateViewNodeDisplay.CircleSolid)]
	public partial CandidateViewNodeDisplay CandidateViewNodeDisplayMode { get; set; }

	/// <summary>
	/// Indicates the displaying mode of an assignment.
	/// </summary>
	[DependencyProperty(DefaultValue = AssignmentDisplay.CircleSolid)]
	public partial AssignmentDisplay AssignmentDisplayMode { get; set; }

	/// <summary>
	/// Indicates the displaying mode of an elimination.
	/// </summary>
	[DependencyProperty(DefaultValue = EliminationDisplay.CircleSolid)]
	public partial EliminationDisplay EliminationDisplayMode { get; set; }

	/// <summary>
	/// Indicates the currently selected cell.
	/// </summary>
	[DependencyProperty]
	public partial Cell SelectedCell { get; set; }

	/// <summary>
	/// Indicates the core-operating sudoku puzzle.
	/// </summary>
	public Grid Puzzle
	{
		get => _puzzle;

		set
		{
			SetPuzzleInternal(value, PuzzleUpdatingMethod.Programmatic, true);
			GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Replacing, value));
		}
	}

	/// <summary>
	/// Indicates the value that describes corner radius for displaying cell view nodes.
	/// </summary>
	[DependencyProperty]
	public partial CornerRadius CellsInnerCornerRadius { get; set; }

	/// <summary>
	/// Indicates the given color.
	/// </summary>
	[DependencyProperty]
	public partial Color GivenColor { get; set; }

	/// <summary>
	/// Indicates the modifiable color.
	/// </summary>
	[DependencyProperty]
	public partial Color ModifiableColor { get; set; }

	/// <summary>
	/// Indicates the pencilmark color.
	/// </summary>
	[DependencyProperty]
	public partial Color PencilmarkColor { get; set; }

	/// <summary>
	/// Indicates the coordinate label color.
	/// </summary>
	[DependencyProperty]
	public partial Color CoordinateLabelColor { get; set; }

	/// <summary>
	/// Indicates the baba group label color.
	/// </summary>
	[DependencyProperty]
	public partial Color BabaGroupLabelColor { get; set; }

	/// <summary>
	/// Indicates the color that is used for displaying candidates that are wrongly removed, but correct.
	/// </summary>
	[DependencyProperty]
	public partial Color DeltaCandidateColor { get; set; }

	/// <summary>
	/// Indicates the color that is used for displaying cell Digits that are wrongly filled.
	/// </summary>
	[DependencyProperty]
	public partial Color DeltaCellColor { get; set; }

	/// <summary>
	/// Indicates the border color.
	/// </summary>
	[DependencyProperty]
	public partial Color BorderColor { get; set; }

	/// <summary>
	/// Indicates the cursor background color.
	/// </summary>
	[DependencyProperty]
	public partial Color CursorBackgroundColor { get; set; }

	/// <summary>
	/// Indicates the link color.
	/// </summary>
	[DependencyProperty]
	public partial Color LinkColor { get; set; }

	/// <summary>
	/// Indicates the normal color.
	/// </summary>
	[DependencyProperty]
	public partial Color NormalColor { get; set; }

	/// <summary>
	/// Indicates the assignment color.
	/// </summary>
	[DependencyProperty]
	public partial Color AssignmentColor { get; set; }

	/// <summary>
	/// Indicates the overlapped assignment color.
	/// </summary>
	[DependencyProperty]
	public partial Color OverlappedAssignmentColor { get; set; }

	/// <summary>
	/// Indicates the elimination color.
	/// </summary>
	[DependencyProperty]
	public partial Color EliminationColor { get; set; }

	/// <summary>
	/// Indicates the cannibalism color.
	/// </summary>
	[DependencyProperty]
	public partial Color CannibalismColor { get; set; }

	/// <summary>
	/// Indicates the exofin color.
	/// </summary>
	[DependencyProperty]
	public partial Color ExofinColor { get; set; }

	/// <summary>
	/// Indicates the endofin color.
	/// </summary>
	[DependencyProperty]
	public partial Color EndofinColor { get; set; }

	/// <summary>
	/// Indicates the grouped node stroke color.
	/// </summary>
	[DependencyProperty]
	public partial Color GroupedNodeStrokeColor { get; set; }

	/// <summary>
	/// Indicates the grouped node background color.
	/// </summary>
	[DependencyProperty]
	public partial Color GroupedNodeBackgroundColor { get; set; }

	/// <summary>
	/// Indicates the feedback color when a house is completed.
	/// </summary>
	[DependencyProperty]
	public partial Color HouseCompletedFeedbackColor { get; set; }

	/// <summary>
	/// Indicates the dash style of the strong links.
	/// </summary>
	[DependencyProperty]
	public partial DashArray StrongLinkDashStyle { get; set; }

	/// <summary>
	/// Indicates the dash style of the weak links.
	/// </summary>
	[DependencyProperty]
	public partial DashArray WeakLinkDashStyle { get; set; }

	/// <summary>
	/// Indicates the value that describes the padding value displaying cell view nodes.
	/// </summary>
	[DependencyProperty]
	public partial Thickness CellsInnerPadding { get; set; }

	/// <summary>
	/// Indicates the given font.
	/// </summary>
	[DependencyProperty]
	public partial FontFamily GivenFont { get; set; }

	/// <summary>
	/// Indicates the modifiable font.
	/// </summary>
	[DependencyProperty]
	public partial FontFamily ModifiableFont { get; set; }

	/// <summary>
	/// Indicates the candidate font.
	/// </summary>
	[DependencyProperty]
	public partial FontFamily PencilmarkFont { get; set; }

	/// <summary>
	/// Indicates the coordinate label font.
	/// </summary>
	[DependencyProperty]
	public partial FontFamily CoordinateLabelFont { get; set; }

	/// <summary>
	/// Indicates the baba group label font.
	/// </summary>
	[DependencyProperty]
	public partial FontFamily BabaGroupLabelFont { get; set; }

	/// <summary>
	/// Indicates the view unit used.
	/// </summary>
	[DependencyProperty]
	public partial ViewUnitBindableSource? ViewUnit { get; set; }

	/// <summary>
	/// Indicates the auxiliary colors.
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette AuxiliaryColors { get; set; }

	/// <summary>
	/// Indicates the foreground colors of all 6 kinds of difficulty levels.
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette DifficultyLevelForegrounds { get; set; }

	/// <summary>
	/// Indicates the background colors of all 6 kinds of difficulty levels.
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette DifficultyLevelBackgrounds { get; set; }

	/// <summary>
	/// Indicates the user-defined colors used by customized views.
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette UserDefinedColorPalette { get; set; }

	/// <summary>
	/// Indicates the colors applied to technique pattern Almost Locked Sets.
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette AlmostLockedSetsColors { get; set; }

	/// <summary>
	/// Indicates the colors applied to technique pattern Rectangles (URs and ARs).
	/// </summary>
	[DependencyProperty]
	public partial ColorPalette RectangleColors { get; set; }

	/// <summary>
	/// Indicates the approximately-measured width and height value of a cell.
	/// </summary>
	internal double ApproximateCellWidth => ((Width + Height) / 2 - 100 - (4 << 1)) / 10;

	/// <summary>
	/// Indicates the solution of property <see cref="Puzzle"/>.
	/// </summary>
	/// <seealso cref="Puzzle"/>
	internal Grid Solution => _puzzle.SolutionGrid;


	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Indicates the event that is triggered when a file is successfully to be received via dropped file.
	/// </summary>
	public event ReceivedDroppedFileSuccessfullyEventHandler? ReceivedDroppedFileSuccessfully;

	/// <summary>
	/// Indicates the event that is triggered when a file is failed to be received via dropped file.
	/// </summary>
	public event ReceivedDroppedFileFailedEventHandler? ReceivedDroppedFileFailed;

	/// <summary>
	/// Indicates the event that is triggered when a digit is input (that cause a change in a cell).
	/// </summary>
	public event DigitInputEventHandler? DigitInput;

	/// <summary>
	/// Indicates the event that is triggered when the internal grid is updated with the specified behavior,
	/// such as removed a candidate, filled with a digit, etc..
	/// </summary>
	public event GridUpdatedEventHandler? GridUpdated;

	/// <summary>
	/// Indicates the event that is triggered when the mouse wheel is changed.
	/// </summary>
	public event SudokuPaneMouseWheelChangedEventHandler? MouseWheelChanged;

	/// <summary>
	/// Indicates the event that is triggered when a candidate is clicked.
	/// This event can be also used for checking the clicked cell, house, chute, etc..
	/// </summary>
	public event GridClickedEventHandler? Clicked;

	/// <summary>
	/// Indicates the event that is triggered when "displaying candidates" option is toggled.
	/// </summary>
	public event CandidatesDisplayingToggledEventHandler? CandidatesDisplayingToggled;

	/// <summary>
	/// Indicates the event that is triggered when a house is completed.
	/// </summary>
	public event HouseCompletedEventHandler? HouseCompleted;

	/// <summary>
	/// Indicates the event that is triggered when the current puzzle is completed.
	/// </summary>
	public event PuzzleCompletedEventHandler? PuzzleCompleted;

	/// <summary>
	/// Indicates the event that is triggered when caching.
	/// </summary>
	public event EventHandler? Caching;


	/// <summary>
	/// Undo a step. This method requires member <see cref="EnableUndoRedoStacking"/> be <see langword="true"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Throws when the property <see cref="EnableUndoRedoStacking"/> is not <see langword="true"/>.
	/// </exception>
	public void UndoStep()
	{
		if (!EnableUndoRedoStacking)
		{
			throw new InvalidOperationException($"Undoing operation requires property '{nameof(EnableUndoRedoStacking)}' be true value.");
		}

		if (_undoStack.Count == 0)
		{
			// No more steps can be undone.
			return;
		}

		_redoStack.Push(_puzzle);

		var target = _undoStack.Pop();

		SetPuzzleCore(target, new(PuzzleUpdatingMethod.UserUpdating, false, true));

		GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Undoing, target));
	}

	/// <summary>
	/// Redo a step. This method requires member <see cref="EnableUndoRedoStacking"/> be <see langword="true"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Throws when the property <see cref="EnableUndoRedoStacking"/> is not <see langword="true"/>.
	/// </exception>
	public void RedoStep()
	{
		if (!EnableUndoRedoStacking)
		{
			throw new InvalidOperationException($"Redoing operation requires property '{nameof(EnableUndoRedoStacking)}' be true value.");
		}

		if (_redoStack.Count == 0)
		{
			// No more steps can be redone.
			return;
		}

		_undoStack.Push(_puzzle);

		var target = _redoStack.Pop();

		SetPuzzleCore(target, new(PuzzleUpdatingMethod.UserUpdating, false, true));

		GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Redoing, target));
	}

	/// <summary>
	/// Try to set a digit into a cell, or delete a digit from a cell.
	/// </summary>
	/// <param name="cell">The cell to be set or delete.</param>
	/// <param name="digit">The digit to be set or delete.</param>
	/// <param name="isSet">Indicates whether the operation is to set a digit into the target cell.</param>
	public void SetOrDeleteDigit(Cell cell, Digit digit, bool isSet)
	{
		var puzzle = Puzzle;
		var conclusion = (Conclusion)(isSet ? new(Assignment, cell, digit) : new(Elimination, cell, digit));
		puzzle.Apply(conclusion);

		SetPuzzleInternal(puzzle, PuzzleUpdatingMethod.UserUpdating);
		GridUpdated?.Invoke(this, new(isSet ? GridUpdatedBehavior.Assignment : GridUpdatedBehavior.Elimination, cell * 9 + digit));
	}

	/// <summary>
	/// Try to update grid state.
	/// </summary>
	/// <param name="newGrid">The new grid to be used for assigning to the target.</param>
	public void UpdateGrid(in Grid newGrid) => SetPuzzleInternal(newGrid, PuzzleUpdatingMethod.Programmatic);

	/// <summary>
	/// <para>Triggers <see cref="GridUpdated"/> event.</para>
	/// <para>This method can only be used by internal control type <see cref="SudokuPaneCell"/> or the current type scope.</para>
	/// </summary>
	/// <param name="behavior">The behavior.</param>
	/// <param name="value">The new value to assign.</param>
	/// <seealso cref="SudokuPaneCell"/>
	internal void TriggerGridUpdated(GridUpdatedBehavior behavior, object value) => GridUpdated?.Invoke(this, new(behavior, value));

	/// <summary>
	///	<para>Triggers <see cref="Clicked"/> event.</para>
	///	<para><inheritdoc cref="TriggerGridUpdated(GridUpdatedBehavior, object)" path="//summary/para[2]"/></para>
	/// </summary>
	/// <param name="mouseButton">Indicates the mouse button clicked.</param>
	/// <param name="candidate">The candidate.</param>
	internal void TriggerClicked(MouseButton mouseButton, Candidate candidate) => Clicked?.Invoke(this, new(mouseButton, candidate));

	/// <summary>
	/// <para>Try to set puzzle, with a <see cref="bool"/> value indicating whether undoing and redoing stacks should be cleared.</para>
	/// <para><inheritdoc cref="TriggerGridUpdated(GridUpdatedBehavior, object)" path="//summary/para[2]"/></para>
	/// </summary>
	/// <param name="value">The newer grid.</param>
	/// <param name="method">The updating method.</param>
	/// <param name="clearStack">
	/// Indicates whether undoing and redoing stacks should be cleared. The default value is <see langword="false"/>.
	/// </param>
	/// <seealso cref="SudokuPaneCell"/>
	internal void SetPuzzleInternal(in Grid value, PuzzleUpdatingMethod method, bool clearStack = false)
		=> SetPuzzleCore(value, new(method, clearStack, false));

	/// <summary>
	/// To initialize children controls for <see cref="_children"/>.
	/// </summary>
	[MemberNotNull(nameof(_children))]
	private void InitializeChildrenControls()
	{
		_children = new SudokuPaneCell[81];
		for (var i = 0; i < 81; i++)
		{
			var cellControl = new SudokuPaneCell { CellIndex = i, BasePane = this };

			GridLayout.SetRow(cellControl, i / 9 + 2);
			GridLayout.SetColumn(cellControl, i % 9 + 2);

			MainGrid.Children.Add(cellControl);
			_children[i] = cellControl;
		}
	}

	/// <summary>
	/// To initializes for stack events.
	/// </summary>
	private void InitializeEvents()
	{
		if (EnableUndoRedoStacking)
		{
			(_undoStack = []).Changed += _ => PropertyChanged?.Invoke(this, new(nameof(_undoStack)));
			(_redoStack = []).Changed += _ => PropertyChanged?.Invoke(this, new(nameof(_redoStack)));
		}

		if (EnableAnimationFeedback)
		{
			HouseCompleted += static (sender, e) => sender.OnHouseCompletedAsync(e);
			Clicked += static (sender, e) => sender.ValueClicked(e.MouseButton, e.Cell);
		}
	}

	/// <summary>
	/// The default handler that is used by <see cref="HouseCompleted"/> event.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	private async void OnHouseCompletedAsync(HouseCompletedEventArgs e)
	{
		if (e.Method == PuzzleUpdatingMethod.Programmatic)
		{
			return;
		}

		foreach (var cells in e.LastCell.GetCellsOrdered(e.House).ToArray())
		{
			cells.ForEach(cell => _children[cell].LightUpAsync(250));
			await 100.ms;
		}
	}

	/// <summary>
	/// Update scaling for <see cref="SudokuPaneCell"/> controls where the corresponding cell is value.
	/// </summary>
	/// <param name="button">The clicked button.</param>
	/// <param name="cell">The cell to be checked.</param>
	private void ValueClicked(MouseButton button, Cell cell)
	{
		if (button != MouseButton.Left)
		{
			return;
		}

		if (_puzzle.GetDigit(cell) is not (var digit and not -1))
		{
			return;
		}

		var a = new List<TextBlock>();
		var b = new List<TextBlock>();
		for (var c = 0; c < 81; c++)
		{
			var textBlock = _children[c].ValueTextBlock;
			if (_puzzle.GetState(c) != CellState.Empty)
			{
				(_puzzle.GetDigit(c) == digit ? a : b).Add(textBlock);
			}
		}

		ResetScale();
		a.ForEach(textBlock => updateScale(1.6F, textBlock));
		b.ForEach(textBlock => updateScale(1.0F, textBlock));


		void updateScale(float value, TextBlock textBlock)
		{
			CreateOrUpdateSpringAnimation(value, .4F);
			textBlock.StartAnimation(_springAnimation);
		}
	}

	/// <summary>
	/// Try to reset scale if worth.
	/// </summary>
	private void ResetScale()
	{
		for (var c = 0; c < 81; c++)
		{
			if (_children[c].ValueTextBlock is { Scale: not { X: 1.0F, Y: 1.0F, Z: 1.0F } } textBlock)
			{
				CreateOrUpdateSpringAnimation(1.0F, .4F);
				textBlock.StartAnimation(_springAnimation);
			}
		}
	}

	/// <summary>
	/// Create or update spring animation.
	/// </summary>
	/// <param name="finalValue">The value to be set. The value will be used for scaling the control.</param>
	/// <param name="dampingRatio">The value indicating how much damping is applied to the spring.</param>
	[MemberNotNull(nameof(_springAnimation))]
	private void CreateOrUpdateSpringAnimation(float finalValue, float dampingRatio)
	{
		if (_springAnimation is null)
		{
			_springAnimation = _compositor.CreateSpringVector3Animation();
			_springAnimation.Target = nameof(Scale);
		}

		_springAnimation.FinalValue = new(finalValue);
		_springAnimation.DampingRatio = dampingRatio;
	}

	/// <summary>
	/// To initialize <see cref="_children"/> values via the specified grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <seealso cref="_children"/>
	private void UpdateCellData(in Grid grid)
	{
		for (var i = 0; i < 81; i++)
		{
			var cellControl = _children[i];
			cellControl.State = grid.GetState(i);
			cellControl.CandidatesMask = grid.GetCandidates(i);
		}
	}

	/// <summary>
	/// Try to update the puzzle value by using the specified newer <see cref="Grid"/> instance,
	/// and two parameters indicating the details of the current updating operation.
	/// </summary>
	/// <param name="value">
	/// <inheritdoc cref="SetPuzzleInternal(in Grid, PuzzleUpdatingMethod, bool)" path="/param[@name='value']"/>
	/// </param>
	/// <param name="data">The details of updating.</param>
	private void SetPuzzleCore(in Grid value, GridUpdatingDetails data)
	{
		var (method, clearStack, whileUndoingOrRedoing) = data;

		// Pushes the grid into the stack if worth.
		if (!whileUndoingOrRedoing && !clearStack && EnableUndoRedoStacking)
		{
			_undoStack.Push(_puzzle);
		}

		// Check whether a house is going to be completed.
		var housesToBeCompleted = value.CompletedHouses & ~_puzzle.CompletedHouses;
		var lastCells = new List<Cell>(BitOperations.PopCount(housesToBeCompleted));
		foreach (var houseToBeCompleted in housesToBeCompleted)
		{
			foreach (var cell in HousesCells[houseToBeCompleted])
			{
				if (_puzzle.GetState(cell) == CellState.Empty && value.GetState(cell) != CellState.Empty)
				{
					lastCells.Add(cell);
					break;
				}
			}
		}

		// Assigns the puzzle.
		_puzzle = value;

		UpdateCellData(value);
		switch (clearStack, whileUndoingOrRedoing)
		{
			case (true, _) when EnableUndoRedoStacking:
			{
				_undoStack.Clear();
				_redoStack.Clear();
				break;
			}
			case (false, false) when EnableUndoRedoStacking:
			{
				_redoStack.Clear();
				break;
			}
		}

		// Reset scale.
		if (EnableAnimationFeedback)
		{
			ResetScale();
		}

		// Triggers the event.
		PropertyChanged?.Invoke(this, new(nameof(Puzzle)));

		var houses = housesToBeCompleted.AllSets;
		for (var i = 0; i < houses.Length; i++)
		{
			HouseCompleted?.Invoke(this, new(lastCells[i], houses[i], method));
		}

		if (value.IsSolved)
		{
			PuzzleCompleted?.Invoke(this, new(value));
		}
	}


	[Callback]
	private static void DisplayCandidatesPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SudokuPane pane)
		{
			pane.CandidatesDisplayingToggled?.Invoke(pane, new());
			UpdateViewUnitControls(pane); // Update view nodes no matter whether the event is triggered.
		}
	}

	[Callback]
	private static void ViewUnitPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SudokuPane pane)
		{
			IncrementalUpdateViewUnitControls(pane, (ViewUnitBindableSource?)e.OldValue, (ViewUnitBindableSource?)e.NewValue);
			pane.Caching?.Invoke(pane, EventArgs.Empty);
		}
	}

	[Callback]
	private static void BabaGroupLabelColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void LinkColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void NormalColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void AssignmentColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void OverlappedAssignmentColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void EliminationColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void CannibalismColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void ExofinColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void EndofinColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void GroupedNodeStrokeColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void GroupedNodeBackgroundColorPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void BabaGroupLabelFontPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void AuxiliaryColorsPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void UserDefinedColorPalettePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void AlmostLockedSetsColorsPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void StrongLinkDashStylePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void WeakLinkDashStylePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void HighlightCandidateCircleScalePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void HighlightBackgroundOpacityPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void ChainStrokeThicknessPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void CandidateViewNodeDisplayModePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void EliminationDisplayModePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void AssignmentDisplayModePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> UpdateViewUnitControls((SudokuPane)d);

	[Callback]
	private static void HouseCompletedFeedbackDurationPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		foreach (var element in ((SudokuPane)d)._children)
		{
			element.HouseCompletedFeedbackDuration = (int)e.NewValue;
		}
	}

	[Callback]
	private static void CoordinateLabelDisplayKindPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is not CoordinateType value)
		{
			return;
		}

		var i = 0;
		foreach (var element in ((SudokuPane)d).MainGrid.Children)
		{
			if (element is not TextBlock t)
			{
				continue;
			}

			t.Text = CoordinateLabelConversion.ToCoordinateLabelText(value, i % 9, i < 18);
			i++;
		}
	}


	private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e) => Focus(FocusState.Programmatic);

	private void UserControl_DragOver(object sender, DragEventArgs e)
	{
		e.AcceptedOperation = DataPackageOperation.Copy;
		e.DragUIOverride.Caption = SR.Get("SudokuPane_DropSudokuFileHere", App.CurrentCulture);
		e.DragUIOverride.IsCaptionVisible = true;
		e.DragUIOverride.IsContentVisible = true;
	}

	private async void UserControl_DropAsync(object sender, DragEventArgs e)
	{
		if (e.DataView is not { } dataView)
		{
			return;
		}

		if (!dataView.Contains(StandardDataFormats.StorageItems))
		{
			return;
		}

		switch (await dataView.GetStorageItemsAsync())
		{
			case [StorageFolder folder]:
			{
				var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery, 0, 2);
				if (files is not [StorageFile { FileType: FileExtensions.Text or FileExtensions.PlainText } file])
				{
					return;
				}

				await handleSudokuFileAsync(file);
				break;
			}
			case [StorageFile { FileType: FileExtensions.Text or FileExtensions.PlainText } file]:
			{
				await handleSudokuFileAsync(file);
				break;
			}
		}


		async Task handleSudokuFileAsync(StorageFile file)
		{
			var filePath = file.Path;
			var fileInfo = new FileInfo(filePath);
			switch (fileInfo.Length)
			{
				case 0:
				{
					ReceivedDroppedFileFailed?.Invoke(this, new(ReceivedDroppedFileFailedReason.FileIsEmpty));
					return;
				}
				case > 1024 * 64:
				{
					ReceivedDroppedFileFailed?.Invoke(this, new(ReceivedDroppedFileFailedReason.FileIsTooLarge));
					return;
				}
				default:
				{
					switch (io::Path.GetExtension(filePath))
					{
						case FileExtensions.PlainText:
						{
							var content = await FileIO.ReadTextAsync(file);
							if (string.IsNullOrWhiteSpace(content))
							{
								ReceivedDroppedFileFailed?.Invoke(this, new(ReceivedDroppedFileFailedReason.FileIsEmpty));
								return;
							}

							if (!Grid.TryParse(content, out var g))
							{
								ReceivedDroppedFileFailed?.Invoke(this, new(ReceivedDroppedFileFailedReason.FileCannotBeParsed));
								return;
							}

							ReceivedDroppedFileSuccessfully?.Invoke(this, new(filePath, new() { BaseGrid = g }));
							break;
						}
						case FileExtensions.Text:
						{
							Action eventHandler = SudokuFileHandler.Read(filePath) switch
							{
								[var gridInfo] => () => ReceivedDroppedFileSuccessfully?.Invoke(this, new(filePath, gridInfo)),
								_ => () => ReceivedDroppedFileFailed?.Invoke(this, new(ReceivedDroppedFileFailedReason.FileCannotBeParsed))
							};
							eventHandler();
							break;
						}
					}
					break;
				}
			}
		}
	}

	private void UserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
	{
		var pointerPoint = e.GetCurrentPoint((UIElement)sender);
		if (pointerPoint.Properties.MouseWheelDelta is not (var delta and not 0))
		{
			return;
		}

		MouseWheelChanged?.Invoke(this, new(delta < 0));

		e.Handled = true;
	}

	private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		switch (Keyboard.GetModifierStateForCurrentThread(), SelectedCell, e.Key, Keyboard.GetInputDigit(e.Key))
		{
			// Invalid cases.
			default:
			case (_, not (>= 0 and < 81), _, _):
			case var (_, cell, _, _) when Puzzle.GetState(cell) == CellState.Given:
			case (_, _, _, -2):
			{
				return;
			}

			// Clear cell.
			case ({ AllFalse: true }, var cell, _, -1):
			{
				var args = new DigitInputEventArgs(cell, -1);
				DigitInput?.Invoke(this, args);
				if (args.Cancel)
				{
					return;
				}

				var modified = Puzzle;
				modified.SetDigit(cell, -1);
				GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Clear, cell));
				SetPuzzleInternal(modified, PuzzleUpdatingMethod.UserUpdating);
				break;
			}

			// Clear a candidate from a cell.
			case ((false, true, false, false), var cell, _, var digit) when Puzzle.Exists(cell, digit) is true:
			{
				var modified = Puzzle;
				modified.SetExistence(cell, digit, false);
				SetPuzzleInternal(modified, PuzzleUpdatingMethod.UserUpdating);
				GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Elimination, cell * 9 + digit));
				break;
			}

			// Set a cell with a digit.
			case ({ AllFalse: true }, var cell, _, var digit)
			when PreventConflictingInput && !Puzzle.ConflictWith(cell, digit) || !PreventConflictingInput:
			{
				var args = new DigitInputEventArgs(cell, digit);
				DigitInput?.Invoke(this, args);
				if (args.Cancel)
				{
					return;
				}

				var modified = Puzzle;
				if (Puzzle.GetState(cell) == CellState.Modifiable)
				{
					// Temporarily re-compute candidates.
					modified.SetDigit(cell, -1);
				}

				modified.SetDigit(cell, digit);
				SetPuzzleInternal(modified, PuzzleUpdatingMethod.UserUpdating);
				GridUpdated?.Invoke(this, new(GridUpdatedBehavior.Assignment, cell * 9 + digit));
				break;
			}
		}
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	public static ReadOnlySpan<CellMap> GetCellsOrdered(this Cell @this, House house)
	{
		var cells = HousesCells[house];
		switch (house.HouseType)
		{
			case HouseType.Row:
			{
				var pos = Array.FindIndex(cells, cell => cell % 9 == @this % 9);
				var result = new List<CellMap>(5);
				for (var i = 1; ; i++)
				{
					var map = CellMap.Empty;
					if (pos >= i)
					{
						map.Add(cells[pos - i]);
					}
					if (pos + i < cells.Length)
					{
						map.Add(cells[pos + i]);
					}

					if (map)
					{
						result.AddRef(map);
						continue;
					}
					break;
				}
				return result.AsSpan();
			}
			case HouseType.Column:
			{
				var pos = Array.FindIndex(cells, cell => cell / 9 == @this / 9);
				var result = new List<CellMap>(5);
				for (var i = 1; ; i++)
				{
					var map = CellMap.Empty;
					if (pos >= i)
					{
						map.Add(cells[pos - i]);
					}
					if (pos + i < cells.Length)
					{
						map.Add(cells[pos + i]);
					}

					if (map)
					{
						result.AddRef(map);
						continue;
					}
					break;
				}
				return result.AsSpan();
			}
			case HouseType.Block:
			{
				return from cell in cells select cell.AsCellMap();
			}
			default:
			{
				throw new();
			}
		}
	}
}
