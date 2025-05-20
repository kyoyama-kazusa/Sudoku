namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a bindable source for collected steps.
/// </summary>
/// <param name="title"><inheritdoc cref="Title" path="/summary"/></param>
/// <param name="step"><inheritdoc cref="Step" path="/summary"/></param>
/// <param name="children"><inheritdoc cref="Children" path="/summary"/></param>
/// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
[method: SetsRequiredMembers]
internal sealed class CollectedStepBindableSource(
	string title,
	Step? step,
	IEnumerable<CollectedStepBindableSource>? children,
	IEnumerable<Inline>? description
)
{
	/// <summary>
	/// Indicates the title.
	/// </summary>
	public required string Title { get; set; } = title;

	/// <summary>
	/// Indicates the step.
	/// </summary>
	public required Step? Step { get; set; } = step;

	/// <summary>
	/// Indicates the values.
	/// </summary>
	public required IEnumerable<CollectedStepBindableSource>? Children { get; set; } = children;

	/// <summary>
	/// Indicates the description.
	/// </summary>
	public required IEnumerable<Inline>? Description { get; set; } = description;
}
