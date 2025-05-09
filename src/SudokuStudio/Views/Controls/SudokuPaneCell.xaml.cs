namespace SudokuStudio.Views.Controls;

/// <summary>
/// Defines a cell displayed in a <see cref="SudokuPane"/>.
/// </summary>
/// <seealso cref="SudokuPane"/>
internal sealed partial class SudokuPaneCell : UserControl
{
	/// <summary>
	/// Indicates the internal digit displayers.
	/// </summary>
	internal List<UIElement> _internalDigitDisplayers;


	/// <summary>
	/// Initializes a <see cref="SudokuPaneCell"/> instance.
	/// </summary>
	public SudokuPaneCell()
	{
		InitializeComponent();
		InitializeAnimationFeedbackIfWorth();
	}


	/// <summary>
	/// Indicates the duration time of fthe feedback on house completed.
	/// </summary>
	[DependencyProperty(DefaultValue = 800)]
	public partial int HouseCompletedFeedbackDuration { get; set; }

	/// <summary>
	/// Indicates the base pane.
	/// </summary>
	public SudokuPane BasePane { get; set; } = null!;

	/// <summary>
	/// Indicates the cell index.
	/// </summary>
	public Cell CellIndex { get; internal init; }

	/// <summary>
	/// Indicates the candidates mask.
	/// </summary>
	[DependencyProperty(DefaultValue = Grid.MaxCandidatesMask)]
	public partial Mask CandidatesMask { get; set; }

	/// <summary>
	/// Indicates the cell state.
	/// </summary>
	[DependencyProperty(DefaultValue = CellState.Empty)]
	public partial CellState State { get; set; }


	/// <summary>
	/// Light up the background of the cell.
	/// </summary>
	/// <param name="duration">The duration.</param>
	/// <remarks><i>
	/// This method returns <see langword="void"/> on purpose because we do not want to make the method wait-able by user.
	/// </i></remarks>
	public async void LightUpAsync(Millisecond duration)
	{
		if (!Application.Current.AsApp().Preference.UIPreferences.EnableAnimationFeedback)
		{
			return;
		}

		ValueSurrounder.Background = new SolidColorBrush(BasePane.HouseCompletedFeedbackColor);
		await duration.ms;
		ValueSurrounder.Background = null;
	}

	/// <summary>
	/// Try to initialize for animation feedback if worth.
	/// </summary>
	[MemberNotNull(nameof(_internalDigitDisplayers))]
	private void InitializeAnimationFeedbackIfWorth()
	{
		_internalDigitDisplayers = [
			ValueTextBlock,
			Candidate0TextBlock,
			Candidate1TextBlock,
			Candidate2TextBlock,
			Candidate3TextBlock,
			Candidate4TextBlock,
			Candidate5TextBlock,
			Candidate6TextBlock,
			Candidate7TextBlock,
			Candidate8TextBlock
		];

		if (!Application.Current.AsApp().Preference.UIPreferences.EnableAnimationFeedback)
		{
			return;
		}

		_internalDigitDisplayers.ForEach(static e => e.OpacityTransition = new());
		ValueSurrounder.BackgroundTransition = new() { Duration = TimeSpan.FromMilliseconds(HouseCompletedFeedbackDuration) };
	}


	[Callback]
	private static void HouseCompletedFeedbackDurationPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((SudokuPaneCell)d).ValueSurrounder.BackgroundTransition.Duration = TimeSpan.FromMilliseconds((int)e.NewValue);


	private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e) => BasePane.SelectedCell = CellIndex;

	private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e) => BasePane.SelectedCell = -1;

	private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e) => BasePane.SelectedCell = CellIndex;

	private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e) => BasePane.SelectedCell = CellIndex;

	private void TextBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
	{
		if (!BasePane.EnableDoubleTapFilling)
		{
			return;
		}

		if ((this, sender) is not ({ BasePane: { Puzzle: var modified, SelectedCell: var cell and not -1 } }, TextBlock { Text: var text }))
		{
			return;
		}

		if (!Digit.TryParse(text, out var originalDigit)
			|| originalDigit - 1 is not (var digit and >= 0 and < 9)
			|| modified.GetState(cell) != CellState.Empty
			|| (modified.GetCandidates(cell) >> digit & 1) == 0)
		{
			return;
		}

		modified.SetDigit(cell, digit);
		BasePane.SetPuzzleInternal(modified, PuzzleUpdatingMethod.UserUpdating);

		BasePane.TriggerGridUpdated(GridUpdatedBehavior.Assignment, cell * 9 + digit);
	}

	private void TextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
	{
		if ((this, sender) is not ({ BasePane: { Puzzle: var modified, SelectedCell: var cell and not -1 } }, TextBlock { Text: var text }))
		{
			return;
		}

		if (!Digit.TryParse(text, out var originalDigit) || originalDigit - 1 is not (var digit and >= 0 and < 9))
		{
			return;
		}

		BasePane.TriggerClicked(MouseButton.Right, cell * 9 + digit);

		if (!BasePane.EnableRightTapRemoving || modified.GetState(cell) != CellState.Empty || (modified.GetCandidates(cell) >> digit & 1) == 0)
		{
			return;
		}

		modified.SetExistence(cell, digit, false);
		BasePane.SetPuzzleInternal(modified, PuzzleUpdatingMethod.UserUpdating);

		BasePane.TriggerGridUpdated(GridUpdatedBehavior.Elimination, cell * 9 + digit);
	}

	private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
	{
		if ((this, sender) is not ({ BasePane.SelectedCell: var cell and not -1 }, TextBlock { Text: var text }))
		{
			return;
		}

		if (!Digit.TryParse(text, out var originalDigit) || originalDigit - 1 is not (var digit and >= 0 and < 9))
		{
			return;
		}

		BasePane.TriggerClicked(MouseButton.Left, cell * 9 + digit);
	}
}
