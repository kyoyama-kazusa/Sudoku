namespace SudokuStudio.Views.Pages.Operation;

/// <summary>
/// Indicates the generating operation command bar.
/// </summary>
public sealed partial class GeneratingOperation : Page, IOperationProviderPage
{
	/// <summary>
	/// The fields for core usages on counting puzzles.
	/// </summary>
	private int _generatingCount, _generatingFilteredCount;


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
		var constraints = Application.Current.AsApp().Preference.ConstraintPreferences.Constraints;
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

		var lastFileId = Application.Current.AsApp().Preference.UIPreferences.FetchingPuzzleLibrary;
		PuzzleLibraryChoser.SelectedIndex = Array.FindIndex(libs, match) is var index and not -1 ? index : 0;


		bool match(LibraryInfo lib) => lib is { FileId: var f } && f == lastFileId;
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
	) where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		BasePage.IsGeneratorLaunched = true;
		BasePage.ClearAnalyzeTabsData();

		var processingText = SR.Get("AnalyzePage_GeneratorIsProcessing", App.CurrentCulture);
		var constraints = Application.Current.AsApp().Preference.ConstraintPreferences.Constraints;
		var difficultyLevel = (from c in constraints.OfType<DifficultyLevelConstraint>() select c.DifficultyLevel) is [var dl] ? dl : default;
		var analyzer = Application.Current.AsApp().GetAnalyzerConfigured(BasePage.SudokuPane, difficultyLevel);
		var ittoryuFinder = new DisorderedIttoryuFinder(TechniqueIttoryuSets.IttoryuTechniques);
		var analysisPref = Application.Current.AsApp().Preference.AnalysisPreferences;
		var filters = (BottleneckFilter[])[
			new(PencilmarkVisibility.Direct, analysisPref.DirectModeBottleneckType),
			new(PencilmarkVisibility.PartialMarking, analysisPref.PartialMarkingModeBottleneckType),
			new(PencilmarkVisibility.FullMarking, analysisPref.FullMarkingModeBottleneckType)
		];

		using var cts = new CancellationTokenSource();
		BasePage._ctsForAnalyzingRelatedOperations = cts;

		try
		{
			(_generatingCount, _generatingFilteredCount) = (0, 0);
			if (onlyGenerateOne)
			{
				if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
				{
					triggerEvents(ref grid, analyzer);
				}
			}
			else
			{
				while (true)
				{
					if (await Task.Run(taskEntry) is { IsUndefined: false } grid)
					{
						triggerEvents(ref grid, analyzer);

						_generatingFilteredCount++;
						continue;
					}

					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			BasePage._ctsForAnalyzingRelatedOperations = null;
			BasePage.IsGeneratorLaunched = false;
		}


		void triggerEvents(ref Grid grid, Analyzer analyzer)
		{
			gridStateChanger?.Invoke(ref grid, analyzer);
			gridTextConsumer?.Invoke(grid.ToString("#"));
		}

		unsafe Grid taskEntry()
		{
			var hasFullHouseConstraint = constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.FullHouse }];
			var hasNakedSingleConstraint = constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.NakedSingle }];
			var hasFullHouseConstraintInTechniqueSet = constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.FullHouse] }];
			var hasNakedSingleConnstraintInTechniqueSet = constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.NakedSingle] }];
			var hasIttoryuConstraint = constraints.OfType<IttoryuConstraint>() is [{ Operator: ComparisonOperator.Equality, Rounds: 1 }];
			return coreHandler(
				constraints,
				hasFullHouseConstraint || hasFullHouseConstraintInTechniqueSet
					? &handlerFullHouse
					: hasNakedSingleConstraint || hasNakedSingleConnstraintInTechniqueSet
						? &handlerNakedSingle
						: hasIttoryuConstraint
							? &handlerIttoryu
							: &handlerDefault,
				progress => DispatcherQueue.TryEnqueue(
					() =>
					{
						BasePage.AnalyzeProgressLabel.Text = processingText;
						BasePage.AnalyzeStepSearcherNameLabel.Text = progress.ToDisplayString();
					}
				),
				cts.Token,
				hasNakedSingleConstraint || hasNakedSingleConnstraintInTechniqueSet
					? analyzer.WithUserDefinedOptions(analyzer.Options with { PrimarySingle = SingleTechniqueFlag.NakedSingle })
					: analyzer,
				ittoryuFinder,
				filters
			);


			static Grid handlerFullHouse(int givens, SymmetricType type, CancellationToken ct)
				=> new FullHouseGenerator
				{
					SymmetricType = type,
					EmptyCellsCount = givens == -1 ? -1 : 81 - givens
				}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

			static Grid handlerNakedSingle(int givens, SymmetricType type, CancellationToken ct)
				=> new NakedSingleGenerator
				{
					SymmetricType = type,
					EmptyCellsCount = givens == -1 ? -1 : 81 - givens
				}.TryGenerateUnique(out var p, ct) ? p : throw new OperationCanceledException();

			static Grid handlerDefault(int givens, SymmetricType symmetry, CancellationToken ct)
			{
				var puzzle = new Generator().Generate(givens, symmetry, ct);
				return puzzle.IsUndefined ? throw new OperationCanceledException() : puzzle;
			}

			static Grid handlerIttoryu(int givens, SymmetricType symmetry, CancellationToken ct)
			{
				var finder = new DisorderedIttoryuFinder();
				var generator = new Generator();
				while (true)
				{
					var puzzle = generator.Generate(givens, symmetry, ct);
					if (puzzle.IsUndefined)
					{
						throw new OperationCanceledException();
					}

					if (finder.FindPath(puzzle, ct) is { IsComplete: true } path)
					{
						puzzle.MakeIttoryu(path);
						return puzzle;
					}
				}
			}
		}

		unsafe Grid coreHandler(
			ConstraintCollection constraints,
			delegate*<Cell, SymmetricType, CancellationToken, Grid> gridCreator,
			Action<TProgressDataProvider> reporter,
			CancellationToken cancellationToken,
			Analyzer analyzer,
			DisorderedIttoryuFinder finder,
			BottleneckFilter[] filters
		)
		{
			// Update generating configurations.
			if (constraints.OfType<BottleneckTechniqueConstraint>() is { Length: not 0 } list)
			{
				foreach (var element in list)
				{
					element.Filters = filters;
				}
			}

			var rng = Random.Shared;
			var symmetries = (
				(
					from c in constraints.OfType<SymmetryConstraint>()
					select c.SymmetricTypes
				) is [var p] ? p : SymmetryConstraint.AllSymmetricTypes
			) switch
			{
				SymmetryConstraint.InvalidSymmetricType => [],
				SymmetryConstraint.AllSymmetricTypes => Enum.GetValues<SymmetricType>(),
				var symmetricTypes and not 0 => symmetricTypes.AllFlags,
				_ => [SymmetricType.None]
			};
			var chosenGivensCountSeed = (
				from c in constraints.OfType<CountBetweenConstraint>()
				let betweenRule = c.BetweenRule
				let pair = (Start: c.Range.Start.Value, End: c.Range.End.Value)
				let targetPair = c.CellState switch { CellState.Given => (pair.Start, pair.End), CellState.Empty => (81 - pair.End, 81 - pair.Start) }
				select (betweenRule, targetPair)
			) is [var (br, (start, end))] ? b(br, start, end) : (-1, -1);
			var givensCount = chosenGivensCountSeed is (var s and not -1, var e and not -1) ? rng.Next(s, e + 1) : -1;
			var difficultyLevel = (
				from c in constraints.OfType<DifficultyLevelConstraint>()
				select c.ValidDifficultyLevels.AllFlags.ToArray()
			) is [var d] ? d[rng.Next(0, d.Length)] : DifficultyLevels.AllValid;
			var progress = new SelfReportingProgress<TProgressDataProvider>(reporter);
			while (true)
			{
				var chosenSymmetricType = symmetries[rng.Next(0, symmetries.Length)];
				var grid = gridCreator(givensCount, chosenSymmetricType, cancellationToken);
				if (grid.IsEmpty || analyzer.Analyze(grid) is var analysisResult && !analysisResult.IsSolved)
				{
					goto ReportState;
				}

				if (constraints.IsValidFor(new(grid, analysisResult)))
				{
					return grid;
				}

			ReportState:
				progress.Report(TProgressDataProvider.Create(++_generatingCount, _generatingFilteredCount));
				cancellationToken.ThrowIfCancellationRequested();
			}


			static (Cell, Cell) b(BetweenRule betweenRule, Cell start, Cell end)
				=> betweenRule switch
				{
					BetweenRule.BothOpen => (start + 1, end - 1),
					BetweenRule.LeftOpen => (start + 1, end),
					BetweenRule.RightOpen => (start, end + 1),
					_ => (start, end)
				};
		}
	}


	private async void NewPuzzleButton_ClickAsync(object sender, RoutedEventArgs e)
		=> await HandleGeneratingAsync<GeneratorProgress>(
			true,
			gridTextConsumer: gridText =>
			{
				var grid = Grid.Parse(gridText);
				if (Application.Current.AsApp().Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					Application.Current.AsApp().PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = grid });
				}

				BasePage.SudokuPane.Puzzle = grid;
				BasePage.ClearAnalyzeTabsData();
				BasePage.SudokuPane.ViewUnit = null;
			}
		);

	private async void LibraryPuzzleFetchButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var library = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		if (!library.Any())
		{
			// There is no puzzle can be selected.
			return;
		}

		var types = Application.Current.AsApp().Preference.LibraryPreferences.LibraryPuzzleTransformations;
		BasePage.SudokuPane.Puzzle = await library.RandomReadOneAsync(types);
		BasePage.ClearAnalyzeTabsData();
		BasePage.SudokuPane.ViewUnit = null;
	}

	private void PuzzleLibraryChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var source = ((LibrarySimpleBindableSource)PuzzleLibraryChoser.SelectedValue).Library;
		Application.Current.AsApp().Preference.UIPreferences.FetchingPuzzleLibrary = source.FileId;
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

		var appendToLibraryTask = static (string _, CancellationToken _ = default) => default(Task)!;
		switch ((SaveToLibraryDialogContent)dialog.Content)
		{
			case { SelectedMode: 0, SelectedLibrary: LibraryBindableSource { Library: var lib } }:
			{
				appendToLibraryTask = lib.AppendPuzzleAsync;
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
				appendToLibraryTask = libraryCreated.AppendPuzzleAsync;
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

				var techniques = TechniqueSets.None;
				foreach (var constraint in Application.Current.AsApp().Preference.ConstraintPreferences.Constraints)
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

				var app = Application.Current.AsApp();
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

				var techniques = TechniqueSets.None;
				foreach (var constraint in Application.Current.AsApp().Preference.ConstraintPreferences.Constraints)
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

				var app = Application.Current.AsApp();
				if (app.Preference.UIPreferences.AlsoSaveBatchGeneratedPuzzlesIntoHistory
					&& app.Preference.UIPreferences.SavePuzzleGeneratingHistory)
				{
					app.PuzzleGeneratingHistory.Puzzles.Add(new() { BaseGrid = Grid.Parse(gridText) });
				}
			}
		);
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Randomly read one puzzle in the specified file, and return it.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <param name="transformTypes">
	/// Indicates the available transform type that the chosen grid can be transformed.
	/// Use <see cref="TransformType"/>.<see langword="operator"/> |(<see cref="TransformType"/>, <see cref="TransformType"/>)
	/// to combine multiple flags.
	/// </param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task{TResult}"/> of <see cref="Grid"/> instance as the result.</returns>
	/// <exception cref="InvalidOperationException">Throw when the library file is not initialized.</exception>
	/// <seealso href="http://tinyurl.com/choose-a-random-element">Choose a random element from a sequence of unknown length</seealso>
	public static async Task<Grid> RandomReadOneAsync(
		this LibraryInfo @this,
		TransformType transformTypes = TransformType.None,
		CancellationToken cancellationToken = default
	)
	{
		if (!@this.IsInitialized)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		var rng = new Random();
		var numberSeen = 0U;
		Unsafe.SkipInit<Grid>(out var chosen);
		await foreach (var text in @this.EnumerateTextAsync(cancellationToken))
		{
			if ((uint)rng.Next() % ++numberSeen == 0)
			{
				chosen = Grid.Parse(text);
			}
		}

		chosen.Transform(transformTypes);
		return chosen;
	}
}
