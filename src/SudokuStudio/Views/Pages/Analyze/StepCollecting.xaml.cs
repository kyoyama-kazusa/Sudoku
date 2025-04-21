namespace SudokuStudio.Views.Pages.Analyze;

/// <summary>
/// Defines a step collecting page.
/// </summary>
public sealed partial class StepCollecting : Page, IAnalyzerTab
{
	/// <summary>
	/// A collection sorted by technique.
	/// </summary>
	private ObservableCollection<CollectedStepBindableSource>? _nodesSortedByTechnique;

	/// <summary>
	/// A collection sorted by the number of eliminations.
	/// </summary>
	private ObservableCollection<CollectedStepBindableSource>? _nodesSortedByEliminationCount;

	/// <summary>
	/// A collection sorted by cell index value.
	/// </summary>
	private ObservableCollection<CollectedStepBindableSource>? _nodesSortedByCell;


	/// <summary>
	/// Initializes a <see cref="StepCollecting"/> instance.
	/// </summary>
	public StepCollecting() => InitializeComponent();


	/// <inheritdoc/>
	public AnalyzePage BasePage { get; set; } = null!;

	/// <summary>
	/// Indicates the tree view items source.
	/// </summary>
	[DependencyProperty]
	internal partial ObservableCollection<CollectedStepBindableSource>? TreeViewItemsSource { get; set; }

	/// <inheritdoc/>
	AnalysisResult? IAnalyzerTab.AnalysisResult { get; set; }


	/// <summary>
	/// Converts the specified collection into the target view source collection.
	/// </summary>
	/// <param name="collection">The raw collection.</param>
	/// <returns>The collection that can be used as view source.</returns>
	private void CollectSteps(Step[] collection)
	{
		var displayItems = Application.Current.AsApp().Preference.UIPreferences.StepDisplayItems;
		var converter = App.Converter;
		_nodesSortedByTechnique = [
			..
			from step in collection
			let technique = step.Code
			orderby step.DifficultyLevel, technique.Group, technique
			group step by step.GetName(App.CurrentCulture) into stepsGroupedByName
			let name = stepsGroupedByName.Key
			select rootOrIntermediateItems(name, g(stepsGroupedByName, displayItems))
		];
		_nodesSortedByEliminationCount = [
			..
			from step in collection
			let sortKey = step.IsAssignment switch { true => 1, false => 2, null => 3 }
			let conclusions = new HashSet<Conclusion>(step.Conclusions.ToArray()) // step.Conclusions make contain duplicate items
			let conclusionsCount = conclusions.Count
			orderby sortKey, conclusionsCount descending
			group step by (ConclusionTypeSortKey: sortKey, Count: conclusionsCount) into stepsGroupedByConclusion
			let keyPair = stepsGroupedByConclusion.Key
			let sortkey = keyPair.ConclusionTypeSortKey
			let conclusionsCount = keyPair.Count
			let segment = sortkey switch { 1 => nameof(Assignment), 2 => nameof(Elimination), _ => "Both" }
			let format = SR.Get("AnalyzePage_ConclusionsCountIs", App.CurrentCulture)
			let pluralSuffix = conclusionsCount == 1 ? string.Empty : SR.Get("_PluralSuffix", App.CurrentCulture)
			let conclusionTypeString = SR.Get($"AnalyzePage_ConclusionType_{segment}", App.CurrentCulture)
			let displayKey = string.Format( format, [conclusionsCount, pluralSuffix, conclusionTypeString])
			select rootOrIntermediateItems(displayKey, g(stepsGroupedByConclusion, displayItems))
		];
		_nodesSortedByCell = [
			..
			from step in collection
			let cells = from conclusion in step.Conclusions.Span select conclusion.Cell
			from cell in cells
			orderby cell
			group step by cell into stepsGroupedByCell
			let cell = stepsGroupedByCell.Key
			select rootOrIntermediateItems(converter.CellConverter(in cell.AsCellMap()), g(stepsGroupedByCell, displayItems))
		];


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static CollectedStepBindableSource leafItems(Step step, StepTooltipDisplayItems displayItems)
			=> new(
				step.ToSimpleString(App.CurrentCulture),
				step,
				null,
				AnalyzeConversion.GetInlinesOfTooltip(new() { DisplayItems = displayItems, InterimStep = step })
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static CollectedStepBindableSource rootOrIntermediateItems(string displayKey, IEnumerable<CollectedStepBindableSource> leafItems)
			=> new(displayKey, null, leafItems, null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static IEnumerable<CollectedStepBindableSource> g<TKey>(IGrouping<TKey, Step> stepsGrouped, StepTooltipDisplayItems displayItems)
			=> from step in stepsGrouped orderby step.DifficultyLevel, step.Difficulty select leafItems(step, displayItems);
	}


#if false
	private void TechniqueGroupView_StepApplied(TechniqueGroupView sender, TechniqueGroupViewStepAppliedEventArgs e)
	{
		var appliedPuzzle = BasePage.SudokuPane.Puzzle;
		appliedPuzzle.Apply(e.ChosenStep);

		BasePage.SudokuPane.Puzzle = appliedPuzzle;
	}
#endif

	private async void CollectButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var grid = BasePage.SudokuPane.Puzzle;
		if (!grid.IsValid)
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

		CollectButton.IsEnabled = false;
		BasePage.IsGathererLaunched = true;
		TreeViewItemsSource = null;

		var textFormat = SR.Get("AnalyzePage_AnalyzerProgress", App.CurrentCulture);
		using var cts = new CancellationTokenSource();
		var uiPref = Application.Current.AsApp().Preference.UIPreferences;
		var analysisPref = Application.Current.AsApp().Preference.AnalysisPreferences;
		var collector = Application.Current.AsApp()
			.Collector
			.WithMaxSteps(analysisPref.CollectorMaxStepsCollected)
			.WithSameLevelConfiguration((CollectorDifficultyLevelMode)analysisPref.DifficultyLevelMode)
			.WithStepSearchers(Application.Current.AsApp().GetStepSearchers())
			.WithRuntimeIdentifierSetters(BasePage.SudokuPane)
			.WithUserDefinedOptions(App.CreateStepSearcherOptions());
		BasePage._ctsForAnalyzingRelatedOperations = cts;

		try
		{
			if (await Task.Run(collectCore) is { } result)
			{
				CollectSteps(result);
			}
		}
		catch (TaskCanceledException)
		{
		}
		finally
		{
			BasePage._ctsForAnalyzingRelatedOperations = null;
			CollectButton.IsEnabled = true;
			BasePage.IsGathererLaunched = false;

			PageSelector.SelectedIndex = 0;

			if (isSupportedTaskbarProgressChanging)
			{
				TaskbarProgress.SetState(handle, TBPFLAG.TBPF_NORMAL);
				TaskbarProgress.SetValue(handle, 0, 100);
			}
		}


		Step[] collectCore()
		{
			lock (AnalyzingRelatedSyncRoot)
			{
				return collector.Collect(
					grid,
					new Progress<StepGathererProgressPresenter>(
						progress => DispatcherQueue.TryEnqueue(
							() =>
							{
								var (stepSearcherName, percent) = progress;
								BasePage.ProgressPercent = percent * 100;
								BasePage.AnalyzeProgressLabel.Text = string.Format(textFormat, percent);
								BasePage.AnalyzeStepSearcherNameLabel.Text = stepSearcherName;

								if (isSupportedTaskbarProgressChanging)
								{
									TaskbarProgress.SetValue(handle, BasePage.ProgressPercent, 100);
								}
							}
						)
					),
					cts.Token
				).ToArray();
			}
		}
	}

	private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
		=> TreeViewItemsSource = PageSelector.Items.IndexOf(PageSelector.SelectedItem) switch
		{
			0 => _nodesSortedByTechnique,
			1 => _nodesSortedByEliminationCount,
			2 => _nodesSortedByCell,
			_ => null
		};

	private void MainTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
	{
		if (args.InvokedItem is CollectedStepBindableSource { Step: { } step })
		{
			BasePage.VisualUnit = step;
		}
	}
}
