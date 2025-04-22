namespace SudokuStudio.Views.Pages.Operation;

/// <summary>
/// Indicates the basic operation command bar.
/// </summary>
public sealed partial class BasicOperation : Page, IOperationProviderPage
{
	/// <summary>
	/// Initializes a <see cref="BasicOperation"/> instance.
	/// </summary>
	public BasicOperation() => InitializeComponent();


	/// <inheritdoc/>
	public AnalyzePage BasePage { get; set; } = null!;


	/// <summary>
	/// The method called by <see cref="Page_Loaded(object, RoutedEventArgs)"/> and <see cref="Page_Unloaded(object, RoutedEventArgs)"/>.
	/// </summary>
	private void OnSaveFileFailed(AnalyzePage _, SaveFileFailedEventArgs e)
		=> (e.Reason switch { SaveFileFailedReason.UnsnappingFailed => ErrorDialog_ProgramIsSnapped }).IsOpen = true;

	/// <summary>
	/// The method called by <see cref="Page_Loaded(object, RoutedEventArgs)"/> and <see cref="Page_Unloaded(object, RoutedEventArgs)"/>.
	/// </summary>
	private void OnOpenFileFailed(AnalyzePage _, OpenFileFailedEventArgs e)
		=> (
			e.Reason switch
			{
				OpenFileFailedReason.UnsnappingFailed => ErrorDialog_ProgramIsSnapped,
				OpenFileFailedReason.FileIsEmpty => ErrorDialog_FileIsEmpty,
				OpenFileFailedReason.FileIsTooLarge => ErrorDialog_FileIsOversized,
				OpenFileFailedReason.FileCannotBeParsed => ErrorDialog_FileCannotBeParsed
			}
		).IsOpen = true;

	/// <summary>
	/// Try to get all possible format flags.
	/// </summary>
	/// <param name="panel">The panel.</param>
	/// <returns>All format flags.</returns>
	private SudokuFormatFlags GetFormatFlags(StackPanel panel)
		=> (
			from children in panel.Children
			where children is CheckBox { Tag: int and not 0, IsChecked: true }
			select (SudokuFormatFlags)(int)((CheckBox)children).Tag!
		).Aggregate(SudokuFormatFlags.None, @delegate.EnumFlagMerger);


	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		BasePage.OpenFileFailed += OnOpenFileFailed;
		BasePage.SaveFileFailed += OnSaveFileFailed;
	}

	private void Page_Unloaded(object sender, RoutedEventArgs e)
	{
		BasePage.OpenFileFailed -= OnOpenFileFailed;
		BasePage.SaveFileFailed -= OnSaveFileFailed;
	}

	private void SaveAsButton_Click(object sender, RoutedEventArgs e) => Dialog_FormatChoosing.IsOpen = true;

	private void CopyButton_Click(object sender, RoutedEventArgs e) => BasePage.CopySudokuGridText();

	private async void CopyPictureButton_ClickAsync(object sender, RoutedEventArgs e) => await BasePage.CopySudokuGridControlAsSnapshotAsync();

	private async void PasteButton_ClickAsync(object sender, RoutedEventArgs e) => await BasePage.PasteCodeToSudokuGridAsync();

	private void ResetButton_Click(object sender, RoutedEventArgs e)
	{
		BasePage.SudokuPane.Puzzle = BasePage.SudokuPane.Puzzle.ResetGrid;
		BasePage.ClearAnalyzeTabsData();
		BasePage.VisualUnit = null;
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e) => Dialog_AreYouSureToReturnToEmpty.IsOpen = true;

	private void UndoButton_Click(object sender, RoutedEventArgs e) => BasePage.SudokuPane.UndoStep();

	private void RedoButton_Click(object sender, RoutedEventArgs e) => BasePage.SudokuPane.RedoStep();

	private async void Dialog_FormatChoosing_ActionButtonClickAsync(TeachingTip sender, object args)
	{
		if (GetFormatFlags(FormatGroupPanel) is not (var flags and not 0))
		{
			return;
		}

		if (await BasePage.SaveFileInternalAsync(from flag in flags.AllFlags.ToArray() select flag.Converter))
		{
			Dialog_FormatChoosing.IsOpen = false;
		}
	}

	private void Dialog_AreYouSureToReturnToEmpty_ActionButtonClick(TeachingTip sender, object args)
	{
		BasePage.SudokuPane.Puzzle = Grid.Empty;
		BasePage.ClearAnalyzeTabsData();
		BasePage.VisualUnit = null;

		Dialog_AreYouSureToReturnToEmpty.IsOpen = false;
	}

	private async void SaveFileButton_ClickAsync(object sender, RoutedEventArgs e) => await BasePage.SaveFileInternalAsync();

	private async void OpenFileButton_ClickAsync(object sender, RoutedEventArgs e) => await BasePage.OpenFileInternalAsync();
}
