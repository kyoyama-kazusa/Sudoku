namespace Sudoku.Analytics.Steps.AlmostLockedSets;

/// <summary>
/// Provides with a step that is a <b>Death Blossom</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pivot"><inheritdoc cref="Pivot" path="/summary"/></param>
/// <param name="branches"><inheritdoc cref="Branches" path="/summary"/></param>
/// <param name="zDigitsMask"><inheritdoc cref="ZDigitsMask" path="/summary"/></param>
public sealed class NormalDeathBlossomStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell pivot,
	NormalBlossomBranchCollection branches,
	Mask zDigitsMask
) : DeathBlossomStep(conclusions, views, options), IBranchTrait, IDeathBlossomCollection<NormalBlossomBranchCollection, Digit>
{
	/// <inheritdoc/>
	public override Technique Code => Technique.DeathBlossom;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Branches.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [PivotStr, BranchesStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [PivotStr, BranchesStr(SR.ChineseLanguage)])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BasicDeathBlossomPetalsCountFactor",
				[nameof(IBranchTrait.BranchesCount)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <summary>
	/// Indicates the pivot cell.
	/// </summary>
	public Cell Pivot { get; } = pivot;

	/// <summary>
	/// Indicates the digits mask as eliminations.
	/// </summary>
	public Mask ZDigitsMask { get; } = zDigitsMask;

	/// <summary>
	/// Indicates the branches.
	/// </summary>
	public NormalBlossomBranchCollection Branches { get; } = branches;

	/// <inheritdoc/>
	int IBranchTrait.BranchesCount => Branches.Count;

	private string PivotStr => Options.Converter.CellConverter(in Pivot.AsCellMap());


	private string BranchesStr(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		return string.Join(
			SR.Get("Comma", culture),
			from branch in Branches
			select $"{Options.Converter.DigitConverter((Mask)(1 << branch.Key))} - {branch.Value}"
		);
	}
}
