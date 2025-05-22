namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Irregular Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class IrregularWingStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	WingStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates whether the pattern is symmetric.
	/// </summary>
	public abstract bool IsSymmetricPattern { get; }

	/// <summary>
	/// Indicates whether the pattern is grouped.
	/// </summary>
	public abstract bool IsGrouped { get; }

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_IrregularWingIsGroupedFactor",
				[nameof(IsGrouped)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];
}
