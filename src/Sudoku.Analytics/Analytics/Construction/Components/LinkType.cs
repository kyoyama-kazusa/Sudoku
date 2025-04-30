namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a link type.
/// </summary>
/// <remarks><include file="../../global-doc-comments.xml" path="/g/flags-attribute"/></remarks>
[Flags]
public enum LinkType
{
	/// <summary>
	/// Indicates the placeholder of the enumeration. The value can be represented in cases "None" or "Unknown".
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Indicates the link type is a single digit (X rule).
	/// </summary>
	[ChainingRule<XChainingRule>]
	SingleDigit = 1 << 0,

	/// <summary>
	/// Indicates the link type is a single cell (Y rule).
	/// </summary>
	[ChainingRule<YChainingRule>]
	SingleCell = 1 << 1,

	/// <summary>
	/// Indicates the link type is a locked candidates.
	/// </summary>
	[ChainingRule<LockedCandidatesChainingRule>]
	LockedCandidates = 1 << 2,

	/// <summary>
	/// Indicates the link type is an almost locked set.
	/// </summary>
	[ChainingRule<AlmostLockedSetsChainingRule>]
	AlmostLockedSets = 1 << 3,

	/// <summary>
	/// Indicates the link type is a kraken normal fish.
	/// </summary>
	[ChainingRule<KrakenNormalFishChainingRule>]
	KrakenNormalFish = 1 << 5,

	/// <summary>
	/// Indicates the link type is an XYZ-Wing.
	/// </summary>
	[ChainingRule<XyzWingChainingRule>]
	XyzWing = 1 << 6,

	/// <summary>
	/// Indicates the link type is an almost unique rectangle, with same digit strong link.
	/// </summary>
	[ChainingRule<UniqueRectangleSameDigitChainingRule>]
	UniqueRectangle_SameDigit = 1 << 7,

	/// <summary>
	/// Indicates the link type is an almost unique rectangle, with a strong link among different digits.
	/// </summary>
	[ChainingRule<UniqueRectangleDifferentDigitChainingRule>]
	UniqueRectangle_DifferentDigit = 1 << 8,

	/// <summary>
	/// Indicates the link type is an almost unique rectangle, with a strong link using single-side to be external.
	/// </summary>
	[ChainingRule<UniqueRectangleSingleSideExternalChainingRule>]
	UniqueRectangle_SingleSideExternal = 1 << 9,

	/// <summary>
	/// Indicates the link type is an almost unique rectangle, with a strong link using double-side to be external.
	/// </summary>
	[ChainingRule<UniqueRectangleDoubleSideExternalChainingRule>]
	UniqueRectangle_DoubleSideExternal = 1 << 10,

	/// <summary>
	/// Indicates the link type is an almost unique rectangle, with a weak link among different digits.
	/// </summary>
	[ChainingRule<UniqueRectangleDifferentDigitWeakChainingRule>]
	UniqueRectangle_DifferentDigitWeak = 1 << 11,

	/// <summary>
	/// Indicates the link type is an almost avoidable rectangle.
	/// </summary>
	[ChainingRule<AvoidableRectangleChainingRule>]
	AvoidableRectangle = 1 << 12
}
