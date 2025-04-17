namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents symmetry constraint.
/// </summary>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString, ToStringBehavior = ToStringBehavior.Specified)]
public sealed partial class SymmetryConstraint : Constraint
{
	/// <summary>
	/// Indicates an invalid value.
	/// </summary>
	public const SymmetricType InvalidSymmetricType = (SymmetricType)(-1);

	/// <summary>
	/// Indicates all possible symmetric types are included.
	/// </summary>
	public const SymmetricType AllSymmetricTypes = (SymmetricType)255;


	/// <summary>
	/// Indicates the supported symmetry types to be used.
	/// </summary>
	[HashCodeMember]
	public SymmetricType SymmetricTypes { get; set; }

	[StringMember]
	private string SymmetricTypesString => SymmetricTypes.ToString();


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is SymmetryConstraint comparer && SymmetricTypes == comparer.SymmetricTypes;

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("SymmetryConstraint", culture),
			SymmetricTypes switch
			{
				InvalidSymmetricType => SR.Get("SymmetryConstraint_NoSymmetrySelected"),
				_ => string.Join(
					SR.Get("_Token_Comma"),
					from type in SymmetricTypes.AllFlags select type.GetName(culture)
				)
			}
		);
	}

	/// <inheritdoc/>
	public override SymmetryConstraint Clone() => new() { IsNegated = IsNegated, SymmetricTypes = SymmetricTypes };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) => (SymmetricTypes & context.Grid.Symmetry) != 0;
}
