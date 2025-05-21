namespace Sudoku.Analytics.Steps.AlmostLockedSets;

/// <summary>
/// Provides with a step that is a <b>n-Times ALS Death Blossom</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="nTimesAlmostLockedSetsDigitsMask">Indicates the digits A^nLS used.</param>
/// <param name="nTimesAlmostLockedSetsCells">Indicates the A^nLS cells used.</param>
/// <param name="branches">Indicates the detail branches.</param>
/// <param name="freedomDegree">Indicates the freedom degree of this A^nLS.</param>
public sealed partial class NTimesAlmostLockedSetsDeathBlossomStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Mask nTimesAlmostLockedSetsDigitsMask,
	[Property] in CellMap nTimesAlmostLockedSetsCells,
	[Property] NTimesAlmostLockedSetsBlossomBranchCollection branches,
	[Property] int freedomDegree
) :
	DeathBlossomStep(conclusions, views, options),
	IBranchTrait,
	IDeathBlossomCollection<NTimesAlmostLockedSetsBlossomBranchCollection, CandidateMap>
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 5;

	/// <inheritdoc/>
	public override Technique Code => Technique.NTimesAlmostLockedSetsDeathBlossom;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Branches.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [FreedomDegreeStr, CellsStr, DigitsStr, BranchesStr(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [FreedomDegreeStr, CellsStr, DigitsStr, BranchesStr(SR.ChineseLanguage)])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_NTimesAlmostLockedSetsDeathBlossomPetalsCountFactor",
				[nameof(IBranchTrait.BranchesCount)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int IBranchTrait.BranchesCount => Branches.Count;

	private string FreedomDegreeStr => FreedomDegree.ToString();

	private string CellsStr => Options.Converter.CellConverter(NTimesAlmostLockedSetsCells);

	private string DigitsStr => Options.Converter.DigitConverter(NTimesAlmostLockedSetsDigitsMask);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is NTimesAlmostLockedSetsDeathBlossomStep comparer
		&& (NTimesAlmostLockedSetsCells, NTimesAlmostLockedSetsDigitsMask, Branches) == (comparer.NTimesAlmostLockedSetsCells, comparer.NTimesAlmostLockedSetsDigitsMask, comparer.Branches);

	/// <inheritdoc/>
	public override int CompareTo(Step? other)
	{
		if (other is not NTimesAlmostLockedSetsDeathBlossomStep comparer)
		{
			return -1;
		}

		if (Branches.Count.CompareTo(comparer.Branches.Count) is var r1 and not 0)
		{
			return r1;
		}

		var leftCellsCount = Branches.Values.Sum(alsCellsCountSelector);
		var rightCellsCount = comparer.Branches.Values.Sum(alsCellsCountSelector);
		if (leftCellsCount.CompareTo(rightCellsCount) is var r2 and not 0)
		{
			return r2;
		}

		if (Conclusions.Length.CompareTo(comparer.Conclusions.Length) is var r3 and not 0)
		{
			return r3;
		}

		if (NTimesAlmostLockedSetsCells.CompareTo(comparer.NTimesAlmostLockedSetsCells) is var r4 and not 0)
		{
			return r4;
		}

		if (NTimesAlmostLockedSetsDigitsMask.CompareTo(comparer.NTimesAlmostLockedSetsDigitsMask) is var r5 and not 0)
		{
			return r5;
		}

		foreach (var branchCandidates in Branches.Keys)
		{
			if (Branches[branchCandidates].CompareTo(comparer.Branches[branchCandidates]) is var r6 and not 0)
			{
				return r6;
			}
		}

		return 0;


		static int alsCellsCountSelector(AlmostLockedSetPattern s) => s.Cells.Count;
	}

	private string BranchesStr(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		return string.Join(
			SR.Get("Comma", culture),
			from branch in Branches
			let p = Options.Converter.CandidateConverter(branch.Key)
			let q = branch.Value.ToString(Options.Converter)
			select $"{p} - {q}"
		);
	}
}
