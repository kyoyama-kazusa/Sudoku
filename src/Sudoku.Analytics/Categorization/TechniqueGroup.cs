namespace Sudoku.Categorization;

/// <summary>
/// Represents a technique group.
/// </summary>
public enum TechniqueGroup
{
	/// <summary>
	/// Indicates the technique doesn't belong to any group.
	/// </summary>
	None,

	/// <summary>
	/// Indicates the singles technique.
	/// </summary>
	Single,

	/// <summary>
	/// Indicates the complex singles technique.
	/// </summary>
	ComplexSingle,

	/// <summary>
	/// Indicates the locked candidates (LC) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "LC")]
	LockedCandidates,

	/// <summary>
	/// Indicates the subset technique.
	/// </summary>
	Subset,

	/// <summary>
	/// Indicates the normal fish technique.
	/// </summary>
	[TechniqueMetadata(SupportsSiamese = true)]
	NormalFish,

	/// <summary>
	/// Indicates the complex fish technique.
	/// </summary>
	[TechniqueMetadata(SupportsSiamese = true)]
	ComplexFish,

	/// <summary>
	/// Indicates the regular wing technique.
	/// </summary>
	RegularWing,

	/// <summary>
	/// Indicates the irregular wing technique.
	/// </summary>
	IrregularWing,

	/// <summary>
	/// Indicates the baba grouping technique.
	/// </summary>
	BabaGrouping,

	/// <summary>
	/// Indicates the wing extension (extended subset principle) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "ESP")]
	ExtendedSubsetPrinciple,

	/// <summary>
	/// Indicates the fat ring technique.
	/// </summary>
	FatRing,

	/// <summary>
	/// Indicates the empty rectangle technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "ER")]
	EmptyRectangle,

	/// <summary>
	/// Indicates the single digit pattern (SDP) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "SDP")]
	SingleDigitPattern,

	/// <summary>
	/// Indicates the empty rectangle intersection pair (ERIP) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "ERIP")]
	EmptyRectangleIntersectionPair,

	/// <summary>
	/// Indicates the almost locked candidates (ALC) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "ALC")]
	AlmostLockedCandidates,

	/// <summary>
	/// Indicates the firework technique.
	/// </summary>
	Firework,

	/// <summary>
	/// Indicates the alternating inference chain (AIC) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "AIC")]
	AlternatingInferenceChain,

	/// <summary>
	/// Indicates the pattern construction technique.
	/// </summary>
	PatternConstruction,

	/// <summary>
	/// Indicates the forcing chains technique.
	/// </summary>
	ForcingChains,

	/// <summary>
	/// Indicates the blossom loop technique.
	/// </summary>
	BlossomLoop,

	/// <summary>
	/// Indicates whip and braid technique.
	/// </summary>
	WhipAndBraid,

	/// <summary>
	/// Indicates the unique rectangle (UR) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "UR")]
	UniqueRectangle,

	/// <summary>
	/// Indicates the avoidable rectangle (AR) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "AR")]
	AvoidableRectangle,

	/// <summary>
	/// Indicates the unique loop (UL) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "UL")]
	UniqueLoop,

	/// <summary>
	/// Indicates the extended rectangle (XR) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "XR")]
	ExtendedRectangle,

	/// <summary>
	/// Indicates the bi-value universal grave (BUG) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "BUG")]
	BivalueUniversalGrave,

	/// <summary>
	/// Indicates the Borescoper's deadly pattern technique.
	/// </summary>
	BorescoperDeadlyPattern,

	/// <summary>
	/// Indicates the Qiu's deadly pattern technique.
	/// </summary>
	QiuDeadlyPattern,

	/// <summary>
	/// Indicates the RW's deadly pattern technique.
	/// </summary>
	RwDeadlyPattern,

	/// <summary>
	/// Indicates the unique matrix technique.
	/// </summary>
	UniqueMatrix,

	/// <summary>
	/// Indicates the conditional deadly pattern.
	/// </summary>
	ConditionalDeadlyPattern,

	/// <summary>
	/// Indicates the reverse bi-value universal grave (Reverse BUG) technique.
	/// </summary>
	ReverseBivalueUniversalGrave,

	/// <summary>
	/// Indicates the uniqueness clue cover.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "UCC")]
	UniquenessClueCover,

	/// <summary>
	/// Indicates the bi-value oddagon technique.
	/// </summary>
	BivalueOddagon,

	/// <summary>
	/// Indicates the chromatic pattern technique.
	/// </summary>
	ChromaticPattern,

	/// <summary>
	/// Indicates the sue de coq (SdC) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "SdC")]
	SueDeCoq,

	/// <summary>
	/// Indicates the broken wing technique.
	/// </summary>
	BrokenWing,

	/// <summary>
	/// Indicates the XYZ-Ring technique.
	/// </summary>
	[TechniqueMetadata(SupportsSiamese = true)]
	XyzRing,

	/// <summary>
	/// Indicates the ALS chaining-like (ALS-XZ, ALS-XY-Wing, ALS-W-Wing) technique.
	/// </summary>
	AlmostLockedSetsChainingLike,

	/// <summary>
	/// Indicates the aligned exclusion technique.
	/// </summary>
	AlignedExclusion,

	/// <summary>
	/// Indicates the death blossom technique.
	/// </summary>
	DeathBlossom,

	/// <summary>
	/// Indicates the SK-Loop technique.
	/// </summary>
	DominoLoop,

	/// <summary>
	/// Indicates the multi-sector locked sets (MSLS) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "MSLS")]
	MultisectorLockedSets,

	/// <summary>
	/// Indicates the exocet technique.
	/// </summary>
	Exocet,

	/// <summary>
	/// Indicates the symmetry technique.
	/// </summary>
	Symmetry,

	/// <summary>
	/// Indicates the technique checked and searched relies on the rank theory.
	/// </summary>
	RankTheory,

	/// <summary>
	/// Indicates the bowman bingo technique.
	/// </summary>
	BowmanBingo,

	/// <summary>
	/// Indicates the pattern overlay method (POM) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "POM")]
	PatternOverlay,

	/// <summary>
	/// Indicates the templating technique.
	/// </summary>
	Templating,

	/// <summary>
	/// Indicates the brute force (BF) technique.
	/// </summary>
	[TechniqueMetadata(Abbreviation = "BF")]
	BruteForce
}
