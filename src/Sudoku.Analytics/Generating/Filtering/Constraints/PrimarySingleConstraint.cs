namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a primary single constraint.
/// </summary>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString, ToStringBehavior = ToStringBehavior.RecordLike)]
public sealed partial class PrimarySingleConstraint : Constraint
{
	/// <summary>
	/// Indicates whether the constraint allows hidden singles on rows or columns.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public bool AllowsHiddenSingleInLines { get; set; }

	/// <summary>
	/// Indicates which technique a user likes to finish a grid.
	/// </summary>
	[HashCodeMember]
	public SingleTechniqueFlag Primary { get; set; }

	[StringMember(nameof(Primary))]
	private string SinglePreferString => Primary.GetName(null);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is PrimarySingleConstraint comparer && Primary == comparer.Primary;

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("PrimarySingleConstraint", culture),
			[
				Primary.GetName(culture),
				AllowsHiddenSingleInLines ? string.Empty : SR.Get("NoString", culture)
			]
		);
	}

	/// <inheritdoc/>
	public override PrimarySingleConstraint Clone()
		=> new() { IsNegated = IsNegated, AllowsHiddenSingleInLines = AllowsHiddenSingleInLines, Primary = Primary };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		ref readonly var grid = ref context.Grid;
		return Primary switch
		{
			SingleTechniqueFlag.FullHouse => grid.CanPrimaryFullHouse,
			SingleTechniqueFlag.HiddenSingle => grid.CanPrimaryHiddenSingle(AllowsHiddenSingleInLines),
			SingleTechniqueFlag.NakedSingle => grid.CanPrimaryNakedSingle
		};
	}
}
