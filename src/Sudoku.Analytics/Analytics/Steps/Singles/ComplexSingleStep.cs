namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Complex Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="basedOn"><inheritdoc cref="BasedOn" path="/summary"/></param>
/// <param name="indirectTechniques"><inheritdoc cref="IndirectTechniques" path="/summary"/></param>
public abstract class ComplexSingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype,
	Technique basedOn,
	Technique[][] indirectTechniques
) : PartialPencilmarkingStep(conclusions, views, options, cell, digit, subtype)
{
	/// <summary>
	/// Indicates the single technique that is based on.
	/// </summary>
	public Technique BasedOn { get; } = basedOn;

	/// <summary>
	/// <para>Indicates the indirect techniques used in this pattern.</para>
	/// <para>
	/// This value is an array of array of <see cref="Technique"/> instances,
	/// describing the detail usage on the complex combination of indirect technique usages. For example:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term>[[<see cref="Technique.Pointing"/>, <see cref="Technique.Claiming"/>]]</term>
	/// <description>The naked single will use a pointing and a claiming technique in one step</description>
	/// </item>
	/// <item>
	/// <term>[[<see cref="Technique.Pointing"/>], [<see cref="Technique.Claiming"/>]]</term>
	/// <description>We should apply a pointing firstly, and then claiming appears, then naked single appears</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	public Technique[][] IndirectTechniques { get; } = indirectTechniques;
}
