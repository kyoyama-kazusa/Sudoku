namespace SudokuStudio.Views.Pages.Operation;

/// <summary>
/// Attribute checking operation page.
/// </summary>
public sealed partial class AttributeCheckingOperation : Page, IOperationProviderPage
{
	/// <summary>
	/// Initializes an <see cref="AttributeCheckingOperation"/> instance.
	/// </summary>
	public AttributeCheckingOperation() => InitializeComponent();


	/// <inheritdoc/>
	public AnalyzePage BasePage { get; set; } = null!;


	private async void BackdoorButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var puzzle = BasePage.SudokuPane.Puzzle;
		if (!puzzle.IsValid)
		{
			ErrorDialog_PuzzleIsInvalid.Target = BackdoorButton;
			ErrorDialog_PuzzleIsInvalid.IsOpen = true;
			return;
		}

		if (puzzle.PuzzleType == SudokuType.Sukaku)
		{
			ErrorDialog_SukakuIsNotSupportedForThisFunction.Target = BackdoorButton;
			ErrorDialog_SukakuIsNotSupportedForThisFunction.IsOpen = true;
			return;
		}

		var backdoors = await Task.Run(() =>
		{
			lock (AnalyzingRelatedSyncRoot)
			{
				BackdoorInferrer.TryInfer(puzzle, out var result);
				return result.Candidates.ToArray();
			}
		});

		BasePage.VisualUnit = new BackdoorVisualUnit([
			..
			from backdoor in backdoors
			let type = backdoor.ConclusionType
			let candidate = backdoor.Candidate
			select new CandidateViewNode(type == Assignment ? ColorIdentifierKind.Assignment : ColorIdentifierKind.Elimination, candidate)
		]);
	}

	private async void TrueCandidateButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var puzzle = BasePage.SudokuPane.Puzzle;
		if (!puzzle.IsValid)
		{
			ErrorDialog_PuzzleIsInvalid.Target = TrueCandidateButton;
			ErrorDialog_PuzzleIsInvalid.IsOpen = true;
			return;
		}

		if (puzzle.PuzzleType == SudokuType.Sukaku)
		{
			ErrorDialog_SukakuIsNotSupportedForThisFunction.Target = TrueCandidateButton;
			ErrorDialog_SukakuIsNotSupportedForThisFunction.IsOpen = true;
			return;
		}

		var trueCandidates = await Task.Run(() =>
		{
			lock (AnalyzingRelatedSyncRoot)
			{
				return TrueCandidate.GetAllTrueCandidates(puzzle);
			}
		});
		if (trueCandidates.Count == 0)
		{
			ErrorDialog_PuzzleIsNotBugMultipleGrid.IsOpen = true;
			return;
		}

		BasePage.VisualUnit = new TrueCandidateVisualUnit([
			..
			from candidate in trueCandidates
			select new CandidateViewNode(ColorIdentifierKind.Assignment, candidate)
		]);
	}

	private void DisorderedIttoryuButton_Click(object sender, RoutedEventArgs e)
	{
		var puzzle = BasePage.SudokuPane.Puzzle;
		if (!puzzle.IsValid)
		{
			ErrorDialog_PuzzleIsInvalid.Target = DisorderedIttoryuButton;
			ErrorDialog_PuzzleIsInvalid.IsOpen = true;
			return;
		}

		if (puzzle.PuzzleType == SudokuType.Sukaku)
		{
			ErrorDialog_SukakuIsNotSupportedForThisFunction.Target = DisorderedIttoryuButton;
			ErrorDialog_SukakuIsNotSupportedForThisFunction.IsOpen = true;
			return;
		}

		TechniqueSet techniqueSet = [.. Application.Current.AsApp().Preference.AnalysisPreferences.IttoryuSupportedTechniques];
		var finder = new DisorderedIttoryuFinder(techniqueSet);
		(InfoDialog_DisorderedIttoryuDigitSequence.Subtitle, InfoDialog_DisorderedIttoryuDigitSequence.IsOpen) =
			finder.FindPath(puzzle) is { Digits.Length: not 0 } path
				? (string.Format(SR.Get("AnalyzePage_DisorderedIttoryuOrderIs", App.CurrentCulture), path.ToString()), true)
				: (SR.Get("AnalyzePage_DisorderedIttoryuDoesNotExist", App.CurrentCulture), true);
	}
}

/// <summary>
/// Defines a backdoor visual unit.
/// </summary>
/// <param name="_view">The view.</param>
file sealed class BackdoorVisualUnit(View _view) : IDrawable
{
	/// <inheritdoc/>
	public ReadOnlyMemory<View> Views { get; } = (View[])[_view];

	/// <inheritdoc/>
	ReadOnlyMemory<Conclusion> IDrawable.Conclusions { get; } = ReadOnlyMemory<Conclusion>.Empty;
}

/// <summary>
/// Defines a true-candidate visual unit.
/// </summary>
/// <param name="_view">The view.</param>
file sealed class TrueCandidateVisualUnit(View _view) : IDrawable
{
	/// <inheritdoc/>
	public ReadOnlyMemory<View> Views { get; } = (View[])[_view];

	/// <inheritdoc/>
	ReadOnlyMemory<Conclusion> IDrawable.Conclusions { get; } = ReadOnlyMemory<Conclusion>.Empty;
}
