namespace SudokuStudio.Views.Pages;

/// <summary>
/// Defines a new page that stores a set of controls to analyze a sudoku grid.
/// </summary>
public sealed partial class AnalyzePage : Page
{
	/// <summary>
	/// Indicates the default thickness.
	/// </summary>
	private static readonly Thickness DefaultMarginForAnalyzerPages = new(10);


	/// <summary>
	/// Indicates the cancellation token source used by analyzing-related operations.
	/// </summary>
	internal CancellationTokenSource? _ctsForAnalyzingRelatedOperations;

	/// <summary>
	/// Represents a list of <see cref="Type"/> instances, for operation pages.
	/// </summary>
	private readonly Type[] _operationPageTypes = [
		typeof(BasicOperation),
		typeof(GeneratingOperation),
		typeof(AttributeCheckingOperation),
		typeof(ShuffleOperation),
		typeof(PrintingOperation)
	];

	/// <summary>
	/// Indicates the previously-selected index of operation page.
	/// </summary>
	private int _previouslySelectedIndexOfOperationPage;

	/// <summary>
	/// Indicates the tab routing data.
	/// </summary>
	private AnalyzeTabPageBindableSource[] _tabsRoutingData;

	/// <summary>
	/// Defines a key-value pair of functions that is used for routing hotkeys.
	/// </summary>
	private (Hotkey Hotkey, Action Action)[] _hotkeyFunctions;


	/// <summary>
	/// Initializes an <see cref="AnalyzePage"/> instance.
	/// </summary>
	public AnalyzePage()
	{
		InitializeComponent();
		InitializeFields();
		LoadInitialGrid();
	}


	/// <summary>
	/// Indicates whether the analyzer is launched.
	/// </summary>
	[DependencyProperty]
	internal partial bool IsAnalyzerLaunched { get; set; }

	/// <summary>
	/// Indicates whether the collector is launched.
	/// </summary>
	[DependencyProperty]
	internal partial bool IsGathererLaunched { get; set; }

	/// <summary>
	/// Indicates whether the generator is launched.
	/// </summary>
	[DependencyProperty]
	internal partial bool IsGeneratorLaunched { get; set; }

	/// <summary>
	/// Indicates the progress percent value.
	/// </summary>
	[DependencyProperty]
	internal partial double ProgressPercent { get; set; }

	/// <summary>
	/// Indicates the current index of the view of property <see cref="IDrawable.Views"/> displayed.
	/// </summary>
	/// <seealso cref="IDrawable.Views"/>
	[DependencyProperty(DefaultValue = -1)]
	internal partial int CurrentViewIndex { get; set; }

	/// <summary>
	/// Indicates the input character that is used as a baba group variable.
	/// </summary>
	[DependencyProperty]
	internal partial string? BabaGroupNameInput { get; set; }

	/// <summary>
	/// Indicates the selected drawing mode.
	/// </summary>
	[DependencyProperty(DefaultValue = DrawingMode.Cell)]
	internal partial DrawingMode SelectedMode { get; set; }

	/// <summary>
	/// Indicates the link type.
	/// </summary>
	[DependencyProperty(DefaultValue = Inference.Strong)]
	internal partial Inference LinkKind { get; set; }

	/// <summary>
	/// Indicates the analysis result cache.
	/// </summary>
	[DependencyProperty]
	internal partial AnalysisResult? AnalysisResultCache { get; set; }

	/// <summary>
	/// Indicates the visual unit.
	/// </summary>
	[DependencyProperty]
	internal partial IDrawable? VisualUnit { get; set; }


	/// <summary>
	/// Provides with an event that is triggered when failed to open a file.
	/// </summary>
	public event OpenFileFailedEventHandler? OpenFileFailed;

	/// <summary>
	/// Provides with an event that is triggered when failed to save a file.
	/// </summary>
	public event SaveFileFailedEventHandler? SaveFileFailed;


	/// <summary>
	/// To clear all possible data from the analyze tabs.
	/// </summary>
	internal void ClearAnalyzeTabsData()
	{
		foreach (var pageData in (IEnumerable<AnalyzeTabPageBindableSource>)AnalyzeTabs.TabItemsSource)
		{
			if (pageData is { Page: IAnalyzerTab subTabPage })
			{
				subTabPage.AnalysisResult = null;
			}
		}
	}

	/// <summary>
	/// To update values via the specified <see cref="AnalysisResult"/> instance.
	/// </summary>
	/// <param name="analysisResult">The analysis result instance.</param>
	/// <seealso cref="AnalysisResult"/>
	internal void UpdateAnalysisResult(AnalysisResult analysisResult)
	{
		foreach (var pageData in (IEnumerable<AnalyzeTabPageBindableSource>)AnalyzeTabs.TabItemsSource)
		{
			if (pageData is { Page: IAnalyzerTab subTabPage })
			{
				subTabPage.AnalysisResult = analysisResult;
			}
		}
	}

	/// <summary>
	/// Copies for sudoku puzzle text.
	/// </summary>
	/// <param name="focusRequired">Indicates whether the focus is required. The default value is <see langword="true"/>.</param>
	internal void CopySudokuGridText(bool focusRequired = true)
	{
		if (focusRequired && !IsSudokuPaneFocused())
		{
			return;
		}

		if (SudokuPane.Puzzle is not { IsUndefined: false, IsEmpty: false } puzzle)
		{
			return;
		}

		var character = Application.Current.AsApp().Preference.UIPreferences.EmptyCellCharacter;
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		dataPackage.SetText(puzzle.ToString($"#{character}"));
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// To determine whether the current application view is in an unsnapped state.
	/// </summary>
	/// <returns>The <see cref="bool"/> value indicating that.</returns>
	internal bool EnsureUnsnapped(bool isFileSaving)
	{
		// 'FileOpenPicker' APIs will not work if the application is in a snapped state.
		// If an app wants to show a 'FileOpenPicker' while snapped, it must attempt to unsnap first.
		var unsnapped = ApplicationView.Value != ApplicationViewState.Snapped || ApplicationView.TryUnsnap();
		if (!unsnapped)
		{
			if (isFileSaving)
			{
				SaveFileFailed?.Invoke(this, new(SaveFileFailedReason.UnsnappingFailed));
			}
			else
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.UnsnappingFailed));
			}
		}
		return unsnapped;
	}

	/// <summary>
	/// Copy the snapshot of the sudoku grid control, to the clipboard.
	/// </summary>
	/// <returns>
	/// The typical <see langword="await"/>able instance that holds the task to copy the snapshot.
	/// </returns>
	/// <remarks>
	/// The code is referenced from
	/// <see href="https://github.com/microsoftarchive/msdn-code-gallery-microsoft/blob/21cb9b6bc0da3b234c5854ecac449cb3bd261f29/Official%20Windows%20Platform%20Sample/XAML%20render%20to%20bitmap%20sample/%5BC%23%5D-XAML%20render%20to%20bitmap%20sample/C%23/Scenario2.xaml.cs#L120">here</see>
	/// and
	/// <see href="https://github.com/microsoftarchive/msdn-code-gallery-microsoft/blob/21cb9b6bc0da3b234c5854ecac449cb3bd261f29/Official%20Windows%20Platform%20Sample/XAML%20render%20to%20bitmap%20sample/%5BC%23%5D-XAML%20render%20to%20bitmap%20sample/C%23/Scenario2.xaml.cs#L182">here</see>.
	/// </remarks>
	internal async Task CopySudokuGridControlAsSnapshotAsync()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		// Creates the stream to store the output image data.
		var stream = new InMemoryRandomAccessStream();
		await OnSavingOrCopyingSudokuPanePictureAsync(stream);

		// Copies the data to the data package.
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		var streamRef = RandomAccessStreamReference.CreateFromStream(stream);
		dataPackage.SetBitmap(streamRef);

		// Copies to the clipboard.
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// Pastes the text, to the clipboard.
	/// </summary>
	/// <returns>
	/// The typical <see langword="await"/>able instance that holds the task to paste the text.
	/// </returns>
	internal async Task PasteCodeToSudokuGridAsync()
	{
		if (IsSudokuPaneFocused()
			&& Clipboard.GetContent() is var dataPackageView
			&& dataPackageView.Contains(StandardDataFormats.Text)
			&& await dataPackageView.GetTextAsync() is var targetText
			&& Grid.TryParse(targetText, out var grid))
		{
			SudokuPane.ViewUnit = null;
			SudokuPane.Puzzle = grid;
		}
	}

	/// <summary>
	/// Open a file.
	/// </summary>
	/// <returns>A task that handles the operation.</returns>
	internal async Task OpenFileInternalAsync()
	{
		if (!EnsureUnsnapped(false))
		{
			return;
		}

		var fop = new FileOpenPicker();
		fop.Initialize(this);
		fop.ViewMode = PickerViewMode.Thumbnail;
		fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fop.AddFileFormat(FileFormats.Text);
		fop.AddFileFormat(FileFormats.PlainText);

		if (await fop.PickSingleFileAsync() is not { } file)
		{
			return;
		}

		await LoadPuzzleCoreAsync(file);
	}

	/// <summary>
	/// Save a file via grid formatters.
	/// </summary>
	/// <param name="gridFormatters">The grid formatters. The default value is <see langword="null"/>.</param>
	/// <returns>A task that handles the operation.</returns>
	internal async Task<bool> SaveFileInternalAsync(GridFormatInfo<Grid>[]? gridFormatters = null)
	{
		if (!EnsureUnsnapped(true))
		{
			return false;
		}

		var fsp = new FileSavePicker();
		fsp.Initialize(this);
		fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fsp.SuggestedFileName = SR.Get("Sudoku", App.CurrentCulture);
		fsp.AddFileFormat(FileFormats.Text);
		fsp.AddFileFormat(FileFormats.PlainText);
		fsp.AddFileFormat(FileFormats.PortablePicture);

		if (await fsp.PickSaveFileAsync() is not { Path: var filePath } file)
		{
			return false;
		}

		if (SudokuPane is not { Puzzle: var grid, ViewUnit: var viewUnit, DisplayCandidates: var displayCandidates })
		{
			return false;
		}

		switch (io::Path.GetExtension(filePath))
		{
			case FileExtensions.PlainText:
			{
				if (gridFormatters is null)
				{
					SudokuPlainTextFileHandler.Write(filePath, grid);
				}
				else
				{
					await File.WriteAllTextAsync(
						filePath,
						string.Join(
							"\r\n\r\n",
							from formatter in gridFormatters select grid.ToString(formatter)
						)
					);
				}
				break;
			}
			case FileExtensions.Text:
			{
				SudokuFileHandler.Write(
					filePath,
					gridFormatters is null
						? [
							new()
							{
								BaseGrid = grid,
								RenderableData = viewUnit switch
								{
									{ Conclusions: var conclusions, View: var view } => new()
									{
										Conclusions = conclusions,
										Views = (View[])[view]
									},
									_ => null
								},
								ShowCandidates = displayCandidates
							}
						]
						:
						from formatter in gridFormatters
						select new GridInfo
						{
							BaseGrid = grid,
							GridString = grid.ToString(formatter),
							RenderableData = viewUnit switch
							{
								{ Conclusions: var conclusions, View: var view } => new()
								{
									Conclusions = conclusions,
									Views = (View[])[view]
								},
								_ => null
							},
							ShowCandidates = displayCandidates
						}
				);
				break;
			}
			case FileExtensions.PortablePicture:
			{
				await OnSavingOrCopyingSudokuPanePictureAsync(file);
				break;
			}
		}
		return true;
	}

	/// <summary>
	/// The internal logic to load a puzzle. This operation can be called after dropping files
	/// or picking files from <see cref="FileOpenPicker"/>.
	/// </summary>
	/// <param name="file">The <see cref="StorageFile"/> instance.</param>
	/// <returns>The task instance that handles this operation.</returns>
	/// <seealso cref="FileOpenPicker"/>
	internal async Task LoadPuzzleCoreAsync(StorageFile? file)
	{
		if (file is null)
		{
			return;
		}

		var filePath = file.Path;
		switch (new FileInfo(filePath).Length)
		{
			case 0:
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsEmpty));
				return;
			}
			case > 1024 * 64:
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsTooLarge));
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
							OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsEmpty));
							return;
						}

						if (!Grid.TryParse(content, out var g))
						{
							OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileCannotBeParsed));
							return;
						}

						SudokuPane.Puzzle = g;
						SudokuPane.ViewUnit = null;
						break;
					}
					case FileExtensions.Text:
					{
						switch (SudokuFileHandler.Read(filePath))
						{
							case [
							{
								BaseGrid: var g,
								GridString: var gridStr,
								ShowCandidates: var showCandidates,
								RenderableData: var possibleDrawable
							}
							]:
							{
								SudokuPane.Puzzle = gridStr is not null && Grid.TryParse(gridStr, out var g2) ? g2 : g;
								SudokuPane.DisplayCandidates = showCandidates;

								if (possibleDrawable is { } drawable)
								{
									VisualUnit = drawable;
								}
								break;
							}
							default:
							{
								OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileCannotBeParsed));
								return;
							}
						}
						break;
					}
				}
				break;
			}
		}
	}

	/// <inheritdoc/>
	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		if (e.Parameter is Grid targetGrid)
		{
			SudokuPane.Puzzle = targetGrid;
			SudokuPane.ViewUnit = null;
		}
	}

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
	/// Try to initialize field <see cref="_hotkeyFunctions"/> and <see cref="_operationPageTypes"/>.
	/// </summary>
	/// <seealso cref="_hotkeyFunctions"/>
	/// <seealso cref="_operationPageTypes"/>
	[MemberNotNull(nameof(_hotkeyFunctions), nameof(_tabsRoutingData))]
	private void InitializeFields()
	{
		_tabsRoutingData = [
			new(
				SR.Get("AnalyzePage_TechniquesTable", App.CurrentCulture),
				new SymbolIconSource { Symbol = Symbol.Flag },
				new Summary { Margin = DefaultMarginForAnalyzerPages, BasePage = this }
			),
			new(
				SR.Get("AnalyzePage_StepDetail", App.CurrentCulture),
				new SymbolIconSource { Symbol = Symbol.ShowResults },
				new SolvingPath { Margin = DefaultMarginForAnalyzerPages, BasePage = this }
			),
			new(
				SR.Get("AnalyzePage_AllStepsInCurrentGrid", App.CurrentCulture),
				new SymbolIconSource { Symbol = Symbol.Shuffle },
				new StepCollecting { Margin = DefaultMarginForAnalyzerPages, BasePage = this }
			)
		];
		_hotkeyFunctions = [
			(new(VirtualKey.Z, VirtualKeyModifiers.Control), SudokuPane.UndoStep),
			(new(VirtualKey.Y, VirtualKeyModifiers.Control), SudokuPane.RedoStep),
			(new(VirtualKey.C, VirtualKeyModifiers.Control), () => CopySudokuGridText()),
			(new(VirtualKey.C, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift), async () => await CopySudokuGridControlAsSnapshotAsync()),
			(new(VirtualKey.V, VirtualKeyModifiers.Control), async () => await PasteCodeToSudokuGridAsync()),
			(new(VirtualKey.O, VirtualKeyModifiers.Control), async () => await OpenFileInternalAsync()),
			(new(VirtualKey.R, VirtualKeyModifiers.Control), UpdatePuzzleViaSolutionGrid),
			(new(VirtualKey.S, VirtualKeyModifiers.Control), async () => await SaveFileInternalAsync()),
			(new((VirtualKey)189), SetPreviousView),
			(new((VirtualKey)187), SetNextView),
			(new(VirtualKey.Home), SetHomeView),
			(new(VirtualKey.End), SetEndView),
			(new(VirtualKey.Escape), ClearView)
		];
	}

	/// <summary>
	/// Load initial grid.
	/// </summary>
	private void LoadInitialGrid()
	{
		if (Application.Current.AsApp().AppStartingGridInfo is
			{
				BaseGrid: var grid,
				GridString: _,
				ShowCandidates: var showCandidates,
				RenderableData: var drawable
			})
		{
			SudokuPane.Puzzle = grid;
			VisualUnit = drawable;
			SudokuPane.DisplayCandidates = showCandidates;

			Application.Current.AsApp().AppStartingGridInfo = null; // Maybe not necessary...
		}
	}

	/// <summary>
	/// Sets the previous view.
	/// </summary>
	private void SetPreviousView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 } && CurrentViewIndex - 1 >= 0)
		{
			CurrentViewIndex--;
		}
	}

	/// <summary>
	/// Sets the next view.
	/// </summary>
	private void SetNextView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: var length and not 0 } && CurrentViewIndex + 1 < length)
		{
			CurrentViewIndex++;
		}
	}

	/// <summary>
	/// Sets the home view.
	/// </summary>
	private void SetHomeView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 })
		{
			CurrentViewIndex = 0;
		}
	}

	/// <summary>
	/// Sets the end view.
	/// </summary>
	private void SetEndView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: var length and not 0 })
		{
			CurrentViewIndex = length - 1;
		}
	}

	/// <summary>
	/// Skips to the specified index of the view.
	/// </summary>
	/// <param name="viewIndex">The view index.</param>
	private void SkipToSpecifiedViewIndex(int viewIndex)
	{
		if (VisualUnit is not { Views.Length: var length } || length <= viewIndex)
		{
			return;
		}

		CurrentViewIndex = viewIndex;
	}

	/// <summary>
	/// Clear the visual unit data.
	/// </summary>
	private void ClearView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 })
		{
			VisualUnit = null;
			SudokuPane.ViewUnit = null;

			// Temporary solution: manually update candidate opacity.
			// This fixes issue #756:
			//   https://github.com/kyoyama-kazusa/Sudoku/issues/756
			if (!SudokuPane.DisplayCandidates)
			{
				ManuallyUpdateCandidateTextBlocksOpacityToZero();
			}
		}
	}

	/// <summary>
	/// Checks whether the pane is focused.
	/// </summary>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool IsSudokuPaneFocused() => SudokuPane.FocusState != FocusState.Unfocused;

	/// <summary>
	/// To update puzzle via solution grid.
	/// </summary>
	private void UpdatePuzzleViaSolutionGrid()
	{
		if (SudokuPane.Puzzle.Uniqueness == Uniqueness.Unique)
		{
			SudokuPane.UpdateGrid(SudokuPane.Puzzle.SolutionGrid);
		}
	}

	/// <summary>
	/// Manually calls <see cref="VisibilityConversion.CellStateToCandidateTextBlockOpacity(Grid, Cell, Digit, ViewUnitBindableSource?, bool)"/>
	/// and update the cooresponding control, making opacity to 0.
	/// </summary>
	private void ManuallyUpdateCandidateTextBlocksOpacityToZero()
	{
		for (var cell = 0; cell < 81; cell++)
		{
			if (SudokuPane.Puzzle.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			for (var digit = 0; digit < 9; digit++)
			{
				var opacity = VisibilityConversion.CellStateToCandidateTextBlockOpacity(
					SudokuPane.Puzzle,
					cell,
					digit,
					null,
					false
				);
				SudokuPane._children[cell]._internalDigitDisplayers[digit + 1].Opacity = opacity;
			}
		}
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
	private async Task OnSavingOrCopyingSudokuPanePictureAsync<T>(T obj) where T : class
	{
		if (Application.Current.AsApp().Preference.UIPreferences.TransparentBackground)
		{
			var color = App.CurrentTheme switch { ApplicationTheme.Light => Colors.White, _ => Colors.Black };
			SudokuPane.MainGrid.Background = new SolidColorBrush(color);
		}

		var desiredSize = Application.Current.AsApp().Preference.UIPreferences.DesiredPictureSizeOnSaving;
		var (originalWidth, originalHeight) = (SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height);
		(SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height) = (desiredSize, desiredSize);

		await SudokuPaneOutsideViewBox.RenderToAsync(obj);

		(SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height) = (originalWidth, originalHeight);

		if (Application.Current.AsApp().Preference.UIPreferences.TransparentBackground)
		{
			SudokuPane.MainGrid.Background = null;
		}
	}


	[Callback]
	private static void CurrentViewIndexPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is (AnalyzePage page, { NewValue: int value }))
		{
			switch (page.VisualUnit)
			{
				case { Conclusions: var conclusions, Views.Length: 0 }:
				{
					page.SudokuPane.ViewUnit = new() { Conclusions = conclusions.ToArray(), View = [] };
					break;
				}
				case { Conclusions: var conclusions, Views: var views } when value != -1:
				{
					page.SudokuPane.ViewUnit = new() { Conclusions = conclusions.ToArray(), View = views.Span[value] };
					break;
				}
			}
		}
	}

	[Callback]
	private static void VisualUnitPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is (AnalyzePage page, { NewValue: var value and (null or IDrawable) }))
		{
			// By forcing assign the value to -1, to trigger view updating operation.
			// Otherwise, the value will always keep the value 0,
			// the view unit won't update because property value 'CurrentViewIndex' won't change.
			page.CurrentViewIndex = -1;

			page.CurrentViewIndex = value is IDrawable ? 0 : -1;

			// Manually updating the pips pager and text block.
			page.ViewsSwitcher.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
			page.ViewsCountDisplayer.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
		}
	}


	private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
	{
		if (e is { Content: IOperationProviderPage page, Parameter: AnalyzePage @this })
		{
			page.BasePage = @this;
		}
	}

	private void FixGridButton_Click(object sender, RoutedEventArgs e) => SudokuPane.UpdateGrid(SudokuPane.Puzzle.FixedGrid);

	private void UnfixGridButton_Click(object sender, RoutedEventArgs e) => SudokuPane.UpdateGrid(SudokuPane.Puzzle.UnfixedGrid);

	private void SudokuPane_MouseWheelChanged(SudokuPane sender, SudokuPaneMouseWheelChangedEventArgs e)
	{
		if (VisualUnit is { Views.Length: not 0 })
		{
			var a = SetNextView;
			var b = SetPreviousView;
			(e.IsClockwise ? a : b)();
		}
	}

	private void SudokuPane_GridUpdated(SudokuPane sender, GridUpdatedEventArgs e)
	{
		if (e.Behavior is >= 0 and <= GridUpdatedBehavior.Clear)
		{
			ClearAnalyzeTabsData();
		}

		VisualUnit = null;
	}

	private void SudokuPane_ReceivedDroppedFileSuccessfully(SudokuPane sender, ReceivedDroppedFileSuccessfullyEventArgs e)
	{
		if (e is not { FilePath: var filePath, GridInfo: var gridInfo })
		{
			return;
		}

		switch (io::Path.GetExtension(filePath), gridInfo)
		{
			case (FileExtensions.PlainText, { BaseGrid: var g }):
			{
				SudokuPane.Puzzle = g;
				SudokuPane.ViewUnit = null;
				break;
			}
			case (FileExtensions.Text, { BaseGrid: var g, RenderableData: { } visualUnit, ShowCandidates: var showCandidates }):
			{
				SudokuPane.Puzzle = g;
				SudokuPane.DisplayCandidates = showCandidates;
				VisualUnit = visualUnit;
				break;
			}
		}
	}

	private async void AnalyzeButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var puzzle = SudokuPane.Puzzle;
		if (puzzle.Uniqueness == Uniqueness.Bad)
		{
			return;
		}

		var handle = WindowNative.GetWindowHandle(App.GetMainWindow(this));
		var isSupportedTaskbarProgressChanging = TaskbarProgress.IsTaskbarSupported;
		if (isSupportedTaskbarProgressChanging)
		{
			TaskbarProgress.SetState(handle, TBPFLAG.TBPF_INDETERMINATE);
			TaskbarProgress.SetValue(handle, 0, 100);
		}

		AnalyzeButton.IsEnabled = false;
		ClearAnalyzeTabsData();
		IsAnalyzerLaunched = true;

		var textFormat = SR.Get("AnalyzePage_AnalyzerProgress", App.CurrentCulture);
		using var cts = new CancellationTokenSource();
		var analyzer = Application.Current.AsApp().GetAnalyzerConfigured(SudokuPane);
		_ctsForAnalyzingRelatedOperations = cts;

		try
		{
			switch (await Task.Run(() =>
			{
				lock (AnalyzingRelatedSyncRoot)
				{
					return analyzer.Analyze(
						puzzle,
						new Progress<StepGathererProgressPresenter>(
							progress => DispatcherQueue.TryEnqueue(
								() =>
								{
									var (stepSearcherName, percent) = progress;
									ProgressPercent = progress.Percent * 100;
									AnalyzeProgressLabel.Text = string.Format(textFormat, percent);
									AnalyzeStepSearcherNameLabel.Text = stepSearcherName;

									if (isSupportedTaskbarProgressChanging)
									{
										TaskbarProgress.SetValue(handle, ProgressPercent, 100);
									}
								}
							)
						),
						cts.Token
					);
				}
			}))
			{
				case var analysisResult and ({ IsSolved: true } or { IsPartiallySolved: true }):
				{
					UpdateAnalysisResult(analysisResult);
					AnalysisResultCache = analysisResult;
					break;
				}
				case
				{
					WrongStep: { Views: var views, Conclusions: var conclusions } wrongStep,
					FailedReason: FailedReason.WrongStep,
					UnhandledException: WrongStepException { InvalidGrid: var invalidGrid }
				}:
				{
					await new ContentDialog
					{
						XamlRoot = XamlRoot,
						Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]!,
						Title = SR.Get("AnalyzePage_ErrorStepEncounteredTitle", App.CurrentCulture),
						CloseButtonText = SR.Get("AnalyzePage_ErrorStepDialogCloseButtonText", App.CurrentCulture),
						DefaultButton = ContentDialogButton.Close,
						Content = new ErrorStepDialogContent
						{
							ErrorStepGrid = invalidGrid,
							ErrorStepText = string.Format(SR.Get("AnalyzePage_ErrorStepDescription", App.CurrentCulture), wrongStep),
							ViewUnit = new() { View = views?[0] ?? [], Conclusions = conclusions }
						}
					}.ShowAsync();
					break;
				}
				case { FailedReason: FailedReason.ExceptionThrown, UnhandledException: { } ex }:
				{
					await new ContentDialog
					{
						XamlRoot = XamlRoot,
						Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]!,
						Title = SR.Get("AnalyzePage_ExceptionThrownTitle", App.CurrentCulture),
						CloseButtonText = SR.Get("AnalyzePage_ErrorStepDialogCloseButtonText", App.CurrentCulture),
						DefaultButton = ContentDialogButton.Close,
						Content = new ExceptionThrownOnAnalyzingContent { ThrownException = ex }
					}.ShowAsync();
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_ctsForAnalyzingRelatedOperations = null;
			AnalyzeButton.IsEnabled = true;
			IsAnalyzerLaunched = false;

			if (isSupportedTaskbarProgressChanging)
			{
				TaskbarProgress.SetState(handle, TBPFLAG.TBPF_NORMAL);
				TaskbarProgress.SetValue(handle, 0, 100);
			}
		}
	}

	private void SudokuPane_Clicked(SudokuPane sender, GridClickedEventArgs e)
	{
		switch (this, e)
		{
#pragma warning disable format
			case (
				{
					SudokuPane: { DisableFlyout: false, Puzzle: var puzzle },
					MainMenuFlyout.SecondaryCommands: var secondaryCommands
				},
				{
					MouseButton: MouseButton.Right,
					Cell: var cell
				}
			) when secondaryCommands.OfType<AppBarButton>() is var appBarButtons:
#pragma warning restore format
			{
				switch (puzzle.GetState(cell))
				{
					case CellState.Empty:
					{
						SudokuPane._temporarySelectedCell = cell;
						foreach (var element in appBarButtons)
						{
							element.IsEnabled = (puzzle.GetCandidates(cell) >> Abs((Digit)element.Tag) - 1 & 1) != 0;
						}

						MainMenuFlyout.ShowAt(SudokuPane);
						break;
					}
					case CellState.Given or CellState.Modifiable:
					{
						SudokuPane._temporarySelectedCell = cell;
						foreach (var element in appBarButtons)
						{
							element.IsEnabled = false;
						}

						MainMenuFlyout.ShowAt(SudokuPane, new() { ShowMode = FlyoutShowMode.Transient });
						break;
					}
				}
				break;
			}
			default:
			{
				// Manually set focus for pane because user clicked a candidate, displayed using a text block, making the pane unfocused.
				SudokuPane.Focus(FocusState.Programmatic);
				break;
			}
		}
	}

	private void SudokuPane_CandidatesDisplayingToggled(SudokuPane sender, CandidatesDisplayingToggledEventArgs e)
		=> Application.Current.AsApp().Preference.UIPreferences.DisplayCandidates = sender.DisplayCandidates;

	private void SudokuPane_Caching(object sender, EventArgs e)
	{
		if (SudokuPane is not { Puzzle: var puzzle, ViewUnit: var viewUnit })
		{
			return;
		}

		// Set as cached values.
		// No matter whether the option 'AutoCachePuzzleAndView' is true or not,
		// here we should save them because in a same program thread we may use the values to recover it.
		Application.Current.AsApp().Preference.UIPreferences.LastGridPuzzle = puzzle;
		Application.Current.AsApp().Preference.UIPreferences.LastRenderable = viewUnit switch
		{
			ViewUnitBindableSource { View: var view, Conclusions: var conclusions } => new() { Conclusions = conclusions, Views = (View[])[view] },
			_ => null
		};
	}

	private void SudokuPane_DigitInput(SudokuPane sender, DigitInputEventArgs e)
	{
		VisualUnit = null;
		SudokuPane.ViewUnit = null;
	}

	private void MainMenuFlyout_Closed(object sender, object e)
	{
		foreach (var element in MainMenuFlyout.SecondaryCommands.OfType<AppBarButton>())
		{
			element.Visibility = Visibility.Visible;
		}
	}

	private void SetOrDeleteButton_Click(object sender, RoutedEventArgs e)
	{
		if ((sender, SudokuPane) is (AppBarButton { Tag: Digit rawDigit }, { _temporarySelectedCell: var cell and not -1 }))
		{
			SudokuPane.SetOrDeleteDigit(cell, Abs(rawDigit) - 1, rawDigit > 0);
		}
	}

	private void CopyButton_Click(object sender, RoutedEventArgs e) => CopySudokuGridText(false);

	private void CopyKindButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: int rawFormatFlag })
		{
			return;
		}

		var flag = (SudokuFormatFlags)rawFormatFlag;
		if (!Enum.IsDefined(flag))
		{
			return;
		}

		if (SudokuPane.Puzzle is not { IsUndefined: false, IsEmpty: false } puzzle)
		{
			return;
		}

		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		dataPackage.SetText(
			flag != SudokuFormatFlags.HodokuCompatibleFormat
				? puzzle.ToString(flag.GetConverter())
				: HodokuCompatibility.GetHodokuLibraryFormat(puzzle, VisualUnit as Step)
		);
		Clipboard.SetContent(dataPackage);
	}

	private void CancelOperationButton_Click(object sender, RoutedEventArgs e) => _ctsForAnalyzingRelatedOperations?.Cancel();

	private void AutoSolveButton_Click(object sender, RoutedEventArgs e) => UpdatePuzzleViaSolutionGrid();

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		// This method is created to solve the problem that WinUI cannot cache navigation view pages due to internal error.
		if (Application.Current.AsApp().Preference.UIPreferences.AutoCachePuzzleAndView)
		{
#if false
			var pref = Application.Current.AsApp().Preference.UIPreferences;

			SudokuPane.Puzzle = pref.LastGridPuzzle;
			SudokuPane.ViewUnit = pref.LastRenderable is ({ } conclusions, [var view, ..])
				? new() { Conclusions = conclusions, View = view }
				: null;
#endif
		}
	}

	private void SudokuPane_Loaded(object sender, RoutedEventArgs e)
	{
		var app = Application.Current.AsApp();
		app.CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);
		app.MainSudokuPane = SudokuPane;
	}

	private void SudokuPane_ActualThemeChanged(FrameworkElement sender, object args)
		=> Application.Current.AsApp().CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);

	private async void SaveToLibraryButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var puzzle = SudokuPane.Puzzle;
		if (!puzzle.IsValid)
		{
			return;
		}

		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogTitle", App.CurrentCulture),
			IsPrimaryButtonEnabled = true,
			DefaultButton = ContentDialogButton.Primary,
			PrimaryButtonText = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogSure", App.CurrentCulture),
			CloseButtonText = SR.Get("AnalyzePage_AddPuzzleToLibraryDialogCancel", App.CurrentCulture),
			Content = new SaveToLibraryDialogContent { AvailableLibraries = LibraryBindableSource.GetLibrariesFromLocal() }
		};
		if (await dialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}

		switch ((SaveToLibraryDialogContent)dialog.Content)
		{
			case { SelectedMode: 0, SelectedLibrary: LibraryBindableSource { Library: var lib } }:
			{
				await lib.AppendPuzzleAsync(puzzle);
				break;
			}
			case { SelectedMode: 1, IsNameValidAsFileId: true } content:
			{
				var libraryCreated = new LibraryInfo(CommonPaths.Library, content.FileId);
				libraryCreated.Initialize();
				libraryCreated.Name = content.LibraryName is var name and not (null or "") ? name : null;
				libraryCreated.Author = content.LibraryAuthor is var author and not (null or "") ? author : null;
				libraryCreated.Description = content.LibraryDescription is var description and not (null or "") ? description : null;
				libraryCreated.Tags = content.LibraryTags is { Count: not 0 } tags ? [.. tags] : null;
				await libraryCreated.AppendPuzzleAsync(puzzle);
				break;
			}
		}
	}

	private void FunctionSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
	{
		var currentSelectedIndex = sender.Items.IndexOf(sender.SelectedItem);
		ContentFrame.Navigate(
			_operationPageTypes[currentSelectedIndex],
			this,
			new SlideNavigationTransitionInfo
			{
				Effect = currentSelectedIndex - _previouslySelectedIndexOfOperationPage > 0
					? SlideNavigationTransitionEffect.FromRight
					: SlideNavigationTransitionEffect.FromLeft
			}
		);
		_previouslySelectedIndexOfOperationPage = currentSelectedIndex;
	}
}
