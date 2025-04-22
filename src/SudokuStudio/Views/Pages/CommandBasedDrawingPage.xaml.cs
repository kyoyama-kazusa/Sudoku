namespace SudokuStudio.Views.Pages;

/// <summary>
/// Represents command-based drawing page.
/// </summary>
public sealed partial class CommandBasedDrawingPage : Page
{
	/// <summary>
	/// Indicates the load keyword.
	/// </summary>
	private const string LoadKeyword = "load";

	/// <summary>
	/// Indicates candidates on syntax.
	/// </summary>
	private const string CandidatesOnSyntax = "+c";

	/// <summary>
	/// Indicates candidates off syntax.
	/// </summary>
	private const string CandidatesOffSyntax = "-c";

	/// <summary>
	/// Indicates the highlight JSON.
	/// </summary>
	[StringSyntax(StringSyntaxAttribute.Json)]
	private const string HighlightJson = """
		{
			"Highlights": [
				{
					"Pattern": "load|candidate|cell|house|chute|baba|icon|link",
					"ColorDark": "#569cd6",
					"ColorLight": "#0000ff"
				},
				{
					"Pattern": "cell|chain|conjugate|circle|cross|diamond|heart|square|star|triangle",
					"ColorDark": "#569cd6",
					"ColorLight": "#0000ff"
				},
				{
					"Pattern": "(?<=load\\s+)[+-]c",
					"ColorDark": "#c0c0c0",
					"ColorLight": "#808080"
				},
				{
					"Pattern": "(?<=!)\\w+|(?<=#)[\\dA-Fa-f]{6}|(?<=#)[\\dA-Fa-f]{8}|(?<=&)(\\d{1,2}|[A-Fa-f])",
					"ColorDark": "#4ec9b0",
					"ColorLight": "#2f91af"
				},
				{
					"Pattern": "(\\:\\d{4}\\:[x\\d]+\\:)?[\\d\\.\\+]{80,}(\\:(\\d{3}\\s+)*\\d{3})?(\\:(\\d{3}\\s+)*\\d{3})?(\\:{2})?",
					"ColorDark": "#d69d85",
					"ColorLight": "#804040"
				}
			],
			"Name": "Command Line Syntax",
			"Filter": "*.*",
			"Description": "Command line syntax.",
			"Author": "Kyouyama Kazusa"
		}
		""";


	/// <summary>
	/// Initializes a <see cref="CommandBasedDrawingPage"/> instance.
	/// </summary>
	public CommandBasedDrawingPage()
	{
		InitializeComponent();

		var result = TextControlBoxNS.TextControlBox.GetSyntaxHighlightingFromJson(HighlightJson);
		if (result.Succeed)
		{
			DrawingCommandTextBox.SyntaxHighlighting = result.SyntaxHighlighting;
		}
	}

	[GeneratedRegex("""load\s+([+-]c\s+)?.+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex LoadCommandPattern { get; }


	/// <summary>
	/// Try to parse command line from control <see cref="DrawingCommandTextBox"/>.
	/// </summary>
	/// <param name="valid">Indicates whether the syntax is valid.</param>
	/// <param name="grid">The grid created.</param>
	/// <param name="showCandidates">Indicates whether candidates should be displayed.</param>
	/// <returns>A <see cref="View"/> instance created.</returns>
	private View? TryParseCommandLine(out bool valid, out Grid? grid, out bool? showCandidates)
	{
		(grid, showCandidates) = (null, null);

		var text = DrawingCommandTextBox.Text;
		if (string.IsNullOrWhiteSpace(text))
		{
			valid = true;
			return null;
		}

		var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		// Find for grid showing command.
		foreach (var line in lines)
		{
			if (!line.StartsWith(LoadKeyword))
			{
				continue;
			}

			// Load keyword found. Check validity of syntax.
			// Syntax:
			//   load_command
			//     : 'load' ' ' ('+' | '-') 'c' ' ' puzzle_string
			//     | 'load' ' ' puzzle_string
			//     ;
			if (LoadCommandPattern.Match(line) is not { Success: true })
			{
				continue;
			}

			// Check candidate showing syntax.
			if (line.Contains(CandidatesOnSyntax) || line.Contains(CandidatesOffSyntax))
			{
				showCandidates = line.Contains(CandidatesOnSyntax);

				var indexStartingGrid = line.IndexOf('c') + 1;
				if (Grid.TryParse(line[indexStartingGrid..], out var g1))
				{
					grid = g1;
					break;
				}

				valid = false;
				return null;
			}

			if (Grid.TryParse(line[(line.IndexOf(' ') + 1)..], out var g2))
			{
				grid = g2;
				break;
			}

			valid = false;
			return null;
		}

		// Parses the command.
		if (grid is not null && showCandidates is null)
		{
			// A rescue: if 'grid' has already parsed, we should set 'showCandidates' to false if the value is unset.
			showCandidates = false;
		}

		var coordinateParser = Application.Current.AsApp().Preference.UIPreferences.ConceptNotationBasedKind.Parser;
		var parser = grid is { } g ? new DrawingCommandParser(in g) : new DrawingCommandParser();
		parser = parser with { CoordinateParser = coordinateParser };
		valid = parser.TryParse(text, out var result);
		return result;
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
	private async Task CopySudokuGridControlAsSnapshotAsync()
	{
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


	private void SudokuPane_Loaded(object sender, RoutedEventArgs e)
	{
		var app = Application.Current.AsApp();
		app.CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);
		app.MainSudokuPane = SudokuPane;
	}

	private void SudokuPane_ActualThemeChanged(FrameworkElement sender, object args)
		=> Application.Current.AsApp().CoverSettingsToSudokuPaneViaApplicationTheme(SudokuPane);

	private void ApplyButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button { Name: nameof(ApplyButton) })
		{
			return;
		}

		var view = TryParseCommandLine(out _, out var nullableGrid, out var nullableShowCandidates);
		if (view is null)
		{
			return;
		}

		// Assign the view onto grid.
		if (nullableGrid is { } grid && nullableShowCandidates is { } showCandidates)
		{
			SudokuPane.Puzzle = grid;
			SudokuPane.DisplayCandidates = showCandidates;
		}
		SudokuPane.ViewUnit = new() { Conclusions = ReadOnlyMemory<Conclusion>.Empty, View = view };
	}

	private async void CopyPictureButton_ClickAsync(object sender, RoutedEventArgs e) => await CopySudokuGridControlAsSnapshotAsync();
}
