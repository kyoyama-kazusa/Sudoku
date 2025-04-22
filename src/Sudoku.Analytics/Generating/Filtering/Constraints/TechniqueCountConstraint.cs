namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a rule that checks whether the specified analyzer result after analyzed by a grid
/// contains the specified techniques.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class TechniqueCountConstraint : Constraint, IComparisonOperatorConstraint, ILimitCountConstraint<int>
{
	/// <inheritdoc/>
	[HashCodeMember]
	[StringMember]
	public int LimitCount { get; set; }

	/// <inheritdoc/>
	[HashCodeMember]
	[StringMember]
	public ComparisonOperator Operator { get; set; }

	/// <summary>
	/// Indicates the technique used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Technique Technique { get; set; }


	/// <inheritdoc/>
	public static int Minimum => 0;

	/// <inheritdoc/>
	public static int Maximum => 20;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is TechniqueCountConstraint comparer
		&& (LimitCount, Operator, Technique) == (comparer.LimitCount, comparer.Operator, comparer.Technique);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("TechniqueCountConstraint", culture),
			[Technique.GetName(culture), Operator.GetOperatorString, LimitCount]
		);
	}

	/// <inheritdoc/>
	public override TechniqueCountConstraint Clone()
		=> new() { IsNegated = IsNegated, LimitCount = LimitCount, Operator = Operator, Technique = Technique };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var times = 0;
		foreach (var step in context.AnalysisResult)
		{
			if (Technique == step.Code)
			{
				times++;
			}
		}
		return Operator.GetOperator<int>()(times, LimitCount);
	}
}
