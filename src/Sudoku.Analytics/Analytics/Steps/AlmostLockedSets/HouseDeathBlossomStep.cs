namespace Sudoku.Analytics.Steps.AlmostLockedSets;

/// <summary>
/// Provides with a step that is a <b>Death Blossom (House Blooming)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="branches"><inheritdoc cref="Branches" path="/summary"/></param>
/// <param name="zDigitsMask"><inheritdoc cref="ZDigitsMask" path="/summary"/></param>
public sealed class HouseDeathBlossomStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House house,
	Digit digit,
	HouseBlossomBranchCollection branches,
	Mask zDigitsMask
) : DeathBlossomStep(conclusions, views, options), IBranchTrait, IDeathBlossomCollection<HouseBlossomBranchCollection, House>
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override Technique Code => Technique.HouseDeathBlossom;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Branches.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [HouseStr, BranchesStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [HouseStr, BranchesStr(SR.ChineseLanguage)])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_HouseDeathBlossomPetalsCountFactor",
				[nameof(IBranchTrait.BranchesCount)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <summary>
	/// Indicates the pivot house.
	/// </summary>
	public House House { get; } = house;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the digits mask as eliminations.
	/// </summary>
	public Mask ZDigitsMask { get; } = zDigitsMask;

	/// <summary>
	/// Indicates the branches.
	/// </summary>
	public HouseBlossomBranchCollection Branches { get; } = branches;

	/// <inheritdoc/>
	int IBranchTrait.BranchesCount => Branches.Count;

	private string HouseStr => Options.Converter.HouseConverter(1 << House);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is HouseDeathBlossomStep comparer
		&& (House, Digit, Branches) == (comparer.House, comparer.Digit, comparer.Branches);

	private string BranchesStr(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		return string.Join(
			SR.Get("Comma", culture),
			from b in Branches select $"{Options.Converter.CellConverter(in b.Key.AsCellMap())} - {b.Value}"
		);
	}
}
