namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Last Digit</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="HiddenSingleStep.House" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="ILastingTrait.Lasting" path="/summary"/></param>
public sealed class LastDigitStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	House house,
	int lasting
) : HiddenSingleStep(conclusions, views, options, cell, digit, house, true, lasting, SingleSubtype.LastDigit, null);
