namespace Sudoku.Filtering;

/// <summary>
/// Represents a rule that checks whether a grid or its relied analysis information is passed the constraint.
/// </summary>
[JsonPolymorphic(
	TypeDiscriminatorPropertyName = "$typeid",
	UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(AllConstraint), nameof(AllConstraint))]
[JsonDerivedType(typeof(AndConstraint), nameof(AndConstraint))]
[JsonDerivedType(typeof(AnyConstraint), nameof(AnyConstraint))]
[JsonDerivedType(typeof(BottleneckTechniqueConstraint), nameof(BottleneckTechniqueConstraint))]
[JsonDerivedType(typeof(ConclusionConstraint), nameof(ConclusionConstraint))]
[JsonDerivedType(typeof(CountBetweenConstraint), nameof(CountBetweenConstraint))]
[JsonDerivedType(typeof(DiamondConstraint), nameof(DiamondConstraint))]
[JsonDerivedType(typeof(DifficultyLevelConstraint), nameof(DifficultyLevelConstraint))]
[JsonDerivedType(typeof(EliminationCountConstraint), nameof(EliminationCountConstraint))]
[JsonDerivedType(typeof(EmptyHousesCountConstraint), nameof(EmptyHousesCountConstraint))]
[JsonDerivedType(typeof(IttoryuConstraint), nameof(IttoryuConstraint))]
[JsonDerivedType(typeof(LastingConstraint), nameof(LastingConstraint))]
[JsonDerivedType(typeof(MinimalConstraint), nameof(MinimalConstraint))]
[JsonDerivedType(typeof(MissingDigitConstraint), nameof(MissingDigitConstraint))]
[JsonDerivedType(typeof(NotConstraint), nameof(NotConstraint))]
[JsonDerivedType(typeof(OrConstraint), nameof(OrConstraint))]
[JsonDerivedType(typeof(PearlConstraint), nameof(PearlConstraint))]
[JsonDerivedType(typeof(PrimarySingleConstraint), nameof(PrimarySingleConstraint))]
[JsonDerivedType(typeof(SymmetryConstraint), nameof(SymmetryConstraint))]
[JsonDerivedType(typeof(TechniqueConstraint), nameof(TechniqueConstraint))]
[JsonDerivedType(typeof(TechniqueCountConstraint), nameof(TechniqueCountConstraint))]
[JsonDerivedType(typeof(TechniqueGroupCountConstraint), nameof(TechniqueGroupCountConstraint))]
[JsonDerivedType(typeof(TechniquePrecedenceConstraint), nameof(TechniquePrecedenceConstraint))]
[JsonDerivedType(typeof(TechniqueSetConstraint), nameof(TechniqueSetConstraint))]
public closed class Constraint : IEquatable<Constraint>, IEqualityOperators<Constraint, Constraint, bool>
{
	/// <summary>
	/// Indicates whether the constraint should be negated.
	/// </summary>
	/// <remarks>
	/// This property can be used on some necessary cases (like technique-appearance-related constraints),
	/// but not all constraints will consume this property.
	/// </remarks>
	public bool IsNegated { get; set; }

	/// <summary>
	/// Indicates the description to the constraint.
	/// </summary>
	public string Description => SR.Get($"ConstraintDescription_{GetType().Name}");


	/// <summary>
	/// Determine whether the specified grid is passed the constraint.
	/// </summary>
	/// <param name="context">Indicates the context used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Check(ConstraintCheckingContext context) => CheckCore(context) is var result && (IsNegated ? !result : result);

	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Constraint);

	/// <inheritdoc/>
	public abstract bool Equals([NotNullWhen(true)] Constraint? other);

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc/>
	public sealed override string ToString() => ToString(CultureInfo.CurrentUICulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public abstract string ToString(CultureInfo culture);

	/// <summary>
	/// Creates a new instance that contains same data with the current instance.
	/// </summary>
	/// <returns>A new instance that contains same data with the current instance.</returns>
	public abstract Constraint Clone();

	/// <summary>
	/// Returns a <see cref="ConstraintOptionsAttribute"/> instance that represents the metadata of the constraint configured.
	/// </summary>
	/// <returns>A <see cref="ConstraintOptionsAttribute"/> instance or <see langword="null"/> if not configured.</returns>
	public ConstraintOptionsAttribute? GetMetadata() => GetType().GetCustomAttribute<ConstraintOptionsAttribute>();

	/// <inheritdoc cref="Check(ConstraintCheckingContext)"/>
	protected abstract bool CheckCore(ConstraintCheckingContext context);


	/// <inheritdoc/>
	public static bool operator ==(Constraint? left, Constraint? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(Constraint? left, Constraint? right) => !(left == right);
}
