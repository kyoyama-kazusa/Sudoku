namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Multi-Branch W-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="leaves"><inheritdoc cref="Leaves" path="/summary"/></param>
/// <param name="root"><inheritdoc cref="Root" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class MultiBranchWWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap leaves,
	in CellMap root,
	House house,
	Mask digitsMask
) : IrregularWingStep(conclusions, views, options), ISizeTrait
{
	/// <inheritdoc/>
	public override bool IsGrouped => false;

	/// <inheritdoc/>
	public override bool IsSymmetricPattern => false;

	/// <inheritdoc/>
	public int Size => Leaves.Count;

	/// <inheritdoc/>
	public override int BaseDifficulty => 44;

	/// <inheritdoc/>
	public override Technique Code => Technique.MultiBranchWWing;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// The leaves of the pattern.
	/// </summary>
	public CellMap Leaves { get; } = leaves;

	/// <summary>
	/// The root cells that corresponds to each leaf.
	/// </summary>
	public CellMap Root { get; } = root;

	/// <summary>
	/// Indicates the house that all cells in <see cref="Root"/> lie in.
	/// </summary>
	public House House { get; } = house;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [LeavesStr, RootStr, HouseStr]), new(SR.ChineseLanguage, [RootStr, HouseStr, LeavesStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_MultiBranchWWingBranchesCountFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! == 3 ? 3 : 0
			)
		];

	private string LeavesStr => Options.Converter.CellConverter(Leaves);

	private string RootStr => Options.Converter.CellConverter(Root);

	private string HouseStr => Options.Converter.HouseConverter(1 << House);
}
