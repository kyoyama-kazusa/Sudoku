namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a data pattern that describes an AHS.
/// </summary>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
/// <param name="candidatesCanFormWeakLink"><inheritdoc cref="CandidatesCanFormWeakLink" path="/summary"/></param>
/// <remarks>
/// An <b>Almost Hidden Set</b> is a sudoku concept, which describes a case that
/// <c>n</c> digits are only appeared inside <c>(n + 1)</c> cells in a house.
/// </remarks>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.ComparisonOperators)]
public sealed partial class AlmostHiddenSetPattern(
	in CellMap cells,
	House house,
	Mask digitsMask,
	Mask subsetDigitsMask,
	in CandidateMap candidatesCanFormWeakLink
) :
	Pattern,
	IComparable<AlmostHiddenSetPattern>,
	IComparisonOperators<AlmostHiddenSetPattern, AlmostHiddenSetPattern, bool>
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.AlmostHiddenSet;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[HashCodeMember]
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the house used.
	/// </summary>
	[HashCodeMember]
	public House House { get; } = house;

	/// <summary>
	/// Indicates the mask of digits used.
	/// </summary>
	[HashCodeMember]
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the mask of subset digits used.
	/// </summary>
	[HashCodeMember]
	public Mask SubsetDigitsMask { get; } = subsetDigitsMask;

	/// <summary>
	/// Indicates all candidates that can be used as weak links.
	/// </summary>
	[HashCodeMember]
	public CandidateMap CandidatesCanFormWeakLink { get; } = candidatesCanFormWeakLink;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is AlmostHiddenSetPattern comparer
		&& Cells == comparer.Cells && House == comparer.House
		&& DigitsMask == comparer.DigitsMask && SubsetDigitsMask == comparer.SubsetDigitsMask;

	/// <inheritdoc/>
	public int CompareTo(AlmostHiddenSetPattern? other)
		=> other is null
			? 1
			: Cells.CompareTo(other.Cells) is var r1 and not 0
				? r1
				: DigitsMask.CompareTo(other.DigitsMask) is var r2 and not 0
					? r2
					: SubsetDigitsMask.CompareTo(other.SubsetDigitsMask) is var r3 and not 0 ? r3 : 0;

	/// <inheritdoc/>
	public override AlmostHiddenSetPattern Clone() => new(Cells, House, DigitsMask, SubsetDigitsMask, CandidatesCanFormWeakLink);


	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostHiddenSetPattern, AlmostHiddenSetPattern, bool>.operator ==(AlmostHiddenSetPattern? left, AlmostHiddenSetPattern? right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostHiddenSetPattern, AlmostHiddenSetPattern, bool>.operator !=(AlmostHiddenSetPattern? left, AlmostHiddenSetPattern? right)
		=> left != right;
}
