namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a type that can be used for binding as source, for technique page routing.
/// </summary>
public sealed class TechniquePageRoutingBindableSource
{
	/// <summary>
	/// Indicates whether the page is enabled.
	/// </summary>
	[MemberNotNullWhen(true, nameof(RoutingPageTypeName))]
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Indicates the resource name of this technique.
	/// </summary>
	public string Name => Technique.GetName(App.CurrentCulture);

	/// <summary>
	/// Indicates the resource default name (English name) of this technique.
	/// </summary>
	public string OriginalName => Technique.EnglishName;

	/// <summary>
	/// Indicates the routing page type name.
	/// </summary>
	[DisallowNull]
	public string? RoutingPageTypeName { get; set; }

	/// <summary>
	/// Indicates the technique code that the page related to.
	/// </summary>
	public required Technique Technique { get; set; }

	/// <summary>
	/// Indicates the color of the current technique.
	/// </summary>
	public required Color Color { get; set; }
}
