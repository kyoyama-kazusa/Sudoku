namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Almost Locked Sets</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public closed class AlmostLockedSetsStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	AdvancedStep(conclusions, views, options);
