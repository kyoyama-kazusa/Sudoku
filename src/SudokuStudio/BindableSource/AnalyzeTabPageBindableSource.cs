namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a type that binds with a analyze tab page type that implements <see cref="IAnalyzerTab"/>.
/// </summary>
/// <param name="header"><inheritdoc cref="Header" path="/summary"/></param>
/// <param name="iconSource"><inheritdoc cref="IconSource" path="/summary"/></param>
/// <param name="page"><inheritdoc cref="Page" path="/summary"/></param>
/// <seealso cref="IAnalyzerTab"/>
internal sealed class AnalyzeTabPageBindableSource(string header, IconSource iconSource, IAnalyzerTab page)
{
	/// <summary>
	/// Indicates the header.
	/// </summary>
	public string Header { get; init; } = header;

	/// <summary>
	/// Indicates the icon source.
	/// </summary>
	public IconSource IconSource { get; init; } = iconSource;

	/// <summary>
	/// Indicates the tab page.
	/// </summary>
	public IAnalyzerTab Page { get; init; } = page;
}
