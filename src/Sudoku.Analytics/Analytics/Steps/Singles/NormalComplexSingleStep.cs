namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Complex Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
/// <param name="basedOn"><inheritdoc cref="ComplexSingleStep.BasedOn" path="/summary"/></param>
/// <param name="indirectTechniques"><inheritdoc cref="ComplexSingleStep.IndirectTechniques" path="/summary"/></param>
public sealed class NormalComplexSingleStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype,
	Technique basedOn,
	Technique[][] indirectTechniques
) : ComplexSingleStep(conclusions, views, options, cell, digit, subtype, basedOn, indirectTechniques)
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> BasedOn switch
		{
			Technique.FullHouse => 10,
			Technique.CrosshatchingBlock => 12,
			Technique.CrosshatchingRow => 15,
			Technique.CrosshatchingColumn => 15,
			Technique.NakedSingle => 23
		};

	/// <inheritdoc/>
	public override Technique Code
		=> BasedOn switch
		{
			Technique.FullHouse => Technique.ComplexFullHouse,
			Technique.CrosshatchingBlock => Technique.ComplexCrosshatchingBlock,
			Technique.CrosshatchingRow => Technique.ComplexCrosshatchingRow,
			Technique.CrosshatchingColumn => Technique.ComplexCrosshatchingColumn,
			Technique.NakedSingle => Technique.ComplexNakedSingle
		};

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [TechniqueNotation(SR.EnglishLanguage)]),
			new(SR.ChineseLanguage, [TechniqueNotation(SR.ChineseLanguage)])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ComplexSingleFactor",
				[nameof(IndirectTechniques)],
				GetType(),
				static args => ComplexTechniqueUsages.GetComplexityDifficulty((Technique[][])args![0]!)
			)
		];


	/// <inheritdoc/>
	public override int CompareTo(Step? other)
	{
		if (other is not NormalComplexSingleStep comparer)
		{
			return -1;
		}

		var (countThis, countOther) = (IndirectTechniques.Length, comparer.IndirectTechniques.Length);
		if (countThis != countOther)
		{
			return countThis > countOther ? 1 : -1;
		}

		var (sortKeyThis, sortKeyOther) = (0, 0);
		for (var i = 0; i < IndirectTechniques.Length; i++)
		{
			sortKeyThis += IndirectTechniques[i].Sum(getSortKey);
			sortKeyOther += comparer.IndirectTechniques[i].Sum(getSortKey);
		}
		return sortKeyThis.CompareTo(sortKeyOther);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int getSortKey(Technique technique)
			=> technique switch
			{
				Technique.Pointing => 1,
				Technique.Claiming => 2,
				Technique.LockedPair or Technique.LockedHiddenPair => 3,
				Technique.LockedTriple or Technique.LockedHiddenTriple => 4,
				Technique.NakedPair or Technique.HiddenPair or Technique.NakedPairPlus => 5,
				Technique.NakedTriple or Technique.HiddenTriple or Technique.NakedTriplePlus => 6,
				Technique.NakedQuadruple or Technique.HiddenQuadruple or Technique.NakedQuadruplePlus => 7,
				_ => 10
			};
	}

	private string TechniqueNotation(string cultureName)
	{
		var culture = new CultureInfo(cultureName);
		var comma = SR.Get("_Token_Comma", culture);
		return string.Join(
			" -> ",
			from techniqueGroup in IndirectTechniques
			let tt = string.Join(comma, from subtechnique in techniqueGroup select subtechnique.GetName(culture))
			select techniqueGroup.Length == 1 ? tt : $"({tt})"
		);
	}
}
