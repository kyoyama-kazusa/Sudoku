namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Naked Subset</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="SubsetStep.House" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="SubsetStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="SubsetStep.DigitsMask" path="/summary"/></param>
/// <param name="isLocked"><inheritdoc cref="IsLocked" path="/summary"/></param>
public sealed class NakedSubsetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House house,
	in CellMap cells,
	Mask digitsMask,
	bool? isLocked
) : SubsetStep(conclusions, views, options, house, cells, digitsMask)
{
	/// <summary>
	/// Indicates which locked type this subset is. The cases are as follows:
	/// <list type="table">
	/// <item>
	/// <term><see langword="true" /></term>
	/// <description>The subset is a locked subset.</description>
	/// </item>
	/// <item>
	/// <term><see langword="false" /></term>
	/// <description>The subset is a naked subset with at least one extra locked candidate.</description>
	/// </item>
	/// <item>
	/// <term><see langword="null" /></term>
	/// <description>The subset is a normal naked subset without any extra locked candidates.</description>
	/// </item>
	/// </list>
	/// </summary>
	public bool? IsLocked { get; } = isLocked;

	/// <inheritdoc/>
	public override Technique Code
		=> (IsLocked, Size) switch
		{
			(true, 2) => Technique.LockedPair,
			(false, 2) => Technique.NakedPairPlus,
			(_, 2) => Technique.NakedPair,
			(true, 3) => Technique.LockedTriple,
			(false, 3) => Technique.NakedTriplePlus,
			(_, 3) => Technique.NakedTriple,
			(false, 4) => Technique.NakedQuadruplePlus,
			(null, 4) => Technique.NakedQuadruple
		};

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, HouseStr]), new(SR.ChineseLanguage, [DigitsStr, HouseStr, SubsetName(SR.ChineseLanguage)])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_NakedSubsetSizeFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! switch { 2 => 0, 3 => 6, 4 => 20 }
			),
			Factor.Create(
				"Factor_NakedSubsetIsLockedFactor",
				[nameof(IsLocked), nameof(Size)],
				GetType(),
				static args => ((bool?)args![0]!, (int)args![1]!) switch
				{
					(true, 2) => -10,
					(true, 3) => -11,
					(false, _) => 1,
					_ => 0
				}
			)
		];

	private string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private string HouseStr => Options.Converter.HouseConverter(1 << House);


	private string SubsetName(string cultureName) => SR.Get($"SubsetNamesSize{Size}", new(cultureName));
}
