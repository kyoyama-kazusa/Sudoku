namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents a fish pattern.
/// </summary>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="baseSets"><inheritdoc cref="BaseSets" path="/summary"/></param>
/// <param name="coverSets"><inheritdoc cref="CoverSets" path="/summary"/></param>
/// <param name="exofins"><inheritdoc cref="Exofins" path="/summary"/></param>
/// <param name="endofins"><inheritdoc cref="Endofins" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class FishPattern(Digit digit, HouseMask baseSets, HouseMask coverSets, in CellMap exofins, in CellMap endofins) :
	Pattern,
	IFormattable
{
	/// <summary>
	/// Indicates whether the pattern is complex fish.
	/// </summary>
	public bool IsComplex => Endofins is not [];

	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.Fish;

	/// <summary>
	/// Indicates the shape kind of the current fish.
	/// </summary>
	public FishShapeKind ShapeKind
	{
		get
		{
			return (k(BaseSets), k(CoverSets)) switch
			{
				(FishShapeKind.Mutant, _) or (_, FishShapeKind.Mutant) => FishShapeKind.Mutant,
				(FishShapeKind.Franken, _) or (_, FishShapeKind.Franken) => FishShapeKind.Franken,
				_ => FishShapeKind.Basic
			};


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static FishShapeKind k(HouseMask mask)
			{
				var (blockMask, rowMask, columnMask) = mask.SplitMask;
				return rowMask != 0 && columnMask != 0
					? FishShapeKind.Mutant
					: (Mask)(rowMask | columnMask) != 0 && blockMask != 0 ? FishShapeKind.Franken : FishShapeKind.Basic;
			}
		}
	}

	/// <summary>
	/// Indicates all fins.
	/// </summary>
	public CellMap Fins => Exofins | Endofins;

	/// <summary>
	/// Indicates the digit to be used.
	/// </summary>
	[HashCodeMember]
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the base sets.
	/// </summary>
	[HashCodeMember]
	public HouseMask BaseSets { get; } = baseSets;

	/// <summary>
	/// Indicates the cover sets.
	/// </summary>
	[HashCodeMember]
	public HouseMask CoverSets { get; } = coverSets;

	/// <summary>
	/// Indicates exo-fins.
	/// </summary>
	[HashCodeMember]
	public CellMap Exofins { get; } = exofins;

	/// <summary>
	/// Indicates endo-fins.
	/// </summary>
	[HashCodeMember]
	public CellMap Endofins { get; } = endofins;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is FishPattern comparer
		&& (Digit, BaseSets, CoverSets) == (comparer.Digit, comparer.BaseSets, comparer.CoverSets)
		&& (Exofins, Endofins) == (comparer.Exofins, comparer.Endofins);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		switch (CoordinateConverter.GetInstance(formatProvider))
		{
			case RxCyConverter c:
			{
				// Special optimization.
				var bs = c.HouseConverter(BaseSets);
				var cs = c.HouseConverter(CoverSets);
				var exofins = this switch
				{
					{ Exofins: var f and not [] } => $" f{c.CellConverter(f)}",
					_ => string.Empty
				};
				var endofins = this switch
				{
					{ Endofins: var e and not [] } => $"ef{c.CellConverter(e)}",
					_ => string.Empty
				};
				return $@"{c.DigitConverter((Mask)(1 << Digit))} {bs}\{cs}{(string.IsNullOrEmpty(endofins) ? exofins : $"{exofins} ")}{endofins}";
			}
			case var c:
			{
				var exofinsAre = SR.Get("ExofinsAre", c.CurrentCulture);
				var comma = SR.Get("Comma", c.CurrentCulture);
				var digitString = c.DigitConverter((Mask)(1 << Digit));
				var bs = c.HouseConverter(BaseSets);
				var cs = c.HouseConverter(CoverSets);
				var exofins = this switch
				{
					{ Exofins: var f and not [] } => $"{comma}{string.Format(exofinsAre, c.CellConverter(f))}",
					_ => string.Empty
				};
				var endofins = this switch
				{
					{ Endofins: var e and not [] } => $"{comma}{string.Format(exofinsAre, c.CellConverter(e))}",
					_ => string.Empty
				};
				return $@"{c.DigitConverter((Mask)(1 << Digit))}{comma}{bs}\{cs}{exofins}{endofins}";
			}
		}
	}

	/// <summary>
	/// Try to get the fin kind using the specified grid as candidate references.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <returns>The fin kind.</returns>
	public FishFinKind GetFinKind(in Grid grid)
	{
		var fins = Fins;
		if (!fins)
		{
			return FishFinKind.Normal;
		}

		var candidatesMap = grid.CandidatesMap;
		foreach (var baseSet in BaseSets)
		{
			if ((HousesMap[baseSet] & ~fins & candidatesMap[Digit]).Count == 1)
			{
				return FishFinKind.Sashimi;
			}
		}
		return FishFinKind.Finned;
	}

	/// <inheritdoc/>
	public override FishPattern Clone() => new(Digit, BaseSets, CoverSets, Exofins, Endofins);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);
}
