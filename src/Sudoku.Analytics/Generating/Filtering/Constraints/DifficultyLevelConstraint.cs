namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents difficulty level constraint.
/// </summary>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class DifficultyLevelConstraint : Constraint, IComparisonOperatorConstraint
{
	/// <summary>
	/// Indicates the difficulty level.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public DifficultyLevel DifficultyLevel { get; set; }

	/// <inheritdoc/>
	[HashCodeMember]
	[StringMember]
	public ComparisonOperator Operator { get; set; }

	/// <summary>
	/// Indicates all possible <see cref="Analytics.DifficultyLevel"/> values. 
	/// </summary>
	public DifficultyLevel ValidDifficultyLevels
	{
		get
		{
			var allValues = DifficultyLevels.AllValid;
			var allValuesLower = DifficultyLevel switch
			{
				DifficultyLevel.Easy => DifficultyLevel.Unknown,
				DifficultyLevel.Moderate => DifficultyLevel.Easy,
				DifficultyLevel.Hard => DifficultyLevel.Easy | DifficultyLevel.Moderate,
				DifficultyLevel.Fiendish => DifficultyLevel.Easy | DifficultyLevel.Moderate | DifficultyLevel.Hard,
				DifficultyLevel.Nightmare => allValues & ~DifficultyLevel.Nightmare
			};
			var allValuesUpper = DifficultyLevel switch
			{
				DifficultyLevel.Easy => allValues & ~DifficultyLevel.Easy,
				DifficultyLevel.Moderate => DifficultyLevel.Hard | DifficultyLevel.Fiendish | DifficultyLevel.Nightmare,
				DifficultyLevel.Hard => DifficultyLevel.Fiendish | DifficultyLevel.Nightmare,
				DifficultyLevel.Fiendish => DifficultyLevel.Nightmare,
				DifficultyLevel.Nightmare => DifficultyLevel.Unknown
			};
			return Operator switch
			{
				ComparisonOperator.Equality => DifficultyLevel,
				ComparisonOperator.Inequality => allValues & ~DifficultyLevel,
				ComparisonOperator.GreaterThan => allValuesUpper,
				ComparisonOperator.GreaterThanOrEqual => allValuesUpper | DifficultyLevel,
				ComparisonOperator.LessThan => allValuesLower,
				ComparisonOperator.LessThanOrEqual => allValuesLower | DifficultyLevel
			};
		}
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is DifficultyLevelConstraint comparer && (DifficultyLevel, Operator) == (comparer.DifficultyLevel, comparer.Operator);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("DifficultyLevelConstraint", culture),
			[
				Operator.GetOperatorString(),
				DifficultyLevel.GetName(culture),
				string.Join(
					SR.Get("_Token_Comma", culture),
					from value in ValidDifficultyLevels.AllFlags select value.GetName(culture)
				)
			]
		);
	}

	/// <inheritdoc/>
	public override DifficultyLevelConstraint Clone()
		=> new() { IsNegated = IsNegated, DifficultyLevel = DifficultyLevel, Operator = Operator };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
		=> (ValidDifficultyLevels & context.AnalysisResult.DifficultyLevel) != 0;
}
