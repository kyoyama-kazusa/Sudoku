namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Intersection</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public closed class IntersectionStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	AdvancedStep(conclusions, views, options);
