namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that checks for pearl or diamond property.
/// </summary>
/// <param name="checkPearl"></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public abstract partial class PearlOrDiamondConstraint(bool checkPearl) : Constraint
{
	/// <summary>
	/// Indicates whether the puzzle should be pearl or diamond.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public bool ShouldBePearlOrDiamond { get; set; }

	/// <summary>
	/// Indicates whether the constraint checks for pearl.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public bool CheckPearl { get; } = checkPearl;


	/// <inheritdocs/>
	public sealed override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is PearlOrDiamondConstraint comparer
		&& (CheckPearl, ShouldBePearlOrDiamond) == (comparer.CheckPearl, comparer.ShouldBePearlOrDiamond);

	/// <inheritdoc/>
	public sealed override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("PearlOrDiamondConstraint", culture),
			[
				SR.Get(CheckPearl ? "PearlString" : "DiamondString", culture).ToUpper(),
				ShouldBePearlOrDiamond ? string.Empty : SR.Get("NoString", culture),
				SR.Get(CheckPearl ? "PearlString" : "DiamondString", culture)
			]
		);
	}

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var isPearl = context.AnalysisResult.IsPearl;
		var isDiamond = context.AnalysisResult.IsDiamond;
		return !(ShouldBePearlOrDiamond ^ ((CheckPearl ? isPearl : isDiamond) ?? false));
	}
}
