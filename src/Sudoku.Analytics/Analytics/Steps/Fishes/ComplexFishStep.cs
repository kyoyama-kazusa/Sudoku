namespace Sudoku.Analytics.Steps.Fishes;

/// <summary>
/// Provides with a step that is a <b>Complex Fish</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc/></param>
/// <param name="baseSetsMask"><inheritdoc/></param>
/// <param name="coverSetsMask"><inheritdoc/></param>
/// <param name="exofins">
/// Indicates the fins, positioned outside the fish.
/// They will be defined if cover sets cannot be fully covered, with the number of cover sets being equal to the number of base sets.
/// </param>
/// <param name="endofins">
/// Indicates the fins, positioned inside the fish. Generally speaking, they will be defined if they are in multiple base sets.
/// </param>
/// <param name="isFranken">
/// Indicates whether the fish is a Franken fish. If <see langword="true"/>, a Franken fish; otherwise, a Mutant fish.
/// </param>
/// <param name="isSashimi"><inheritdoc/></param>
/// <param name="isCannibalism">Indicates whether the fish contains any cannibalism.</param>
/// <param name="isSiamese"><inheritdoc/></param>
public sealed partial class ComplexFishStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	HouseMask baseSetsMask,
	HouseMask coverSetsMask,
	[Property] in CellMap exofins,
	[Property] in CellMap endofins,
	[Property] bool isFranken,
	bool? isSashimi,
	[Property] bool isCannibalism,
	bool isSiamese = false
) : FishStep(conclusions, views, options, digit, baseSetsMask, coverSetsMask, exofins | endofins, isSashimi, isSiamese)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 32;

	/// <inheritdoc/>
	public override Technique Code => TechniqueNaming.Fish.GetTechnique(this);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [InternalNotation]), new(SR.ChineseLanguage, [InternalNotation])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ComplexFishSizeFactor",
				[nameof(Size)],
				GetType(),
				static args => (int)args![0]! switch { 2 => 0, 3 => 6, 4 => 20, 5 => 33, 6 => 45, 7 => 56, _ => 66 }
			),
			Factor.Create(
				"Factor_ComplexFishIsSashimiFactor",
				[nameof(IsSashimi), nameof(Size)],
				GetType(),
				static args => (bool?)args![0]! switch
				{
					false => (int)args[1]! switch { 2 or 3 or 4 => 2, 5 or 6 or 7 => 3, _ => 4 },
					true => (int)args[1]! switch { 2 or 3 => 3, 4 or 5 => 4, 6 => 5, 7 => 6, _ => 7 },
					_ => 0
				}
			),
			Factor.Create(
				"Factor_ComplexFishShapeFactor",
				[nameof(IsFranken), nameof(Size)],
				GetType(),
				static args => (bool)args![0]!
					? (int)args[1]! switch { 2 => 0, 3 or 4 => 11, 5 or 6 or 7 => 12, _ => 13 }
					: (int)args[1]! switch { 2 => 0, 3 or 4 => 14, 5 or 6 => 16, 7 => 17, _ => 20 }
			),
			Factor.Create(
				"Factor_ComplexFishCannibalismFactor",
				[nameof(IsCannibalism)],
				GetType(),
				static args => (bool)args![0]! ? 3 : 0
			)
		];


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is ComplexFishStep comparer
		&& Digit == comparer.Digit
		&& BaseSetsMask == comparer.BaseSetsMask && CoverSetsMask == comparer.CoverSetsMask
		&& Exofins == comparer.Exofins && Endofins == comparer.Endofins;
}
