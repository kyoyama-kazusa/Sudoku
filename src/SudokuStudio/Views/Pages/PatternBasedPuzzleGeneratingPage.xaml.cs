namespace SudokuStudio.Views.Pages;

/// <summary>
/// Represents a page that generates for pattern-based puzzles.
/// </summary>
public sealed partial class PatternBasedPuzzleGeneratingPage : Page
{
	/// <summary>
	/// Defines a user-defined view that will be used.
	/// </summary>
	private readonly ViewUnitBindableSource _userColoringView = new();


	/// <summary>
	/// Indicates the number of generating and generated puzzles.
	/// </summary>
	private int _generatingCount, _generatingFilteredCount;

	/// <summary>
	/// Indicates the cancellation token source for generating operations.
	/// </summary>
	private CancellationTokenSource? _ctsForGeneratingOperations;

	/// <summary>
	/// Defines a key-value pair of functions that is used for routing hotkeys.
	/// </summary>
	private (Hotkey Hotkey, Action Action)[] _hotkeyFunctions;


	/// <summary>
	/// Initializes a <see cref="PatternBasedPuzzleGeneratingPage"/> instance.
	/// </summary>
	public PatternBasedPuzzleGeneratingPage()
	{
		InitializeComponent();
		InitializeFields();
	}


	/// <summary>
	/// Indicates whether the generator is running.
	/// </summary>
	[DependencyProperty]
	internal partial bool IsGeneratorLaunched { get; set; }

	/// <summary>
	/// Indicates the progress.
	/// </summary>
	[DependencyProperty]
	internal partial double ProgressPercent { get; set; }

	/// <summary>
	/// Indicates the missing digit.
	/// </summary>
	[DependencyProperty]
	internal partial Digit MissingDigit { get; set; }

	/// <summary>
	/// Indicates all selected cells.
	/// </summary>
	[DependencyProperty]
	internal partial CellMap SelectedCells { get; set; }

	/// <summary>
	/// Indicates the fixed candidates.
	/// </summary>
	[DependencyProperty]
	internal partial CandidateMap FixedCandidates { get; set; }


	/// <inheritdoc/>
	protected override void OnKeyDown(KeyRoutedEventArgs e)
	{
		base.OnKeyDown(e);

		// This method routes the hotkeys.
		var modifierState = Keyboard.GetModifierStateForCurrentThread();
		foreach (var ((key, modifiers), action) in _hotkeyFunctions)
		{
			if (modifierState == modifiers && e.Key == key)
			{
				action();
				break;
			}
		}
		e.Handled = false;
	}

	/// <summary>
	/// Try to initialize field <see cref="_hotkeyFunctions"/>.
	/// </summary>
	/// <seealso cref="_hotkeyFunctions"/>
	[MemberNotNull(nameof(_hotkeyFunctions))]
	private void InitializeFields()
		=> _hotkeyFunctions = [
			(new(VirtualKey.C, VirtualKeyModifiers.Control), CopyPatternText),
			(new(VirtualKey.C, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift), async () => await CopySudokuGridControlAsSnapshotAsync()),
			(new(VirtualKey.V, VirtualKeyModifiers.Control), async () => await PastePatternTextAsync()),
		];

	/// <summary>
	/// Copy pattern text.
	/// </summary>
	private void CopyPatternText()
	{
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		dataPackage.SetText(SelectedCells.ToString(new BitmapCellMapFormatInfo()));
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// Copy grid text.
	/// </summary>
	private void CopyGridText()
	{
		var placeholderText = Application.Current.AsApp().Preference.UIPreferences.EmptyCellCharacter;
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		dataPackage.SetText(SudokuPane.Puzzle.ToString($"{placeholderText}"));
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// Copy snapshot.
	/// </summary>
	private async Task CopySudokuGridControlAsSnapshotAsync()
	{
		if (SudokuPane.FocusState == FocusState.Unfocused)
		{
			return;
		}

		// Creates the stream to store the output image data.
		var stream = new InMemoryRandomAccessStream();
		await OnSavingOrCopyingSudokuPanePictureSimpleAsync(stream);

		// Copies the data to the data package.
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		var streamRef = RandomAccessStreamReference.CreateFromStream(stream);
		dataPackage.SetBitmap(streamRef);

		// Copies to the clipboard.
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// Produces a copying/saving operation for pictures from sudoku pane <see cref="SudokuPane"/>.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the handling argument. This type should be <see cref="IRandomAccessStream"/> or <see cref="StorageFile"/>.
	/// </typeparam>
	/// <param name="obj">The argument.</param>
	/// <returns>A <see cref="Task"/> instance that contains details for the current asynchronous operation.</returns>
	/// <seealso cref="IRandomAccessStream"/>
	/// <seealso cref="StorageFile"/>
	private async Task OnSavingOrCopyingSudokuPanePictureSimpleAsync<T>(T obj) where T : class
	{
		if (Application.Current.AsApp().Preference.UIPreferences.TransparentBackground)
		{
			var color = App.CurrentTheme switch { ApplicationTheme.Light => Colors.White, _ => Colors.Black };
			SudokuPane.MainGrid.Background = new SolidColorBrush(color);
		}

		await SudokuPane.RenderToAsync(obj);

		if (Application.Current.AsApp().Preference.UIPreferences.TransparentBackground)
		{
			SudokuPane.MainGrid.Background = null;
		}
	}

	/// <summary>
	/// Paste the pattern text.
	/// </summary>
	private async Task PastePatternTextAsync()
	{
		if (SudokuPane.FocusState != FocusState.Unfocused
			&& Clipboard.GetContent() is var dataPackageView
			&& dataPackageView.Contains(StandardDataFormats.Text)
			&& await dataPackageView.GetTextAsync() is var targetText)
		{
			try
			{
				SelectedCells = CellMap.Parse(targetText, new BitmapCellMapFormatInfo());
			}
			catch (Exception ex) when (ex is InvalidOperationException or FormatException)
			{
			}
		}
	}


	[Callback]
	private static void MissingDigitPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is not (PatternBasedPuzzleGeneratingPage { MissingDigitSelector: var selector }, { NewValue: Digit newValue }))
		{
			return;
		}

		var control = selector.Items.First(item => item.Tag is Digit d && d == newValue);
		selector.SelectedItem = control;
	}

	[Callback]
	private static void SelectedCellsPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is not (
			PatternBasedPuzzleGeneratingPage
			{
				_userColoringView: var view,
				PatternCounter: var counterTextBlock,
				SudokuPane: var pane
			} page,
		{ NewValue: CellMap newValue }
		))
		{
			return;
		}

		var uiPref = Application.Current.AsApp().Preference.UIPreferences;
		var copied = view.Clone();
		var (a, r, g, b) = App.CurrentTheme switch { ApplicationTheme.Light => uiPref.ActiveCellColor, _ => uiPref.ActiveCellColor_Dark };
		copied.View.Clear();
		copied.View.AddRange(from cell in newValue select new CellViewNode(new ColorColorIdentifier(a, r, g, b), cell));

		pane.ViewUnit = copied;

		var p = SR.Get("PatternBasedPuzzleGeneratingPage_SelectedCellsCount", App.CurrentCulture);
		counterTextBlock.Text = $"{p}{newValue.Count}";
	}

	[Callback]
	private static void FixedCandidatesPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is not (PatternBasedPuzzleGeneratingPage { SudokuPane: var pane }, { NewValue: CandidateMap newValue }))
		{
			return;
		}

		var puzzle = Grid.Empty;
		foreach (var (_, cell, digit) in newValue.EnumerateCellDigit())
		{
			puzzle.SetDigit(cell, digit);
		}
		pane.Puzzle = puzzle;
	}


	private void SudokuPane_Loaded(object sender, RoutedEventArgs e)
	{
		var app = Application.Current.AsApp();
		app.CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);
		app.MainSudokuPane = SudokuPane;
	}

	private void SudokuPane_ActualThemeChanged(FrameworkElement sender, object args)
		=> Application.Current.AsApp().CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);

	private void SudokuPane_Clicked(SudokuPane sender, GridClickedEventArgs e)
	{
		if (e is not { Cell: var cell and not -1 })
		{
			return;
		}

		var newValue = SelectedCells;
		newValue.Toggle(cell);
		SelectedCells = newValue;
	}

	private void SudokuPane_DigitInput(SudokuPane sender, DigitInputEventArgs e)
	{
		// Update fixed candidates.
		var originalMap = FixedCandidates;
		if (e.DigitInput == -1)
		{
			originalMap.RemoveCell(e.Cell);
		}
		else
		{
			originalMap.Add(e.Candidate);
		}

		FixedCandidates = originalMap;
	}

	private async void GeneratingButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var (pattern, missingDigit, fixedCandidates) = (SelectedCells, MissingDigit, FixedCandidates);
		var ratingScale = Application.Current.AsApp().Preference.TechniqueInfoPreferences.RatingScale;
		var ratingScaleFormat = FactorMarshal.GetScaleFormatString(1 / ratingScale);
		var analyzer = Application.Current.AsApp().GetAnalyzerConfigured(SudokuPane)
			.WithUserDefinedOptions(new() { IsDirectMode = true });
		using var cts = new CancellationTokenSource();
		try
		{
			(_ctsForGeneratingOperations, IsGeneratorLaunched, _generatingCount, _generatingFilteredCount) = (cts, true, 0, 0);
			if (await Task.Run(() => taskEntry(analyzer, cts.Token)) is
				{
					Grid: { IsUndefined: false } grid,
					Result: { DifficultyLevel: var difficultyLevel, MaxDifficulty: var rating, TotalDifficulty: var ratingSum }
				})
			{
				SudokuPane.Puzzle = grid;
				//SudokuPane.ViewUnit = null; // This is expected to be removed because we should always show view nodes.
				RatingDisplayer.Text = string.Format(
					SR.Get("PatternBasedPuzzleGeneratingPage_RatingInfo", App.CurrentCulture),
					[
						(rating / ratingScale).ToString(ratingScaleFormat),
						(ratingSum / ratingScale).ToString(ratingScaleFormat),
						difficultyLevel.GetName(App.CurrentCulture)
					]
				);
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_ctsForGeneratingOperations = null;
			IsGeneratorLaunched = false;
		}


		static bool checkGrid(ref Grid grid, in CandidateMap fixedCandidates)
		{
			if (!fixedCandidates)
			{
				return true;
			}

			var cellsMap = fixedCandidates.CellDistribution;
			foreach (var cell in fixedCandidates.EnumerateCells())
			{
				grid.SwapDigit(grid.GetSolutionGrid().GetDigit(cell), BitOperations.Log2(cellsMap[cell]));
			}
			foreach (var cell in fixedCandidates.EnumerateCells())
			{
				if (grid.GetSolutionGrid().GetDigit(cell) != BitOperations.Log2(cellsMap[cell]))
				{
					return false;
				}
			}
			return true;
		}

		static AnalysisResult getAnalyzedResult(Analyzer analyzer, in Grid grid, CancellationToken cancellationToken)
		{
			lock (AnalyzingRelatedSyncRoot)
			{
				return analyzer.Analyze(grid, cancellationToken: cancellationToken);
			}
		}

		static TProgressDataProvider createProgress<TProgressDataProvider>(ref int total, int filtered)
			where TProgressDataProvider :
				struct,
				IEquatable<TProgressDataProvider>,
				IProgressDataProvider<TProgressDataProvider>,
				allows ref struct
			=> TProgressDataProvider.Create(++total, filtered);

		void reportCallback<TProgressDataProvider>(TProgressDataProvider progress)
			where TProgressDataProvider :
				struct,
				IEquatable<TProgressDataProvider>,
				IProgressDataProvider<TProgressDataProvider>,
				allows ref struct
		{
			var str = progress.ToDisplayString();
			DispatcherQueue.TryEnqueue(() => AnalyzeStepSearcherNameLabel.Text = str);
		}

		(Grid Grid, AnalysisResult Result) taskEntry(Analyzer analyzer, CancellationToken cancellationToken)
		{
			var generator = new PatternBasedPuzzleGenerator(in pattern, missingDigit);
			var progress = new SelfReportingProgress<GeneratorProgress>(reportCallback);
			while (true)
			{
				var grid = generator.Generate(cancellationToken: cancellationToken);
				if (checkGrid(ref grid, fixedCandidates))
				{
					return (grid, getAnalyzedResult(analyzer, grid, cancellationToken));
				}

				progress.Report(createProgress<GeneratorProgress>(ref _generatingCount, _generatingFilteredCount));
				cancellationToken.ThrowIfCancellationRequested();
			}
		}
	}

	private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
		=> MissingDigit = (Digit)sender.SelectedItem.Tag!;

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		MissingDigit = -1;

		var p = SR.Get("PatternBasedPuzzleGeneratingPage_SelectedCellsCount", App.CurrentCulture);
		PatternCounter.Text = $"{p}0";
	}

	private void CancelOperationButton_Click(object sender, RoutedEventArgs e) => _ctsForGeneratingOperations?.Cancel();

	private void CopyButton_Click(object sender, RoutedEventArgs e) => CopyPatternText();

	private async void CopyPictureButton_ClickAsync(object sender, RoutedEventArgs e) => await CopySudokuGridControlAsSnapshotAsync();

	private async void PasteButton_ClickAsync(object sender, RoutedEventArgs e) => await PastePatternTextAsync();

	private void Dialog_AreYouSureToReturnToEmpty_ActionButtonClick(TeachingTip sender, object args)
	{
		SudokuPane.Puzzle = Grid.Empty;
		SudokuPane.ViewUnit = null;
		SelectedCells = [];
		FixedCandidates = [];
		Dialog_AreYouSureToReturnToEmpty.IsOpen = false;
	}

	private void CopyPuzzleButton_Click(object sender, RoutedEventArgs e) => CopyGridText();

	private void GoToAnalyzePageButton_Click(object sender, RoutedEventArgs e)
		=> App.GetMainWindow(this).NavigateToPage(typeof(AnalyzePage), SudokuPane.Puzzle);

	private void ClearCellButton_Click(object sender, RoutedEventArgs e)
	{
		if (!IsGeneratorLaunched)
		{
			SelectedCells = [];
		}
	}

	private void ClearDigitButton_Click(object sender, RoutedEventArgs e)
	{
		if (!IsGeneratorLaunched)
		{
			FixedCandidates = [];
		}
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e) => Dialog_AreYouSureToReturnToEmpty.IsOpen = true;
}
