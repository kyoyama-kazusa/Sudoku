namespace SudokuStudio.Views.Pages.Operation;

/// <summary>
/// Indicates the generating operation command bar.
/// </summary>
public sealed partial class GeneratingOperation : Page, IOperationProviderPage
{
	/// <summary>
	/// Initializes a <see cref="GeneratingOperation"/> instance.
	/// </summary>
	public GeneratingOperation() => InitializeComponent();


	/// <inheritdoc/>
	public AnalyzePage BasePage { get; set; } = null!;


	/// <inheritdoc/>
	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		if (e.Parameter is AnalyzePage p)
		{
			SetGeneratingStrategyTooltip(p);
			RefreshPuzzleLibraryComboBox();
		}
	}

	/// <summary>
	/// Set generating strategy tooltip.
	/// </summary>
	/// <param name="basePage">The base page.</param>
	private void SetGeneratingStrategyTooltip(AnalyzePage basePage)
	{
		var constraints = Application.CurrentApp.Preference.ConstraintPreferences.Constraints;
		TextBlockBindable.SetInlines(
			GeneratorStrategyTooltip,
			[new Run { Text = string.Join(Environment.NewLine, from c in constraints select c.ToString(App.CurrentCulture)) }]
		);
	}

	/// <summary>
	/// Refreshes puzzle library for combo box.
	/// </summary>
	private void RefreshPuzzleLibraryComboBox()
	{
		var libs = LibrarySimpleBindableSource.GetLibraries();
		(PuzzleLibraryChoser.Visibility, LibraryPuzzleFetchButton.Visibility, LibSeparator.Visibility) = libs.Length != 0
			? (Visibility.Visible, Visibility.Visible, Visibility.Visible)
			: (Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed);
		PuzzleLibraryChoser.ItemsSource = (from lib in libs select new LibrarySimpleBindableSource { Library = lib }).ToArray();

		var lastFileId = Application.CurrentApp.Preference.UIPreferences.FetchingPuzzleLibrary;
		PuzzleLibraryChoser.SelectedIndex = Array.FindIndex(libs, match) is var index and not -1 ? index : 0;


		bool match(Library lib) => io::Path.GetFileNameWithoutExtension(lib.LibraryPath) == lastFileId;
	}

	/// <summary>
	/// Handle generating operation.
	/// </summary>
	/// <typeparam name="TProgressDataProvider">The type of the progress data provider.</typeparam>
	/// <param name="onlyGenerateOne">Indicates whether the generator engine only generates for one puzzle.</param>
	/// <param name="gridStateChanger">
	/// The method that can change the state of the target grid. This callback method will be used for specify the grid state
	/// when a user has set the techniques that must be appeared.
	/// </param>
	/// <param name="gridTextConsumer">An action that consumes the generated <see cref="string"/> grid text code.</param>
	/// <returns>The task that holds the asynchronous operation.</returns>
	private async Task HandleGeneratingAsync<TProgressDataProvider>(
		bool onlyGenerateOne,
		GridStateChanger<Analyzer>? gridStateChanger = null,
		Action<string>? gridTextConsumer = null
	)
		where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		var processingText = SR.Get("AnalyzePage_GeneratorIsProcessing", App.CurrentCulture);
		await GeneratorHub.GenerateAsync(
			onlyGenerateOne,
			new GeneratorDriver<TProgressDataProvider>
			{
				ConstraintsCreator = static () => Application.CurrentApp.Preference.ConstraintPreferences.Constraints,
				DifficultyLevelCreator = static constraints =>
				{
					var difficultyLevels = from c in constraints.OfType<DifficultyLevelConstraint>() select c.DifficultyLevel;
					return difficultyLevels is [var dl] ? dl : default;
				},
				AnalyzerCreator = difficultyLevel => Application.CurrentApp.GetAnalyzerConfigured(BasePage.SudokuPane, difficultyLevel),
				IttoryuFinderCreator = static () => new DisorderedIttoryuFinder(TechniqueSet.IttoryuTechniques),
				CancellationTokenSourceAssigner = cts => BasePage._ctsForAnalyzingRelatedOperations = cts,
				StateInitializer = () =>
				{
					BasePage.IsGeneratorLaunched = true;
					BasePage.ClearAnalyzeTabsData();
				},
				StateFinalizer = () =>
				{
					BasePage._ctsForAnalyzingRelatedOperations = null;
					BasePage.IsGeneratorLaunched = false;
				},
				BottleneckFiltersCreator = static () =>
				{
					var analysisPref = Application.CurrentApp.Preference.AnalysisPreferences;
					return [
						new(TechniqueType.Direct, analysisPref.DirectModeBottleneckType),
						new(TechniqueType.Snyder, analysisPref.PartialMarkingModeBottleneckType),
						new(TechniqueType.Advanced, analysisPref.FullMarkingModeBottleneckType)
					];
				},
				ReportAction = progress => DispatcherQueue.TryEnqueue(
					() =>
					{
						BasePage.AnalyzeProgressLabel.Text = processingText;
						BasePage.AnalyzeStepSearcherNameLabel.Text = progress.ToDisplayString();
					}
				),
				GridStateChanger = gridStateChanger,
				GridTextConsumer = gridTextConsumer
			}
		);
	}


	private async void NewPuzzleButton_ClickAsync(object sender, RoutedEventArgs e)
		=> await HandleGeneratingAsync<GeneratorProgress>(
			true,
			gridTextConsumer: gridText =>
			{
				var grid = Grid.Parse(gridText);
				if (Application.CurrentApp.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					Application.CurrentApp.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = grid });
				}

				BasePage.SudokuPane.Puzzle = grid;
				BasePage.ClearAnalyzeTabsData();
				BasePage.SudokuPane.ViewUnit = null;
			}
		);

	private async void LibraryPuzzleFetchButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var library = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		if (library.IsEmpty)
		{
			// There is no puzzle can be selected.
			return;
		}

		var types = Application.CurrentApp.Preference.LibraryPreferences.LibraryPuzzleTransformations;
		BasePage.SudokuPane.Puzzle = await library.RandomReadOneAsync(types);
		BasePage.ClearAnalyzeTabsData();
		BasePage.SudokuPane.ViewUnit = null;
	}

	private void PuzzleLibraryChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var source = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		var fileId = io::Path.GetFileNameWithoutExtension(source.LibraryPath);
		Application.CurrentApp.Preference.UIPreferences.FetchingPuzzleLibrary = fileId;
	}

	private async void BatchGeneratingToLibraryButton_ClickAsync(object sender, RoutedEventArgs e)
	{
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

		var appendToLibraryTask = static (string _, CancellationToken _ = default) => default(ValueTask)!;
		switch ((SaveToLibraryDialogContent)dialog.Content)
		{
			case { SelectedMode: 0, SelectedLibrary: LibraryBindableSource { Library: var lib } }:
			{
				appendToLibraryTask = lib.WriteLineAsync;
				break;
			}
			case { SelectedMode: 1, IsNameValidAsFileId: true } content:
			{
				var libraryCreated = new Library(CommonPaths.Library, content.FileId);
				if (content.LibraryName is var name and not (null or ""))
				{
					libraryCreated.WriteName(name);
				}
				if (content.LibraryAuthor is var author and not (null or ""))
				{
					libraryCreated.WriteAuthor(author);
				}
				if (content.LibraryDescription is var description and not (null or ""))
				{
					libraryCreated.WriteDescription(description);
				}
				if (content.LibraryTags is { Count: not 0 } tags)
				{
					libraryCreated.WriteTags([.. tags]);
				}
				appendToLibraryTask = libraryCreated.WriteLineAsync;
				break;
			}
		}

		await HandleGeneratingAsync<FilteredGeneratorProgress>(
			false,
			static (ref grid, analyzer) =>
			{
				var analysisResult = analyzer.Analyze(grid);
				if (analysisResult is not { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps })
				{
					return;
				}

				var techniques = TechniqueSet.None;
				foreach (var constraint in Application.CurrentApp.Preference.ConstraintPreferences.Constraints)
				{
					switch (constraint)
					{
						case TechniqueConstraint { Techniques: var t }:
						{
							techniques |= t;
							break;
						}
						case TechniqueCountConstraint { Technique: var technique } and not { Operator: ComparisonOperator.Equality, LimitCount: 0 }:
						{
							techniques.Add(technique);
							break;
						}
					}
				}

				foreach (var (g, s) in Step.Combine(grids, steps))
				{
					if (techniques.Contains(s.Code))
					{
						grid = g;
						break;
					}
				}
			},
			async gridText =>
			{
				await appendToLibraryTask($"{gridText}{Environment.NewLine}");

				var app = Application.CurrentApp;
				if (app.Preference.UIPreferences.AlsoSaveBatchGeneratedPuzzlesIntoHistory
					&& app.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					app.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = Grid.Parse(gridText) });
				}
			}
		);
	}

	private async void BatchGeneratingButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (!BasePage.EnsureUnsnapped(true))
		{
			return;
		}

		var fsp = new FileSavePicker();
		fsp.Initialize(this);
		fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fsp.SuggestedFileName = SR.Get("Sudoku", App.CurrentCulture);
		fsp.AddFileFormat(FileFormats.PlainText);

		if (await fsp.PickSaveFileAsync() is not { Path: var filePath })
		{
			return;
		}

		await HandleGeneratingAsync<FilteredGeneratorProgress>(
			false,
			static (ref grid, analyzer) =>
			{
				var analysisResult = analyzer.Analyze(grid);
				if (analysisResult is not { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps })
				{
					return;
				}

				var techniques = TechniqueSet.None;
				foreach (var constraint in Application.CurrentApp.Preference.ConstraintPreferences.Constraints)
				{
					switch (constraint)
					{
						case TechniqueConstraint { Techniques: var t }:
						{
							techniques |= t;
							break;
						}
						case TechniqueCountConstraint { Technique: var technique } and not { Operator: ComparisonOperator.Equality, LimitCount: 0 }:
						{
							techniques.Add(technique);
							break;
						}
					}
				}

				foreach (var (g, s) in Step.Combine(grids, steps))
				{
					if (techniques.Contains(s.Code))
					{
						grid = g;
						break;
					}
				}
			},
			gridText =>
			{
				File.AppendAllText(filePath, $"{gridText}{Environment.NewLine}");

				var app = Application.CurrentApp;
				if (app.Preference.UIPreferences.AlsoSaveBatchGeneratedPuzzlesIntoHistory
					&& app.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					app.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = Grid.Parse(gridText) });
				}
			}
		);
	}
}

/// <summary>
/// Provides a generator driver instance.
/// </summary>
/// <typeparam name="TProgressDataProvider">The type of progress data provider.</typeparam>
file sealed class GeneratorDriver<TProgressDataProvider> : IGeneratorDriver<TProgressDataProvider>
	where TProgressDataProvider :
		struct,
		IEquatable<TProgressDataProvider>,
		IProgressDataProvider<TProgressDataProvider>,
		allows ref struct
{
	/// <inheritdoc/>
	public required Func<ConstraintCollection> ConstraintsCreator { get; init; }

	/// <inheritdoc/>
	public required Func<ConstraintCollection, DifficultyLevel> DifficultyLevelCreator { get; init; }

	/// <inheritdoc/>
	public required Func<DifficultyLevel, Analyzer> AnalyzerCreator { get; init; }

	/// <inheritdoc/>
	public required Func<DisorderedIttoryuFinder> IttoryuFinderCreator { get; init; }

	/// <inheritdoc/>
	public required Action<CancellationTokenSource> CancellationTokenSourceAssigner { get; init; }

	/// <inheritdoc/>
	public required Action StateInitializer { get; init; }

	/// <inheritdoc/>
	public required Action StateFinalizer { get; init; }

	/// <inheritdoc/>
	public required Func<BottleneckFilter[]> BottleneckFiltersCreator { get; init; }

	/// <inheritdoc/>
	public required Action<TProgressDataProvider> ReportAction { get; init; }

	/// <inheritdoc/>
	public required GridStateChanger<Analyzer>? GridStateChanger { get; init; }

	/// <inheritdoc/>
	public required Action<string>? GridTextConsumer { get; init; }
}
