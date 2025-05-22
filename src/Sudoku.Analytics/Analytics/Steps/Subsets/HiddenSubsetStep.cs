namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Hidden Subset</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="SubsetStep.House" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="SubsetStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="SubsetStep.DigitsMask" path="/summary"/></param>
/// <param name="isLocked"><inheritdoc cref="IsLocked" path="/summary"/></param>
public sealed class HiddenSubsetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House house,
	in CellMap cells,
	Mask digitsMask,
	bool isLocked
) : SubsetStep(conclusions, views, options, house, cells, digitsMask)
{
	/// <summary>
	/// Indicates which locked type this subset is. The cases are as belows:
	/// <list type="table">
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>The subset is a locked hidden subset.</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>The subset is a normal hidden subset without any extra locked candidates.</description>
	/// </item>
	/// </list>
	/// </summary>
	public bool IsLocked { get; } = isLocked;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 4;

	/// <inheritdoc/>
	public override Technique Code
		=> (IsLocked, Size) switch
		{
			(true, 2) => Technique.LockedHiddenPair,
			(_, 2) => Technique.HiddenPair,
			(true, 3) => Technique.LockedHiddenTriple,
			(_, 3) => Technique.HiddenTriple,
			(_, 4) => Technique.HiddenQuadruple
		};

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, HouseStr]), new(SR.ChineseLanguage, [DigitStr, HouseStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_HiddenSubsetSizeFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! switch { 2 => 0, 3 => 6, 4 => 20 }
			),
			Factor.Create(
				"Factor_HiddenSubsetIsLockedFactor",
				[nameof(IsLocked), nameof(Size)],
				GetType(),
				static args => (bool)args![0]! ? (int)args![1]! switch { 2 => -12, 3 => -13 } : 0
			)
		];

	private string DigitStr => Options.Converter.DigitConverter(DigitsMask);

	private string HouseStr => Options.Converter.HouseConverter(1 << House);
}
