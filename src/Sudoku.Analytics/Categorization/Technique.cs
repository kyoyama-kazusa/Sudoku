#undef COMPATIBLE_ORIGINAL_TECHNIQUE_RULES

namespace Sudoku.Categorization;

/// <summary>
/// Represents a technique as unique identifier on describing different techniques and their own patterns used.
/// </summary>
/// <remarks><b><i>
/// Please note that some fields declared in this type may not be used by neither direct reference nor reflection,
/// but they are reserved ones. In the future I'll implement the searching logic on those fields. Maybe or maybe not :D
/// </i></b></remarks>
public enum Technique
{
	/// <summary>
	/// The placeholder of this enumeration type.
	/// </summary>
	None,

	//
	// Singles
	//
	#region Singles
	/// <summary>
	/// Indicates full house. This technique is the most elementary technique to be used in the candidate view mode.
	/// </summary>
	[Hodoku(Rating = 4, DifficultyLevel = HodokuDifficultyLevel.Easy, Prefix = "0000")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.Single, RatingOriginal = [1.0], Aliases = ["Single"])]
	[TechniqueMetadata(
		Rating = 10,
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(FullHouseStep),
		StepSearcherType = typeof(SingleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Full_House.html"])]
	FullHouse,

	/// <summary>
	/// Indicates last digit.
	/// </summary>
	[Hodoku(Prefix = "0001")]
	[TechniqueMetadata(
		Rating = 11,
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(LastDigitStep),
		StepSearcherType = typeof(SingleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Last_Digit.html"])]
	LastDigit,

	/// <summary>
	/// Indicates hidden single (in block).
	/// </summary>
	[Hodoku(Rating = 14, DifficultyLevel = HodokuDifficultyLevel.Easy, Prefix = "0002")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenSingle, RatingOriginal = [1.2])]
	[TechniqueMetadata(
		Rating = 19,
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.FullMarking,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"])]
	HiddenSingleBlock,

	/// <summary>
	/// Indicates hidden single (in row).
	/// </summary>
	[Hodoku(Rating = 14, DifficultyLevel = HodokuDifficultyLevel.Easy, Prefix = "0002")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenSingle, RatingOriginal = [1.5])]
	[TechniqueMetadata(
		Rating = 23,
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.FullMarking,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"])]
	HiddenSingleRow,

	/// <summary>
	/// Indicates hidden single (in column).
	/// </summary>
	[Hodoku(Rating = 14, DifficultyLevel = HodokuDifficultyLevel.Easy, Prefix = "0002")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenSingle, RatingOriginal = [1.5])]
	[TechniqueMetadata(
		Rating = 23,
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.FullMarking,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"])]
	HiddenSingleColumn,

	/// <summary>
	/// Indicates crosshatching in block, equivalent to hidden single in block, but used in direct views.
	/// </summary>
	[TechniqueMetadata(
		Rating = 12,
		Abbreviation = "CHB",
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"],
		Features = TechniqueFeatures.DirectTechniques)]
	CrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in row, equivalent to hidden single in row, but used in direct views.
	/// </summary>
	[TechniqueMetadata(
		Rating = 15,
		Abbreviation = "CHR",
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"],
		Features = TechniqueFeatures.DirectTechniques)]
	CrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in column, equivalent to hidden single in column, but used in direct views.
	/// </summary>
	[TechniqueMetadata(
		Rating = 15,
		Abbreviation = "CHC",
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(HiddenSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct,
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Single.html"],
		Features = TechniqueFeatures.DirectTechniques)]
	CrosshatchingColumn,

	/// <summary>
	/// Indicates naked single.
	/// </summary>
	[Hodoku(Rating = 4, DifficultyLevel = HodokuDifficultyLevel.Easy, Prefix = "0003")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.NakedSingle, RatingOriginal = [2.3])]
	[TechniqueMetadata(
		Rating = 23,
		DirectRating = 10,
		Abbreviation = "NS",
		DifficultyLevel = DifficultyLevel.Easy,
		ContainingGroup = TechniqueGroup.Single,
		StepType = typeof(NakedSingleStep),
		StepSearcherType = typeof(SingleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Single.html"])]
	NakedSingle,
	#endregion

	//
	// Complex Singles
	//
	#region Complex Singles
	/// <summary>
	/// Indicates full house, with pointing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	PointingFullHouse,

	/// <summary>
	/// Indicates full house, with claiming.
	/// </summary>
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ClaimingFullHouse,

	/// <summary>
	/// Indicates full house, with naked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairFullHouse,

	/// <summary>
	/// Indicates full house, with naked pair (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairPlusFullHouse,

	/// <summary>
	/// Indicates full house, with hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenPairFullHouse,

	/// <summary>
	/// Indicates full house, with locked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedPairFullHouse,

	/// <summary>
	/// Indicates full house, with locked hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenPairFullHouse,

	/// <summary>
	/// Indicates full house, with naked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTripleFullHouse,

	/// <summary>
	/// Indicates full house, with naked triple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTriplePlusFullHouse,

	/// <summary>
	/// Indicates full house, with hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenTripleFullHouse,

	/// <summary>
	/// Indicates full house, with locked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedTripleFullHouse,

	/// <summary>
	/// Indicates full house, with locked hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenTripleFullHouse,

	/// <summary>
	/// Indicates full house, with naked quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadrupleFullHouse,

	/// <summary>
	/// Indicates full house, with naked quadruple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadruplePlusFullHouse,

	/// <summary>
	/// Indicates full house, with hidden quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenQuadrupleFullHouse,

	/// <summary>
	/// Indicates crosshatching in block, with pointing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	PointingCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with claiming.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ClaimingCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked pair (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairPlusCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenPairCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with locked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedPairCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with locked hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenPairCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTripleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked triple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTriplePlusCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenTripleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with locked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedTripleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with locked hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenTripleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadrupleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in block, with naked quadruple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadruplePlusCrosshatchingBlock,

	/// <summary>
	/// Indicates full house, with hidden quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenQuadrupleCrosshatchingBlock,

	/// <summary>
	/// Indicates crosshatching in row, with pointing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	PointingCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with claiming.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ClaimingCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked pair (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairPlusCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenPairCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with locked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedPairCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with locked hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenPairCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTripleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked triple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTriplePlusCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenTripleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with locked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedTripleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with locked hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenTripleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadrupleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in row, with naked quadruple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadruplePlusCrosshatchingRow,

	/// <summary>
	/// Indicates full house, with hidden quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenQuadrupleCrosshatchingRow,

	/// <summary>
	/// Indicates crosshatching in column, with pointing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	PointingCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with claiming.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ClaimingCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked pair (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairPlusCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenPairCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with locked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedPairCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with locked hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenPairCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTripleCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked triple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTriplePlusCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenTripleCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with locked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedTripleCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with locked hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenTripleCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadrupleCrosshatchingColumn,

	/// <summary>
	/// Indicates crosshatching in column, with naked quadruple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadruplePlusCrosshatchingColumn,

	/// <summary>
	/// Indicates full house, with hidden quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenQuadrupleCrosshatchingColumn,

	/// <summary>
	/// Indicates naked single, with pointing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	PointingNakedSingle,

	/// <summary>
	/// Indicates naked single, with claiming.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectIntersectionStep),
		StepSearcherType = typeof(DirectIntersectionStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ClaimingNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked pair (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedPairPlusNakedSingle,

	/// <summary>
	/// Indicates naked single, with hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenPairNakedSingle,

	/// <summary>
	/// Indicates naked single, with locked pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedPairNakedSingle,

	/// <summary>
	/// Indicates naked single, with locked hidden pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenPairNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTripleNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked triple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedTriplePlusNakedSingle,

	/// <summary>
	/// Indicates naked single, with hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenTripleNakedSingle,

	/// <summary>
	/// Indicates naked single, with locked triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedTripleNakedSingle,

	/// <summary>
	/// Indicates naked single, with locked hidden triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	LockedHiddenTripleNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadrupleNakedSingle,

	/// <summary>
	/// Indicates naked single, with naked quadruple (+).
	/// </summary>
	[TechniqueMetadata(
		Rating = 33,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	NakedQuadruplePlusNakedSingle,

	/// <summary>
	/// Indicates full house, with hidden quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 37,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(DirectSubsetStep),
		StepSearcherType = typeof(DirectSubsetStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	HiddenQuadrupleNakedSingle,

	/// <summary>
	/// Indicates complex full house.
	/// </summary>
	[TechniqueMetadata(
		Rating = 15,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(NormalComplexSingleStep),
		StepSearcherType = typeof(ComplexSingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ComplexFullHouse,

	/// <summary>
	/// Indicates complex crosshatching in block.
	/// </summary>
	[TechniqueMetadata(
		Rating = 17,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(NormalComplexSingleStep),
		StepSearcherType = typeof(ComplexSingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ComplexCrosshatchingBlock,

	/// <summary>
	/// Indicates complex crosshatching in row.
	/// </summary>
	[TechniqueMetadata(
		Rating = 20,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(NormalComplexSingleStep),
		StepSearcherType = typeof(ComplexSingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ComplexCrosshatchingRow,

	/// <summary>
	/// Indicates complex crosshatching in column.
	/// </summary>
	[TechniqueMetadata(
		Rating = 20,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(NormalComplexSingleStep),
		StepSearcherType = typeof(ComplexSingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ComplexCrosshatchingColumn,

	/// <summary>
	/// Indicates complex naked single.
	/// </summary>
	[TechniqueMetadata(
		Rating = 28,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexSingle,
		StepType = typeof(NormalComplexSingleStep),
		StepSearcherType = typeof(ComplexSingleStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct | PencilmarkVisibility.PartialMarking,
		Features = TechniqueFeatures.DirectTechniques)]
	ComplexNakedSingle,
	#endregion

	//
	// Locked Candidates
	//
	#region Locked Candidates
	/// <summary>
	/// Indicates pointing.
	/// </summary>
	[Hodoku(
		Rating = 50,
		DifficultyLevel = HodokuDifficultyLevel.Medium,
		Prefix = "0100",
		Aliases = ["Locked Candidates Type 1"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.Pointing, RatingOriginal = [2.6])]
	[Npg(Rating = 20)]
	[TechniqueMetadata(
		Rating = 26,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.LockedCandidates,
		Abbreviation = "LC1",
		StepType = typeof(LockedCandidatesStep),
		StepSearcherType = typeof(LockedCandidatesStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Locked_Candidates.html"])]
	Pointing,

	/// <summary>
	/// Indicates claiming.
	/// </summary>
	[Hodoku(
		Rating = 50,
		DifficultyLevel = HodokuDifficultyLevel.Medium,
		Prefix = "0101",
		Aliases = ["Locked Candidates Type 2"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.Claiming, RatingOriginal = [2.8])]
	[Npg(Rating = 20)]
	[TechniqueMetadata(
		Rating = 28,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.LockedCandidates,
		Abbreviation = "LC2",
		StepType = typeof(LockedCandidatesStep),
		StepSearcherType = typeof(LockedCandidatesStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Locked_Candidates.html"])]
	Claiming,

	/// <summary>
	/// Indicates law of leftover.
	/// </summary>
	[TechniqueMetadata(
		Rating = 20,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.LockedCandidates,
		StepType = typeof(LawOfLeftoverStep),
		StepSearcherType = typeof(LawOfLeftoverStepSearcher),
		PencilmarkVisibility = PencilmarkVisibility.Direct,
		Abbreviation = "LoL",
		Features = TechniqueFeatures.DirectTechniques,
		Links = ["http://sudopedia.enjoysudoku.com/Law_of_Leftovers.html"])]
	LawOfLeftover,
	#endregion

	//
	// Subsets
	//
	#region Subsets
	/// <summary>
	/// Indicates naked pair.
	/// </summary>
	[Hodoku(Rating = 60, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0200")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.NakedPair, RatingOriginal = [3.0])]
	[Npg(Rating = 30)]
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS2",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Pair.html"])]
	NakedPair,

	/// <summary>
	/// Indicates naked pair plus (naked pair (+)).
	/// </summary>
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS2+",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Pair.html"])]
	NakedPairPlus,

	/// <summary>
	/// Indicates locked pair.
	/// </summary>
	[Hodoku(Rating = 40, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0110-1")]
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DirectHiddenPair, RatingOriginal = [2.0], Aliases = ["Direct Hidden Pair"])]
#endif
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "LS2",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(LockedSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Locked_Pair.html"])]
	LockedPair,

	/// <summary>
	/// Indicates hidden pair.
	/// </summary>
	[Hodoku(Rating = 70, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0210")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenPair, RatingOriginal = [3.4])]
	[Npg(Rating = 30)]
	[TechniqueMetadata(
		Rating = 34,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "HS2",
		StepType = typeof(HiddenSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Pair.html"])]
	HiddenPair,

	/// <summary>
	/// Indicates locked hidden pair.
	/// </summary>
	[Hodoku(Prefix = "0110-2")]
	[TechniqueMetadata(
		Rating = 34,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "LHS2",
		StepType = typeof(HiddenSubsetStep),
		StepSearcherType = typeof(LockedSubsetStepSearcher))]
	LockedHiddenPair,

	/// <summary>
	/// Indicates naked triple.
	/// </summary>
	[Hodoku(Rating = 80, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0201")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.NakedTriplet, RatingOriginal = [3.6], Aliases = ["Naked Triplet"])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS3",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Triple.html"])]
	NakedTriple,

	/// <summary>
	/// Indicates naked triple plus (naked triple (+)).
	/// </summary>
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS3+",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Triple.html"])]
	NakedTriplePlus,

	/// <summary>
	/// Indicates locked triple.
	/// </summary>
	[Hodoku(Rating = 60, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0111-1")]
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DirectHiddenTriplet, RatingOriginal = [2.5], Aliases = ["Direct Hidden Triplet"])]
#endif
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "LS3",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(LockedSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Locked_Triple.html"])]
	LockedTriple,

	/// <summary>
	/// Indicates hidden triple.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Medium, Prefix = "0211")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenTriplet, RatingOriginal = [4.0], Aliases = ["Hidden Triplet"])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 34,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "HS3",
		StepType = typeof(HiddenSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Triple.html"])]
	HiddenTriple,

	/// <summary>
	/// Indicates locked hidden triple.
	/// </summary>
	[Hodoku(Prefix = "0111-2")]
	[TechniqueMetadata(
		Rating = 34,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NHS3",
		StepType = typeof(HiddenSubsetStep),
		StepSearcherType = typeof(LockedSubsetStepSearcher))]
	LockedHiddenTriple,

	/// <summary>
	/// Indicates naked quadruple.
	/// </summary>
	[Hodoku(Rating = 120, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0202")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.NakedQuad, RatingOriginal = [5.0], Aliases = ["Naked Quad"])]
	[Npg(Rating = 150)]
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS4",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Quad.html"])]
	NakedQuadruple,

	/// <summary>
	/// Indicates naked quadruple plus (naked quadruple (+)).
	/// </summary>
	[TechniqueMetadata(
		Rating = 30,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "NS4+",
		StepType = typeof(NakedSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Naked_Quad.html"])]
	NakedQuadruplePlus,

	/// <summary>
	/// Indicates hidden quadruple.
	/// </summary>
	[Hodoku(Rating = 150, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0212")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.HiddenQuad, RatingOriginal = [5.4], Aliases = ["Hidden Quad"])]
	[Npg(Rating = 150)]
	[TechniqueMetadata(
		Rating = 34,
		DifficultyLevel = DifficultyLevel.Moderate,
		ContainingGroup = TechniqueGroup.Subset,
		Abbreviation = "HS4",
		StepType = typeof(HiddenSubsetStep),
		StepSearcherType = typeof(NormalSubsetStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Hidden_Quad.html"])]
	HiddenQuadruple,
	#endregion

	//
	// Fishes
	//
	#region Fishes
	/// <summary>
	/// Indicates X-Wing.
	/// </summary>
	[Hodoku(Rating = 140, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0300")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.XWing, RatingOriginal = [3.2])]
	[Npg(Rating = 80)]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/X-Wing.html", "http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html"])]
	XWing,

	/// <summary>
	/// Indicates finned X-Wing.
	/// </summary>
	[Hodoku(Rating = 130, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0310")]
	[SudokuExplainer(RatingAdvanced = [3.4])]
	[TechniqueMetadata(
		Rating = 32, DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Finned_Fish.html",
			"http://sudopedia.enjoysudoku.com/Finned_X-Wing.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793"
		])]
	FinnedXWing,

	/// <summary>
	/// Indicates sashimi X-Wing.
	/// </summary>
	[Hodoku(Rating = 150, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0320")]
	[SudokuExplainer(RatingAdvanced = [3.5])]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Sashimi_Fish.html",
			"http://sudopedia.enjoysudoku.com/Sashimi_X-Wing.html"
		])]
	SashimiXWing,

	/// <summary>
	/// Indicates Siamese finned X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793"])]
	SiameseFinnedXWing,

	/// <summary>
	/// Indicates Siamese sashimi X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html"])]
	SiameseSashimiXWing,

	/// <summary>
	/// Indicates swordfish.
	/// </summary>
	[Hodoku(Rating = 150, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0301")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.Swordfish, RatingOriginal = [3.8])]
	[Npg(Rating = 200)]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Swordfish.html"])]
	Swordfish,

	/// <summary>
	/// Indicates finned swordfish.
	/// </summary>
	[Hodoku(Rating = 200, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0311")]
	[SudokuExplainer(RatingAdvanced = [4.0])]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Finned_Fish.html",
			"http://sudopedia.enjoysudoku.com/Finned_Swordfish.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793"
		])]
	FinnedSwordfish,

	/// <summary>
	/// Indicates sashimi swordfish.
	/// </summary>
	[Hodoku(
		Rating = 240,
		DifficultyLevel = HodokuDifficultyLevel.Unfair,
		Prefix = "0321")]
	[SudokuExplainer(RatingAdvanced = [4.1])]
	[TechniqueMetadata(
		Rating = 32, DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Sashimi_Fish.html",
			"http://sudopedia.enjoysudoku.com/Sashimi_Swordfish.html"
		])]
	SashimiSwordfish,

	/// <summary>
	/// Indicates Siamese finned swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793"])]
	SiameseFinnedSwordfish,

	/// <summary>
	/// Indicates Siamese sashimi swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html"])]
	SiameseSashimiSwordfish,

	/// <summary>
	/// Indicates jellyfish.
	/// </summary>
	[Hodoku(Rating = 160, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0302")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.Jellyfish, RatingOriginal = [5.2])]
	[Npg(Rating = 300)]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Jellyfish.html"
		])]
	Jellyfish,

	/// <summary>
	/// Indicates finned jellyfish.
	/// </summary>
	[Hodoku(Rating = 250, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0312")]
	[SudokuExplainer(RatingAdvanced = [5.4])]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Finned_Fish.html",
			"http://sudopedia.enjoysudoku.com/Finned_Jellyfish.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793"
		])]
	FinnedJellyfish,

	/// <summary>
	/// Indicates sashimi jellyfish.
	/// </summary>
	[Hodoku(Rating = 260, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0322")]
	[SudokuExplainer(RatingAdvanced = [5.6])]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Sashimi_Fish.html",
			"http://sudopedia.enjoysudoku.com/Sashimi_Jellyfish.html"
		])]
	SashimiJellyfish,

	/// <summary>
	/// Indicates Siamese finned jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793"
		])]
	SiameseFinnedJellyfish,

	/// <summary>
	/// Indicates Siamese sashimi jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.NormalFish,
		StepType = typeof(NormalFishStep),
		StepSearcherType = typeof(NormalFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html"])]
	SiameseSashimiJellyfish,
	#endregion

	//
	// Complex Fishes
	//
	#region Complex Fishes
	/// <summary>
	/// Indicates franken X-Wing.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0330")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	FrankenXWing,

	/// <summary>
	/// Indicates finned franken X-Wing.
	/// </summary>
	[Hodoku(Rating = 390, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0340")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	FinnedFrankenXWing,

	/// <summary>
	/// Indicates sashimi franken X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SashimiFrankenXWing,

	/// <summary>
	/// Indicates Siamese finned franken X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	SiameseFinnedFrankenXWing,

	/// <summary>
	/// Indicates Siamese sashimi franken X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html"])]
	SiameseSashimiFrankenXWing,

	/// <summary>
	/// Indicates mutant X-Wing.
	/// </summary>
	[Hodoku(Rating = 450, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0350")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	MutantXWing,

	/// <summary>
	/// Indicates finned mutant X-Wing.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0360")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	FinnedMutantXWing,

	/// <summary>
	/// Indicates sashimi mutant X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SashimiMutantXWing,

	/// <summary>
	/// Indicates Siamese finned mutant X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	SiameseFinnedMutantXWing,

	/// <summary>
	/// Indicates Siamese sashimi mutant X-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SiameseSashimiMutantXWing,

	/// <summary>
	/// Indicates swordfish.
	/// </summary>
	[Hodoku(
		Rating = 350,
		DifficultyLevel = HodokuDifficultyLevel.Unfair,
		Prefix = "0331")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Swordfish.html"
		])]
	FrankenSwordfish,

	/// <summary>
	/// Indicates finned franken swordfish.
	/// </summary>
	[Hodoku(Rating = 410, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0341")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Swordfish.html"
		])]
	FinnedFrankenSwordfish,

	/// <summary>
	/// Indicates sashimi franken swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Swordfish.html"
		])]
	SashimiFrankenSwordfish,

	/// <summary>
	/// Indicates Siamese finned franken swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Swordfish.html"
		])]
	SiameseFinnedFrankenSwordfish,

	/// <summary>
	/// Indicates Siamese sashimi franken swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Swordfish.html"
		])]
	SiameseSashimiFrankenSwordfish,

	/// <summary>
	/// Indicates mutant swordfish.
	/// </summary>
	[Hodoku(Rating = 450, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0351")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Swordfish.html"
		])]
	MutantSwordfish,

	/// <summary>
	/// Indicates finned mutant swordfish.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0361")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Swordfish.html"
		])]
	FinnedMutantSwordfish,

	/// <summary>
	/// Indicates sashimi mutant swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Swordfish.html"
		])]
	SashimiMutantSwordfish,

	/// <summary>
	/// Indicates Siamese finned mutant swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Swordfish.html"
		])]
	SiameseFinnedMutantSwordfish,

	/// <summary>
	/// Indicates Siamese sashimi mutant swordfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Swordfish.html"
		])]
	SiameseSashimiMutantSwordfish,

	/// <summary>
	/// Indicates franken jellyfish.
	/// </summary>
	[Hodoku(Rating = 370, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0332")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Jellyfish.html"
		])]
	FrankenJellyfish,

	/// <summary>
	/// Indicates finned franken jellyfish.
	/// </summary>
	[Hodoku(Rating = 430, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0342")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Jellyfish.html"
		])]
	FinnedFrankenJellyfish,

	/// <summary>
	/// Indicates sashimi franken jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Jellyfish.html"
		])]
	SashimiFrankenJellyfish,

	/// <summary>
	/// Indicates Siamese finned franken jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Jellyfish.html"
		])]
	SiameseFinnedFrankenJellyfish,

	/// <summary>
	/// Indicates Siamese sashimi franken jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html",
			"http://sudopedia.enjoysudoku.com/Franken_Jellyfish.html"
		])]
	SiameseSashimiFrankenJellyfish,

	/// <summary>
	/// Indicates mutant jellyfish.
	/// </summary>
	[Hodoku(Rating = 450, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0352")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Jellyfish.html"
		])]
	MutantJellyfish,

	/// <summary>
	/// Indicates finned mutant jellyfish.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0362")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Jellyfish.html"
		])]
	FinnedMutantJellyfish,

	/// <summary>
	/// Indicates sashimi mutant jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Jellyfish.html"
		])]
	SashimiMutantJellyfish,

	/// <summary>
	/// Indicates Siamese finned mutant jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Jellyfish.html"
		])]
	SiameseFinnedMutantJellyfish,

	/// <summary>
	/// Indicates Siamese sashimi mutant jellyfish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Jellyfish.html"
		])]
	SiameseSashimiMutantJellyfish,

	/// <summary>
	/// Indicates franken squirmbag.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0333")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	FrankenSquirmbag,

	/// <summary>
	/// Indicates finned franken squirmbag.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0343")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	FinnedFrankenSquirmbag,

	/// <summary>
	/// Indicates sashimi franken squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SashimiFrankenSquirmbag,

	/// <summary>
	/// Indicates Siamese finned franken squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	SiameseFinnedFrankenSquirmbag,

	/// <summary>
	/// Indicates Siamese sashimi franken squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SiameseSashimiFrankenSquirmbag,

	/// <summary>
	/// Indicates mutant squirmbag.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0353")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	MutantSquirmbag,

	/// <summary>
	/// Indicates finned mutant squirmbag.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0363")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	FinnedMutantSquirmbag,

	/// <summary>
	/// Indicates sashimi mutant squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SashimiMutantSquirmbag,

	/// <summary>
	/// Indicates Siamese finned mutant squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	SiameseFinnedMutantSquirmbag,

	/// <summary>
	/// Indicates Siamese sashimi mutant squirmbag.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SiameseSashimiMutantSquirmbag,

	/// <summary>
	/// Indicates franken whale.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0334")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	FrankenWhale,

	/// <summary>
	/// Indicates finned franken whale.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0344")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	FinnedFrankenWhale,

	/// <summary>
	/// Indicates sashimi franken whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SashimiFrankenWhale,

	/// <summary>
	/// Indicates Siamese finned franken whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	SiameseFinnedFrankenWhale,

	/// <summary>
	/// Indicates Siamese sashimi franken whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SiameseSashimiFrankenWhale,

	/// <summary>
	/// Indicates mutant whale.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0354")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	MutantWhale,

	/// <summary>
	/// Indicates finned mutant whale.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0364")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	FinnedMutantWhale,

	/// <summary>
	/// Indicates sashimi mutant whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SashimiMutantWhale,

	/// <summary>
	/// Indicates Siamese finned mutant whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	SiameseFinnedMutantWhale,

	/// <summary>
	/// Indicates Siamese sashimi mutant whale.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SiameseSashimiMutantWhale,

	/// <summary>
	/// Indicates franken leviathan.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0335")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	FrankenLeviathan,

	/// <summary>
	/// Indicates finned franken leviathan.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0345")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	FinnedFrankenLeviathan,

	/// <summary>
	/// Indicates sashimi franken leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SashimiFrankenLeviathan,

	/// <summary>
	/// Indicates Siamese finned franken leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Franken_Fish.html"
		])]
	SiameseFinnedFrankenLeviathan,

	/// <summary>
	/// Indicates Siamese sashimi franken leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Franken_Fish.html"])]
	SiameseSashimiFrankenLeviathan,

	/// <summary>
	/// Indicates mutant leviathan.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0355")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	MutantLeviathan,

	/// <summary>
	/// Indicates finned mutant leviathan.
	/// </summary>
	[Hodoku(Rating = 470, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "0365")]
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	FinnedMutantLeviathan,

	/// <summary>
	/// Indicates sashimi mutant leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SashimiMutantLeviathan,

	/// <summary>
	/// Indicates Siamese finned mutant leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = [
			"http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2793",
			"http://sudopedia.enjoysudoku.com/Mutant_Fish.html"
		])]
	SiameseFinnedMutantLeviathan,

	/// <summary>
	/// Indicates Siamese sashimi mutant leviathan.
	/// </summary>
	[TechniqueMetadata(
		Rating = 32,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ComplexFish,
		StepType = typeof(ComplexFishStep),
		StepSearcherType = typeof(ComplexFishStepSearcher),
		Links = ["http://forum.enjoysudoku.com/the-ultimate-fish-guide-t4993.html", "http://sudopedia.enjoysudoku.com/Mutant_Fish.html"])]
	SiameseSashimiMutantLeviathan,
	#endregion

	//
	// Regular Wings
	//
	#region Regular Wings
	/// <summary>
	/// Indicates XY-Wing.
	/// </summary>
	[Hodoku(Rating = 160, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0800")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.XyWing, RatingOriginal = [4.2])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/XY-Wing.html"])]
	XyWing,

	/// <summary>
	/// Indicates XYZ-Wing.
	/// </summary>
	[Hodoku(Rating = 180, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0801")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.XyzWing, RatingOriginal = [4.4])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/XYZ-Wing.html"])]
	XyzWing,

	/// <summary>
	/// Indicates WXYZ-Wing.
	/// </summary>
	[Hodoku(Prefix = "0802")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.XyzWing, RatingAdvanced = [4.6])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/WXYZ-Wing.html"])]
	WxyzWing,

	/// <summary>
	/// Indicates VWXYZ-Wing.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [double.NaN])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	VwxyzWing,

	/// <summary>
	/// Indicates UVWXYZ-Wing.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [double.NaN])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	UvwxyzWing,

	/// <summary>
	/// Indicates TUVWXYZ-Wing.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [double.NaN])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	TuvwxyzWing,

	/// <summary>
	/// Indicates STUVWXYZ-Wing.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [double.NaN])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	StuvwxyzWing,

	/// <summary>
	/// Indicates RSTUVWXYZ-Wing.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [double.NaN])]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	RstuvwxyzWing,

	/// <summary>
	/// Indicates incomplete WXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/WXYZ-Wing.html"])]
	IncompleteWxyzWing,

	/// <summary>
	/// Indicates incomplete VWXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher))]
	IncompleteVwxyzWing,

	/// <summary>
	/// Indicates incomplete UVWXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	IncompleteUvwxyzWing,

	/// <summary>
	/// Indicates incomplete TUVWXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	IncompleteTuvwxyzWing,

	/// <summary>
	/// Indicates incomplete STUVWXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	IncompleteStuvwxyzWing,

	/// <summary>
	/// Indicates incomplete RSTUVWXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RegularWing,
		StepType = typeof(RegularWingStep),
		StepSearcherType = typeof(RegularWingStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	IncompleteRstuvwxyzWing,
	#endregion

	//
	// Irregular Wings
	//
	#region Irregular Wings
	/// <summary>
	/// Indicates W-Wing.
	/// </summary>
	[Hodoku(Rating = 150, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0803")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.WWing, RatingAdvanced = [4.4])]
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(WWingStep),
		StepSearcherType = typeof(IrregularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/W-Wing.html"])]
	WWing,

	/// <summary>
	/// Indicates Multi-Branch W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(MultiBranchWWingStep),
		StepSearcherType = typeof(IrregularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/W-Wing.html"])]
	MultiBranchWWing,

	/// <summary>
	/// Indicates grouped W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(WWingStep),
		StepSearcherType = typeof(IrregularWingStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/W-Wing.html"])]
	GroupedWWing,

	/// <summary>
	/// Indicates M-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(MWingStep),
		StepSearcherType = typeof(IrregularWingStepSearcher))]
	MWing,

	/// <summary>
	/// Indicates grouped M-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(MWingStep),
		StepSearcherType = typeof(IrregularWingStepSearcher))]
	GroupedMWing,

	/// <summary>
	/// Indicates S-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher))]
	SWing,

	/// <summary>
	/// Indicates grouped S-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedSWing,

	/// <summary>
	/// Indicates local wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Links = ["http://forum.enjoysudoku.com/local-wing-t34685.html"])]
	LWing,

	/// <summary>
	/// Indicates grouped local wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Links = ["http://forum.enjoysudoku.com/local-wing-t34685.html"])]
	GroupedLWing,

	/// <summary>
	/// Indicates hybrid wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Links = ["http://forum.enjoysudoku.com/hybrid-wings-work-in-progress-t34212.html"])]
	HWing,

	/// <summary>
	/// Indicates grouped hybrid wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.IrregularWing,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Links = ["http://forum.enjoysudoku.com/hybrid-wings-work-in-progress-t34212.html"])]
	GroupedHWing,
	#endregion

	//
	// Almost Locked Candidates
	//
	#region Almost Locked Candidates
	/// <summary>
	/// Indicates almost locked pair.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [4.5])]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AlmostLockedCandidates,
		StepType = typeof(AlmostLockedCandidatesStep),
		StepSearcherType = typeof(AlmostLockedCandidatesStepSearcher),
		Abbreviation = "ALP",
		Links = ["http://sudopedia.enjoysudoku.com/Almost_Locked_Candidates.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4477"])]
	AlmostLockedPair,

	/// <summary>
	/// Indicates almost locked triple.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [5.2])]
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AlmostLockedCandidates,
		StepType = typeof(AlmostLockedCandidatesStep),
		StepSearcherType = typeof(AlmostLockedCandidatesStepSearcher),
		Abbreviation = "ALT",
		Links = ["http://sudopedia.enjoysudoku.com/Almost_Locked_Candidates.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4477"])]
	AlmostLockedTriple,

	/// <summary>
	/// Indicates almost locked quadruple.
	/// The technique may not be useful because it'll be replaced with Sue de Coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AlmostLockedCandidates,
		StepType = typeof(AlmostLockedCandidatesStep),
		StepSearcherType = typeof(AlmostLockedCandidatesStepSearcher),
		Abbreviation = "ALQ",
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://sudopedia.enjoysudoku.com/Almost_Locked_Candidates.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4477"])]
	AlmostLockedQuadruple,

	/// <summary>
	/// Indicates almost locked triple value type.
	/// The technique may not be often used because it'll be replaced with some kinds of Sue de Coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AlmostLockedCandidates,
		StepType = typeof(AlmostLockedCandidatesStep),
		StepSearcherType = typeof(AlmostLockedCandidatesStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Almost_Locked_Candidates.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4477"])]
	AlmostLockedTripleValueType,

	/// <summary>
	/// Indicates almost locked quadruple value type.
	/// The technique may not be often used because it'll be replaced with some kinds of Sue de Coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AlmostLockedCandidates,
		StepType = typeof(AlmostLockedCandidatesStep),
		StepSearcherType = typeof(AlmostLockedCandidatesStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Almost_Locked_Candidates.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4477"])]
	AlmostLockedQuadrupleValueType,
	#endregion

	//
	// Extended Subset Principle
	//
	#region Extended Subset Principle
	/// <summary>
	/// Indicates extended subset principle.
	/// </summary>
	[Hodoku(Prefix = "1102", Aliases = ["Subset Counting"])]
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ExtendedSubsetPrinciple,
		StepType = typeof(ExtendedSubsetPrincipleStep),
		StepSearcherType = typeof(ExtendedSubsetPrincipleStepSearcher),
		Abbreviation = "ESP",
		Links = ["http://sudopedia.enjoysudoku.com/Subset_Counting.html", "http://forum.enjoysudoku.com/viewtopic.php?t=3479"])]
	ExtendedSubsetPrinciple,
	#endregion

	//
	// Fat Ring
	//
	#region Fat Ring
	/// <summary>
	/// Indicates fat ring.
	/// </summary>
	[TechniqueMetadata(
		Rating = 65,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.FatRing,
		StepSearcherType = typeof(FatRingStepSearcher),
		StepType = typeof(FatRingStep))]
	FatRing,
	#endregion

	//
	// Unique Rectangle
	//
	#region Unique Rectangle
	/// <summary>
	/// Indicates unique rectangle type 1.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0600", Aliases = ["Uniqueness Test 1"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingOriginal = [4.5])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleType1Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2000",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType1,

	/// <summary>
	/// Indicates unique rectangle type 2.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0601", Aliases = ["Uniqueness Test 2"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingOriginal = [4.5], RatingAdvanced = [4.6])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleType2Or5Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2000",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType2,

	/// <summary>
	/// Indicates unique rectangle type 3.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0602", Aliases = ["Uniqueness Test 3"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingOriginal = [4.5, 4.8], RatingAdvanced = [4.6, 4.9])]
	[Npg(Rating = 150)]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleType3Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2000",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType3,

	/// <summary>
	/// Indicates unique rectangle type 4.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0603", Aliases = ["Uniqueness Test 4"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingOriginal = [4.5], RatingAdvanced = [4.6])]
	[Npg(Rating = 150)]
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=2000",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType4,

	/// <summary>
	/// Indicates unique rectangle type 5.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0604", Aliases = ["Uniqueness Test 5"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingAdvanced = [4.6])]
	[Npg(Rating = 100)]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleType2Or5Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType5,

	/// <summary>
	/// Indicates unique rectangle type 6.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0605", Aliases = ["Uniqueness Test 6"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingAdvanced = [4.6])]
	[Npg(Rating = 150)]
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleType6,

	/// <summary>
	/// Indicates hidden unique rectangle.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0606", Aliases = ["Hidden Rectangle"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueRectangle, RatingAdvanced = [4.8])]
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(HiddenUniqueRectangleStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Abbreviation = "HUR",
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	HiddenUniqueRectangle,

	/// <summary>
	/// Indicates unique rectangle + 2D.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangle2DOr3XStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle2D,

	/// <summary>
	/// Indicates unique rectangle + 2B / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle2B1,

	/// <summary>
	/// Indicates unique rectangle + 2D / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle2D1,

	/// <summary>
	/// Indicates unique rectangle + 3X.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangle2DOr3XStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3X,

	/// <summary>
	/// Indicates unique rectangle + 3x / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 49,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3X1L,

	/// <summary>
	/// Indicates unique rectangle + 3X / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 49,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3X1U,

	/// <summary>
	/// Indicates unique rectangle + 3X / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3X2,

	/// <summary>
	/// Indicates unique rectangle + 3N / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3N2,

	/// <summary>
	/// Indicates unique rectangle + 3U / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3U2,

	/// <summary>
	/// Indicates unique rectangle + 3E / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle3E2,

	/// <summary>
	/// Indicates unique rectangle + 4x / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 49,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4X1L,

	/// <summary>
	/// Indicates unique rectangle + 4X / 1SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 49,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4X1U,

	/// <summary>
	/// Indicates unique rectangle + 4x / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4X2L,

	/// <summary>
	/// Indicates unique rectangle + 4X / 2SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4X2U,

	/// <summary>
	/// Indicates unique rectangle + 4X / 3SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4X3,

	/// <summary>
	/// Indicates unique rectangle + 4C / 3SL.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleConjugatePairStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangle4C3,

	/// <summary>
	/// Indicates unique rectangle burred subset.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleBurredSubsetStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleBurredSubset,

	/// <summary>
	/// Indicates unique rectangle-XY-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleXyWing,

	/// <summary>
	/// Indicates unique rectangle-XYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleXyzWing,

	/// <summary>
	/// Indicates unique rectangle-WXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleWxyzWing,

	/// <summary>
	/// Indicates unique rectangle W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleWWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleWWing,

	/// <summary>
	/// Indicates unique rectangle sue de coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleSueDeCoq,

	/// <summary>
	/// Indicates unique rectangle singly-linked almost locked sets XZ.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleSinglyLinkedAlmostLockedSetsXz,

	/// <summary>
	/// Indicates unique rectangle doubly-linked almost locked sets XZ.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleDoublyLinkedAlmostLockedSetsXz,

	/// <summary>
	/// Indicates unique rectangle external type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalType1Or2Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html", "http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalType1,

	/// <summary>
	/// Indicates unique rectangle external type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalType1Or2Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalType2,

	/// <summary>
	/// Indicates unique rectangle external type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalType3Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalType3,

	/// <summary>
	/// Indicates unique rectangle external type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalType4Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalType4,

	/// <summary>
	/// Indicates unique rectangle external turbot fish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalTurbotFishStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalTurbotFish,

	/// <summary>
	/// Indicates unique rectangle external W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 48,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalWWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalWWing,

	/// <summary>
	/// Indicates unique rectangle external XY-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalXyWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalXyWing,

	/// <summary>
	/// Indicates unique rectangle external almost locked sets XZ rule.
	/// </summary>
	[TechniqueMetadata(
		Rating = 48,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueRectangle,
		StepType = typeof(UniqueRectangleExternalAlmostLockedSetsXzStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Unique_Rectangle.html",
			"http://forum.enjoysudoku.com/uniqueness-type-6-ur-meets-x-wing-t3709-30.html#p26448",
			"http://forum.enjoysudoku.com/unique-rectangles-gallery-t33752.html"
		])]
	UniqueRectangleExternalAlmostLockedSetsXz,
	#endregion

	//
	// Avoidable Rectangle
	//
	#region Avoidable Rectangle
	/// <summary>
	/// Indicates avoidable rectangle type 1.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0607")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AvoidableRectangle, RatingAdvanced = [4.7])] // I think this difficulty may be a mistake.
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleType1Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleType1,

	/// <summary>
	/// Indicates avoidable rectangle type 2.
	/// </summary>
	[Hodoku(Rating = 100, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0608")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AvoidableRectangle, RatingAdvanced = [4.5])] // I think this difficulty may be a mistake.
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleType2Or5Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleType2,

	/// <summary>
	/// Indicates avoidable rectangle type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleType3Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleType3,

	/// <summary>
	/// Indicates avoidable rectangle type 5.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleType2Or5Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleType5,

	/// <summary>
	/// Indicates hidden avoidable rectangle.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(HiddenUniqueRectangleStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Abbreviation = "HAR",
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	HiddenAvoidableRectangle,

	/// <summary>
	/// Indicates avoidable rectangle + 2D.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangle2DOr3XStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangle2D,

	/// <summary>
	/// Indicates avoidable rectangle + 3X.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangle2DOr3XStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangle3X,

	/// <summary>
	/// Indicates avoidable rectangle XY-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleXyWing,

	/// <summary>
	/// Indicates avoidable rectangle XYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleXyzWing,

	/// <summary>
	/// Indicates avoidable rectangle WXYZ-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleRegularWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleWxyzWing,

	/// <summary>
	/// Indicates avoidable rectangle W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleWWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleWWing,

	/// <summary>
	/// Indicates avoidable rectangle sue de coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleSueDeCoq,

	/// <summary>
	/// Indicates avoidable rectangle singly-linked almost locked sets XZ.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleSinglyLinkedAlmostLockedSetsXz,

	/// <summary>
	/// Indicates avoidable rectangle doubly-linked almost locked sets XZ.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleSueDeCoqStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleDoublyLinkedAlmostLockedSetsXz,

	/// <summary>
	/// Indicates avoidable rectangle hidden single in block.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(AvoidableRectangleHiddenSingleStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleHiddenSingleBlock,

	/// <summary>
	/// Indicates avoidable rectangle hidden single in row.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(AvoidableRectangleHiddenSingleStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleHiddenSingleRow,

	/// <summary>
	/// Indicates avoidable rectangle hidden single in column.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(AvoidableRectangleHiddenSingleStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleHiddenSingleColumn,

	/// <summary>
	/// Indicates avoidable rectangle external type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalType1Or2Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalType1,

	/// <summary>
	/// Indicates avoidable rectangle external type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalType1Or2Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalType2,

	/// <summary>
	/// Indicates avoidable rectangle external type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalType3Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalType3,

	/// <summary>
	/// Indicates avoidable rectangle external type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalType4Step),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalType4,

	/// <summary>
	/// Indicates avoidable rectangle external XY-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 47,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalXyWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalXyWing,

	/// <summary>
	/// Indicates avoidable rectangle external W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 48,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalWWingStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Features = TechniqueFeatures.NotImplemented,
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalWWing,

	/// <summary>
	/// Indicates avoidable rectangle external almost locked sets XZ rule.
	/// </summary>
	[TechniqueMetadata(
		Rating = 48,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AvoidableRectangle,
		StepType = typeof(UniqueRectangleExternalAlmostLockedSetsXzStep),
		StepSearcherType = typeof(UniqueRectangleStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Avoidable_Rectangle.html"])]
	AvoidableRectangleExternalAlmostLockedSetsXz,
	#endregion

	//
	// Unique Loop
	//
	#region Unique Loop
	/// <summary>
	/// Indicates unique loop type 1.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.UniqueLoop, RatingOriginal = [4.6, 5.0])]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueLoop,
		StepType = typeof(UniqueLoopType1Step),
		StepSearcherType = typeof(UniqueLoopStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=39748#p39748"])]
	UniqueLoopType1,

	/// <summary>
	/// Indicates unique loop type 2.
	/// </summary>
	[SudokuExplainer(
		Technique = SudokuExplainerTechnique.UniqueLoop,
		RatingOriginal = [4.6, 5.0],
		RatingAdvanced = [4.7, 5.1])]
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueLoop,
		StepType = typeof(UniqueLoopType2Step),
		StepSearcherType = typeof(UniqueLoopStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=39748#p39748"])]
	UniqueLoopType2,

	/// <summary>
	/// Indicates unique loop type 3.
	/// </summary>
	[SudokuExplainer(
		Technique = SudokuExplainerTechnique.UniqueLoop, RatingOriginal = [4.6, 5.0],
		RatingAdvanced = [4.7, 5.1])]
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueLoop,
		StepType = typeof(UniqueLoopType3Step),
		StepSearcherType = typeof(UniqueLoopStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=39748#p39748"])]
	UniqueLoopType3,

	/// <summary>
	/// Indicates unique loop type 4.
	/// </summary>
	[SudokuExplainer(
		Technique = SudokuExplainerTechnique.UniqueLoop, RatingOriginal = [4.6, 5.0],
		RatingAdvanced = [4.7, 5.1])]
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueLoop,
		StepType = typeof(UniqueLoopType4Step),
		StepSearcherType = typeof(UniqueLoopStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=39748#p39748"])]
	UniqueLoopType4,

	/// <summary>
	/// Indicates unique loop strong link type.
	/// </summary>
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.UniqueLoop,
		StepType = typeof(UniqueLoopConjugatePairsTypeStep),
		StepSearcherType = typeof(UniqueLoopStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=39748#p39748"])]
	UniqueLoopStrongLinkType,
	#endregion

	//
	// Extended Rectangle
	//
	#region Extended Rectangle
	/// <summary>
	/// Indicates extended rectangle type 1.
	/// </summary>
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[Hodoku(Prefix = "0620")]
#endif
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ExtendedRectangle,
		StepType = typeof(ExtendedRectangleType1Step),
		StepSearcherType = typeof(ExtendedRectangleStepSearcher))]
	ExtendedRectangleType1,

	/// <summary>
	/// Indicates extended rectangle type 2.
	/// </summary>
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[Hodoku(Prefix = "0621")]
#endif
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ExtendedRectangle,
		StepType = typeof(ExtendedRectangleType2Step),
		StepSearcherType = typeof(ExtendedRectangleStepSearcher))]
	ExtendedRectangleType2,

	/// <summary>
	/// Indicates extended rectangle type 3.
	/// </summary>
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[Hodoku(Prefix = "0622")]
#endif
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ExtendedRectangle,
		StepType = typeof(ExtendedRectangleType3Step),
		StepSearcherType = typeof(ExtendedRectangleStepSearcher))]
	ExtendedRectangleType3,

	/// <summary>
	/// Indicates extended rectangle type 3 (cannibalism).
	/// </summary>
	[TechniqueMetadata(
		Rating = 45,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ExtendedRectangle,
		StepType = typeof(ExtendedRectangleType3Step),
		StepSearcherType = typeof(ExtendedRectangleStepSearcher))]
	ExtendedRectangleType3Cannibalism,

	/// <summary>
	/// Indicates extended rectangle type 4.
	/// </summary>
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[Hodoku(Prefix = "0623")]
#endif
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.ExtendedRectangle,
		StepType = typeof(ExtendedRectangleType4Step),
		StepSearcherType = typeof(ExtendedRectangleStepSearcher))]
	ExtendedRectangleType4,
	#endregion

	//
	// Bivalue Universal Grave
	//
	#region Bivalue Universal Grave
	/// <summary>
	/// Indicates bi-value universal grave type 1.
	/// </summary>
	[Hodoku(
		Rating = 100,
		DifficultyLevel = HodokuDifficultyLevel.Hard,
		Prefix = "0610",
		Aliases = ["Bivalue Universal Grave + 1"])]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.BivalueUniversalGrave, RatingOriginal = [5.6])]
	[TechniqueMetadata(
		Rating = 56,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveType1Step),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveType1,

	/// <summary>
	/// Indicates bi-value universal grave type 2.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.BivalueUniversalGrave, RatingOriginal = [5.7])]
	[TechniqueMetadata(
		Rating = 56,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveType2Step),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveType2,

	/// <summary>
	/// Indicates bi-value universal grave type 3.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.BivalueUniversalGrave, RatingOriginal = [5.8, 6.1])]
	[TechniqueMetadata(
		Rating = 56,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveType3Step),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveType3,

	/// <summary>
	/// Indicates bi-value universal grave type 4.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.BivalueUniversalGrave, RatingOriginal = [5.7])]
	[TechniqueMetadata(
		Rating = 56,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveType4Step),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveType4,

	/// <summary>
	/// Indicates bi-value universal grave + n.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.BivalueUniversalGravePlusN, RatingAdvanced = [5.7])]
	[TechniqueMetadata(
		Rating = 57,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveMultipleStep),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Abbreviation = "BUG + n",
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGravePlusN,

	/// <summary>
	/// Indicates bi-value universal grave false candidate type.
	/// </summary>
	[TechniqueMetadata(
		Rating = 57,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveFalseCandidateTypeStep),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveFalseCandidateType,

	/// <summary>
	/// Indicates bi-value universal grave XZ rule.
	/// </summary>
	[TechniqueMetadata(
		Rating = 58,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		StepType = typeof(BivalueUniversalGraveXzStep),
		StepSearcherType = typeof(BivalueUniversalGraveStepSearcher),
		Abbreviation = "BUG-XZ",
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGraveXzRule,
	#endregion

	//
	// Reverse Bivalue Universal Grave
	//
	#region Reverse Bivalue Universal Grave
	/// <summary>
	/// Indicates reverse bi-value universal grave type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ReverseBivalueUniversalGrave,
		StepType = typeof(ReverseBivalueUniversalGraveType1Step),
		StepSearcherType = typeof(ReverseBivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Reverse_BUG.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4431"])]
	ReverseBivalueUniversalGraveType1,

	/// <summary>
	/// Indicates reverse bi-value universal grave type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 61,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ReverseBivalueUniversalGrave,
		StepType = typeof(ReverseBivalueUniversalGraveType2Step),
		StepSearcherType = typeof(ReverseBivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Reverse_BUG.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4431"])]
	ReverseBivalueUniversalGraveType2,

	/// <summary>
	/// Indicates reverse bi-value universal grave type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ReverseBivalueUniversalGrave,
		StepType = typeof(ReverseBivalueUniversalGraveType3Step),
		StepSearcherType = typeof(ReverseBivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Reverse_BUG.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4431"])]
	ReverseBivalueUniversalGraveType3,

	/// <summary>
	/// Indicates reverse bi-value universal grave type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 63,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ReverseBivalueUniversalGrave,
		StepType = typeof(ReverseBivalueUniversalGraveType4Step),
		StepSearcherType = typeof(ReverseBivalueUniversalGraveStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Reverse_BUG.html", "http://forum.enjoysudoku.com/viewtopic.php?t=4431"])]
	ReverseBivalueUniversalGraveType4,
	#endregion

	//
	// Uniqueness Clue Cover
	//
	#region Uniqueness Clue Cover
	/// <summary>
	/// Indicates uniqueness clue cover.
	/// </summary>
	[TechniqueMetadata(
		Rating = 65,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniquenessClueCover,
		StepType = typeof(UniquenessClueCoverStep),
		StepSearcherType = typeof(UniquenessClueCoverStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://sudopedia.enjoysudoku.com/Uniqueness_Clue_Cover.html", "http://forum.enjoysudoku.com/uniqueness-clue-cover-t40814.html"])]
	UniquenessClueCover,
	#endregion

	//
	// Borescoper's Deadly Pattern
	//
	#region Borescoper's Deadly Pattern
	/// <summary>
	/// Indicates Borescoper's deadly pattern type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BorescoperDeadlyPattern,
		StepType = typeof(BorescoperDeadlyPatternType1Step),
		StepSearcherType = typeof(BorescoperDeadlyPatternStepSearcher))]
	BorescoperDeadlyPatternType1,

	/// <summary>
	/// Indicates Borescoper's deadly pattern type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BorescoperDeadlyPattern,
		StepType = typeof(BorescoperDeadlyPatternType2Step),
		StepSearcherType = typeof(BorescoperDeadlyPatternStepSearcher))]
	BorescoperDeadlyPatternType2,

	/// <summary>
	/// Indicates Borescoper's deadly pattern type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BorescoperDeadlyPattern,
		StepType = typeof(BorescoperDeadlyPatternType3Step),
		StepSearcherType = typeof(BorescoperDeadlyPatternStepSearcher))]
	BorescoperDeadlyPatternType3,

	/// <summary>
	/// Indicates Borescoper's deadly pattern type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BorescoperDeadlyPattern,
		StepType = typeof(BorescoperDeadlyPatternType4Step),
		StepSearcherType = typeof(BorescoperDeadlyPatternStepSearcher))]
	BorescoperDeadlyPatternType4,
	#endregion

	//
	// Qiu's Deadly Pattern
	//
	#region Qiu's Deadly Pattern
	/// <summary>
	/// Indicates Qiu's deadly pattern type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 58,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternType1Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternType1,

	/// <summary>
	/// Indicates Qiu's deadly pattern type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 59,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternType2Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternType2,

	/// <summary>
	/// Indicates Qiu's deadly pattern type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 58,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternType3Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternType3,

	/// <summary>
	/// Indicates Qiu's deadly pattern type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternType4Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternType4,

	/// <summary>
	/// Indicates locked Qiu's deadly pattern.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternLockedTypeStep),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	LockedQiuDeadlyPattern,

	/// <summary>
	/// Indicates Qiu's deadly pattern external type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternExternalType1Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternExternalType1,

	/// <summary>
	/// Indicates Qiu's deadly pattern external type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 61,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.QiuDeadlyPattern,
		StepType = typeof(QiuDeadlyPatternExternalType2Step),
		StepSearcherType = typeof(QiuDeadlyPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/distinction-theory-t35042.html"])]
	QiuDeadlyPatternExternalType2,
	#endregion

	//
	// Unique Matrix
	//
	#region Unique Matrix
	/// <summary>
	/// Indicates unique matrix type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueMatrix,
		StepType = typeof(UniqueMatrixType1Step),
		StepSearcherType = typeof(UniqueMatrixStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	UniqueMatrixType1,

	/// <summary>
	/// Indicates unique matrix type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 54,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueMatrix,
		StepType = typeof(UniqueMatrixType2Step),
		StepSearcherType = typeof(UniqueMatrixStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	UniqueMatrixType2,

	/// <summary>
	/// Indicates unique matrix type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 53,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueMatrix,
		StepType = typeof(UniqueMatrixType3Step),
		StepSearcherType = typeof(UniqueMatrixStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	UniqueMatrixType3,

	/// <summary>
	/// Indicates unique matrix type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.UniqueMatrix,
		StepType = typeof(UniqueMatrixType4Step),
		StepSearcherType = typeof(UniqueMatrixStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	UniqueMatrixType4,
	#endregion

	//
	// Rotating Deadly Pattern
	//
	#region Rotating Deadly Pattern
	/// <summary>
	/// Indicates rotating deadly pattern type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 58,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType1Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	RotatingDeadlyPatternType1,

	/// <summary>
	/// Indicates rotating deadly pattern type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 59,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType2Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	RotatingDeadlyPatternType2,

	/// <summary>
	/// Indicates rotating deadly pattern type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType3Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	RotatingDeadlyPatternType3,

	/// <summary>
	/// Indicates rotating deadly pattern type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 59,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType4Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	RotatingDeadlyPatternType4,
	#endregion

	//
	// Anonymous Deadly Pattern
	//
	#region Anonymous Deadly Pattern
	/// <summary>
	/// Indicates anonymous deadly pattern type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType1Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	AnonymousDeadlyPatternType1,

	/// <summary>
	/// Indicates anonymous deadly pattern type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType2Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	AnonymousDeadlyPatternType2,

	/// <summary>
	/// Indicates anonymous deadly pattern type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType3Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	AnonymousDeadlyPatternType3,

	/// <summary>
	/// Indicates anonymous deadly pattern type 4.
	/// </summary>
	[TechniqueMetadata(
		Rating = 51,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.ConditionalDeadlyPattern,
		StepType = typeof(AnonymousDeadlyPatternType4Step),
		StepSearcherType = typeof(AnonymousDeadlyPatternStepSearcher))]
	AnonymousDeadlyPatternType4,
	#endregion

	//
	// Sue de Coq
	//
	#region Sue de Coq
	/// <summary>
	/// Indicates sue de coq.
	/// </summary>
	[Hodoku(Rating = 250, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "1101")]
	[SudokuExplainer(RatingAdvanced = [5.0])]
	[TechniqueMetadata(
		Rating = 50, DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.SueDeCoq,
		StepType = typeof(SueDeCoqStep),
		StepSearcherType = typeof(SueDeCoqStepSearcher),
		Abbreviation = "SdC",
		Links = [
			"http://sudopedia.enjoysudoku.com/Sue_de_Coq.html",
			"http://forum.enjoysudoku.com/two-sector-disjoint-subsets-t2033.html",
			"http://forum.enjoysudoku.com/benchmark-sudoku-list-t3834-15.html#p43170"
		])]
	SueDeCoq,

	/// <summary>
	/// Indicates sue de coq with isolated digit.
	/// </summary>
	[Hodoku(Rating = 250, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "1101")]
	[SudokuExplainer(RatingAdvanced = [5.0])]
	[TechniqueMetadata(
		Rating = 50, DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.SueDeCoq,
		StepType = typeof(SueDeCoqStep),
		StepSearcherType = typeof(SueDeCoqStepSearcher),
		Links = [
			"http://sudopedia.enjoysudoku.com/Sue_de_Coq.html",
			"http://forum.enjoysudoku.com/two-sector-disjoint-subsets-t2033.html",
			"http://forum.enjoysudoku.com/benchmark-sudoku-list-t3834-15.html#p43170"
		])]
	SueDeCoqIsolated,

	/// <summary>
	/// Indicates 3-dimensional sue de coq.
	/// </summary>
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.SueDeCoq,
		StepType = typeof(SueDeCoq3DimensionStep),
		StepSearcherType = typeof(SueDeCoq3DimensionStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Sue_de_Coq.html"])]
	SueDeCoq3Dimension,

	/// <summary>
	/// Indicates sue de coq cannibalism.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.SueDeCoq,
		StepType = typeof(SueDeCoqStep),
		StepSearcherType = typeof(SueDeCoqStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Sue_de_Coq.html"])]
	SueDeCoqCannibalism,
	#endregion

	//
	// Fireworks
	//
	#region Fireworks
	/// <summary>
	/// Indicates firework triple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.Firework,
		StepType = typeof(FireworkTripleStep),
		StepSearcherType = typeof(FireworkStepSearcher),
		Links = ["http://forum.enjoysudoku.com/fireworks-t39513.html"])]
	FireworkTriple,

	/// <summary>
	/// Indicates firework quadruple.
	/// </summary>
	[TechniqueMetadata(
		Rating = 63,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.Firework,
		StepType = typeof(FireworkQuadrupleStep),
		StepSearcherType = typeof(FireworkStepSearcher),
		Links = ["http://forum.enjoysudoku.com/fireworks-t39513.html"])]
	FireworkQuadruple,
	#endregion

	//
	// Broken Wing
	//
	#region Broken Wing
	/// <summary>
	/// Indicates broken wing.
	/// </summary>
	[Hodoku(Prefix = "0705")]
	[TechniqueMetadata(
		Rating = 55,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.BrokenWing,
		StepType = typeof(GuardianStep),
		StepSearcherType = typeof(GuardianStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Broken_Wing.html"])]
	BrokenWing,
	#endregion

	//
	// Bi-value Oddagon
	//
	#region Bivalue Oddagon
	/// <summary>
	/// Indicates bi-value oddagon type 2.
	/// </summary>
	[TechniqueMetadata(
		Rating = 61,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.BivalueOddagon,
		StepType = typeof(BivalueOddagonType2Step),
		StepSearcherType = typeof(BivalueOddagonStepSearcher),
		Links = ["http://forum.enjoysudoku.com/technique-share-odd-bivalue-loop-bivalue-oddagon-t33153.html"])]
	BivalueOddagonType2,

	/// <summary>
	/// Indicates bi-value oddagon type 3.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.BivalueOddagon,
		StepType = typeof(BivalueOddagonType3Step),
		StepSearcherType = typeof(BivalueOddagonStepSearcher),
		Links = ["http://forum.enjoysudoku.com/technique-share-odd-bivalue-loop-bivalue-oddagon-t33153.html"])]
	BivalueOddagonType3,
	#endregion

	//
	// Chromatic Pattern
	//
	#region Chromatic Pattern
	/// <summary>
	/// Indicates chromatic pattern (tri-value oddagon) type 1.
	/// </summary>
	[TechniqueMetadata(
		Rating = 65,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RankTheory,
		StepType = typeof(ChromaticPatternType1Step),
		StepSearcherType = typeof(ChromaticPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/chromatic-patterns-t39885.html", "http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html"])]
	ChromaticPatternType1,

	/// <summary>
	/// Indicates chromatic pattern (tri-value oddagon) type 2.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RankTheory,
		Features = TechniqueFeatures.HardToBeGenerated | TechniqueFeatures.NotImplemented,
		Links = ["http://forum.enjoysudoku.com/chromatic-patterns-t39885.html", "http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html"])]
	ChromaticPatternType2,

	/// <summary>
	/// Indicates chromatic pattern (tri-value oddagon) type 3.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RankTheory,
		Features = TechniqueFeatures.HardToBeGenerated | TechniqueFeatures.NotImplemented,
		Links = ["http://forum.enjoysudoku.com/chromatic-patterns-t39885.html", "http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html"])]
	ChromaticPatternType3,

	/// <summary>
	/// Indicates chromatic pattern (tri-value oddagon) type 4.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RankTheory,
		Features = TechniqueFeatures.HardToBeGenerated | TechniqueFeatures.NotImplemented,
		Links = ["http://forum.enjoysudoku.com/chromatic-patterns-t39885.html", "http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html"])]
	ChromaticPatternType4,

	/// <summary>
	/// Indicates chromatic pattern (tri-value oddagon) XZ rule.
	/// </summary>
	[TechniqueMetadata(
		Rating = 67,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.RankTheory,
		StepType = typeof(ChromaticPatternXzStep),
		StepSearcherType = typeof(ChromaticPatternStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated)]
	ChromaticPatternXzRule,
	#endregion

	//
	// Single Digit Pattern
	//
	#region Single Digit Pattern
	/// <summary>
	/// Indicates skyscraper.
	/// </summary>
	[Hodoku(Rating = 130, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0400")]
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[SudokuExplainer(Technique = SudokuExplainerTechnique.TurbotFish, RatingOriginal = [6.6], Aliases = ["Turbot Fish"])]
#endif
	[Npg(Rating = 50)]
	[TechniqueMetadata(
		Rating = 40,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Skyscraper.html"])]
	Skyscraper,

	/// <summary>
	/// Indicates two-string kite.
	/// </summary>
	[Hodoku(Rating = 150, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0401")]
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[SudokuExplainer(Rating = 66, TechniqueDefined = SudokuExplainerTechnique.TurbotFish, Aliases = ["Turbot Fish"])]
#endif
	[Npg(Rating = 50)]
	[TechniqueMetadata(
		Rating = 41,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/2-String_Kite.html"])]
	TwoStringKite,

	/// <summary>
	/// Indicates turbot fish.
	/// </summary>
	[Hodoku(Rating = 120, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0403")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.TurbotFish, RatingOriginal = [6.6])]
	[Npg(Rating = 50)]
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?t=833"])]
	TurbotFish,

	/// <summary>
	/// Indicates grouped skyscraper.
	/// </summary>
	[TechniqueMetadata(
		Rating = 42,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Skyscraper.html"])]
	GroupedSkyscraper,

	/// <summary>
	/// Indicates grouped two-string kite.
	/// </summary>
	[TechniqueMetadata(
		Rating = 43,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/2-String_Kite.html"])]
	GroupedTwoStringKite,

	/// <summary>
	/// Indicates grouped turbot fish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 44,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.SingleDigitPattern,
		StepType = typeof(TwoStrongLinksStep),
		StepSearcherType = typeof(TwoStrongLinksStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/2-String_Kite.html"])]
	GroupedTurbotFish,
	#endregion

	//
	// Empty Rectangle
	//
	#region Empty Rectangle
	/// <summary>
	/// Indicates empty rectangle.
	/// </summary>
	[Hodoku(Rating = 120, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0402")]
	[TechniqueMetadata(
		Rating = 46,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.EmptyRectangle,
		StepType = typeof(EmptyRectangleStep),
		StepSearcherType = typeof(EmptyRectangleStepSearcher),
		Abbreviation = "ER",
		Links = ["http://sudopedia.enjoysudoku.com/Empty_Rectangle.html"])]
	EmptyRectangle,
	#endregion

	//
	// Remote Pair
	//
	#region Remote Pair
	/// <summary>
	/// Indicates remote pair.
	/// </summary>
	[SudokuExplainer(RatingAdvanced = [5.0], Technique = SudokuExplainerTechnique.RemotePair)]
	[Hodoku(Rating = 110, DifficultyLevel = HodokuDifficultyLevel.Hard, Prefix = "0703")]
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BabaGrouping,
		StepType = typeof(RemotePairStep),
		StepSearcherType = typeof(RemotePairStepSearcher))]
	RemotePair,

	/// <summary>
	/// Indicates complex remote pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Hard,
		ContainingGroup = TechniqueGroup.BabaGrouping,
		StepType = typeof(RemotePairStep),
		StepSearcherType = typeof(RemotePairStepSearcher))]
	ComplexRemotePair,
	#endregion

	//
	// Chains & Loops
	//
	#region Chains & Loops
	/// <summary>
	/// Indicates X-Chain.
	/// </summary>
	[Hodoku(Rating = 260, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0701")]
	[SudokuExplainer(RatingOriginal = [6.6, 6.9])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/X-Chain.html"])]
	XChain,

	/// <summary>
	/// Indicates Y-Chain.
	/// </summary>
#if COMPATIBLE_ORIGINAL_TECHNIQUE_RULES
	[Hodoku(Rating = 260, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0702")]
#endif
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique)]
	YChain,

	/// <summary>
	/// Indicates fishy cycle (X-Cycle).
	/// </summary>
	[Hodoku(Prefix = "0704")]
	[SudokuExplainer(RatingOriginal = [6.5, 6.6], Aliases = ["Bidirectional X-Cycle"])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Fishy_Cycle.html"])]
	FishyCycle,

	/// <summary>
	/// Indicates XY-Chain.
	/// </summary>
	[Hodoku(Rating = 260, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0702")]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/XY-Chain.html"])]
	XyChain,

	/// <summary>
	/// Indicates XY-Cycle.
	/// </summary>
	[SudokuExplainer(RatingOriginal = [6.6, 7.0])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher))]
	XyCycle,

	/// <summary>
	/// Indicates XY-X-Chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher))]
	XyXChain,

	/// <summary>
	/// Indicates purple cow.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher))]
	PurpleCow,

	/// <summary>
	/// Indicates discontinuous nice loop.
	/// </summary>
	[Hodoku(Rating = 280, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0707")]
	[SudokuExplainer(RatingOriginal = [7.0, 7.6], Aliases = ["Forcing Chain"])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Abbreviation = "DNL",
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?t=2859"])]
	DiscontinuousNiceLoop,

	/// <summary>
	/// Indicates continuous nice loop.
	/// </summary>
	[Hodoku(Rating = 280, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0706")]
	[SudokuExplainer(RatingOriginal = [7.0, 7.3], Aliases = ["Bidirectional Cycle"])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Abbreviation = "CNL",
		Links = ["http://sudopedia.enjoysudoku.com/Nice_Loop.html"])]
	ContinuousNiceLoop,

	/// <summary>
	/// Indicates alternating inference chain.
	/// </summary>
	[Hodoku(Rating = 280, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0708")]
	[SudokuExplainer(RatingOriginal = [7.0, 7.6])]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher),
		Abbreviation = "AIC",
		Links = ["http://sudopedia.enjoysudoku.com/Alternating_Inference_Chain.html", "http://forum.enjoysudoku.com/viewtopic.php?t=3865"])]
	AlternatingInferenceChain,

	/// <summary>
	/// Indicates grouped X-Chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedXChain,

	/// <summary>
	/// Indicates grouped fishy cycle (grouped X-Cycle).
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedFishyCycle,

	/// <summary>
	/// Indicates grouped XY-Chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedXyChain,

	/// <summary>
	/// Indicates grouped XY-Cycle.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedXyCycle,

	/// <summary>
	/// Indicates grouped XY-X-Chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedXyXChain,

	/// <summary>
	/// Indicates grouped purple cow.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedPurpleCow,

	/// <summary>
	/// Indicates grouped discontinuous nice loop.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0710")]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedDiscontinuousNiceLoop,

	/// <summary>
	/// Indicates grouped continuous nice loop.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0709")]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedContinuousNiceLoop,

	/// <summary>
	/// Indicates grouped alternating inference chain.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0711")]
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedAlternatingInferenceChain,

	/// <summary>
	/// Indicates special case that a chain starts and ends with a same node of a single candidate.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(ChainStepSearcher))]
	SelfConstraint,

	/// <summary>
	/// Indicates grouped self constraint.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedSelfConstraint,

	/// <summary>
	/// Indicates special case that a grouped alternating inference chain has a collision between start and end node.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	NodeCollision,

	/// <summary>
	/// Indicates finned chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 62,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(FinnedChainStep),
		StepSearcherType = typeof(MultipleForcingChainsStepSearcher))]
	FinnedChain,

	/// <summary>
	/// Indicates finned grouped chain.
	/// </summary>
	[TechniqueMetadata(
		Rating = 62,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlternatingInferenceChain,
		StepType = typeof(FinnedChainStep),
		StepSearcherType = typeof(MultipleForcingChainsStepSearcher))]
	FinnedGroupedChain,
	#endregion

	//
	// Blossom Loop
	//
	#region Blossom Loop
	/// <summary>
	/// Indicates blossom loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 65,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.BlossomLoop,
		Links = ["http://forum.enjoysudoku.com/blossom-loop-t42270.html"])]
	BlossomLoop,
	#endregion

	//
	// Forcing Chains
	//
	#region Forcing Chains
	/// <summary>
	/// Indicates nishio forcing chains.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.NishioForcingChain, RatingOriginal = [7.6, 8.1])]
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains,
		Links = ["http://sudopedia.enjoysudoku.com/Nishio.html"],
		Features = TechniqueFeatures.NotImplemented)]
	NishioForcingChains,

	/// <summary>
	/// Indicates region forcing chains (i.e. house forcing chains).
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1301")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.MultipleForcingChain, RatingOriginal = [8.2, 8.6])]
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	RegionForcingChains,

	/// <summary>
	/// Indicates cell forcing chains.
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1301")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.MultipleForcingChain, RatingOriginal = [8.2, 8.6])]
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	CellForcingChains,

	/// <summary>
	/// Indicates merged region forcing chains (i.e. merged house forcing chains).
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1301")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.MultipleForcingChain, RatingOriginal = [8.2, 8.6])]
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	MergedRegionForcingChains,

	/// <summary>
	/// Indicates merged cell forcing chains.
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1301")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.MultipleForcingChain, RatingOriginal = [8.2, 8.6])]
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	MergedCellForcingChains,

	/// <summary>
	/// Indicates rectangle forcing chains.
	/// </summary>
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	RectangleForcingChains,

	/// <summary>
	/// Indicates bi-value universal grave + n with forcing chains.
	/// </summary>
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.BivalueUniversalGrave,
		Links = ["http://sudopedia.enjoysudoku.com/Bivalue_Universal_Grave.html", "http://forum.enjoysudoku.com/viewtopic.php?t=2352"])]
	BivalueUniversalGravePlusNForcingChains,

	/// <summary>
	/// Indicates dynamic region forcing chains (i.e. dynamic house forcing chains).
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1303")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DynamicForcingChain, RatingOriginal = [8.6, 9.4])]
	[TechniqueMetadata(
		Rating = 88,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	DynamicRegionForcingChains,

	/// <summary>
	/// Indicates dynamic cell forcing chains.
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1303")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DynamicForcingChain, RatingOriginal = [8.6, 9.4])]
	[TechniqueMetadata(
		Rating = 88,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	DynamicCellForcingChains,

	/// <summary>
	/// Indicates dynamic contradiction forcing chains.
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1304")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DynamicForcingChain, RatingOriginal = [8.8, 9.4])]
	[TechniqueMetadata(
		Rating = 88,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	DynamicContradictionForcingChains,

	/// <summary>
	/// Indicates dynamic double forcing chains.
	/// </summary>
	[Hodoku(Rating = 500, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1304")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.DynamicForcingChain, RatingOriginal = [8.8, 9.4])]
	[TechniqueMetadata(
		Rating = 88,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains)]
	DynamicDoubleForcingChains,

	/// <summary>
	/// Indicates dynamic forcing chains.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.ForcingChains,
		Features = TechniqueFeatures.NotImplemented)]
	DynamicForcingChains,
	#endregion

	//
	// Whips & Braids
	//
	#region Whips & Braids
	/// <summary>
	/// Indicates whip.
	/// </summary>
	[TechniqueMetadata(
		Rating = 80,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.WhipAndBraid,
		Links = ["http://forum.enjoysudoku.com/fully-supersymmetric-chains-t5591-195.html"])]
	Whip,

	/// <summary>
	/// Indicates grouped whip.
	/// </summary>
	[TechniqueMetadata(
		Rating = 82,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.WhipAndBraid,
		Links = ["http://forum.enjoysudoku.com/fully-supersymmetric-chains-t5591-195.html"])]
	GroupedWhip,
	#endregion

	//
	// Aligned Exclusion
	//
	#region Aligned Exclusion
	/// <summary>
	/// Indicates aligned pair exclusion.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AlignedPairExclusion, RatingOriginal = [6.2])]
	[TechniqueMetadata(
		Rating = 62,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlignedExclusion,
		StepType = typeof(AlignedExclusionStep),
		StepSearcherType = typeof(AlignedExclusionStepSearcher),
		Abbreviation = "APE",
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = [
			"http://sudopedia.enjoysudoku.com/Subset_Exclusion.html",
			"http://sudopedia.enjoysudoku.com/Aligned_Pair_Exclusion.html",
			"http://forum.enjoysudoku.com/viewtopic.php?t=3882"
		])]
	AlignedPairExclusion,

	/// <summary>
	/// Indicates aligned triple exclusion.
	/// </summary>
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AlignedTripletExclusion, RatingOriginal = [7.5])]
	[TechniqueMetadata(
		Rating = 75,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlignedExclusion,
		StepType = typeof(AlignedExclusionStep),
		StepSearcherType = typeof(AlignedExclusionStepSearcher),
		Abbreviation = "ATE",
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Subset_Exclusion.html", "http://sudopedia.enjoysudoku.com/Aligned_Pair_Exclusion.html",])]
	AlignedTripleExclusion,

	/// <summary>
	/// Indicates aligned quadruple exclusion.
	/// </summary>
	[TechniqueMetadata(
		Rating = 81,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlignedExclusion,
		StepType = typeof(AlignedExclusionStep),
		StepSearcherType = typeof(AlignedExclusionStepSearcher),
		Abbreviation = "AQE",
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Subset_Exclusion.html"])]
	AlignedQuadrupleExclusion,

	/// <summary>
	/// Indicates aligned quintuple exclusion.
	/// </summary>
	[TechniqueMetadata(
		Rating = 84,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlignedExclusion,
		StepType = typeof(AlignedExclusionStep),
		StepSearcherType = typeof(AlignedExclusionStepSearcher),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Subset_Exclusion.html"])]
	AlignedQuintupleExclusion,
	#endregion

	//
	// XYZ-Ring
	//
	#region XYZ-Ring
	/// <summary>
	/// Indicates XYZ loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	XyzLoop,

	/// <summary>
	/// Indicates Siamese XYZ loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	SiameseXyzLoop,

	/// <summary>
	/// Indicates XYZ nice loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	XyzNiceLoop,

	/// <summary>
	/// Indicates Siamese XYZ nice loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	SiameseXyzNiceLoop,

	/// <summary>
	/// Indicates grouped XYZ loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	GroupedXyzLoop,

	/// <summary>
	/// Indicates Siamese grouped XYZ loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 50,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	SiameseGroupedXyzLoop,

	/// <summary>
	/// Indicates grouped XYZ nice loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	GroupedXyzNiceLoop,

	/// <summary>
	/// Indicates Siamese grouped XYZ nice loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 52,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.XyzRing,
		StepType = typeof(XyzRingStep),
		StepSearcherType = typeof(XyzRingStepSearcher),
		Links = ["http://forum.enjoysudoku.com/xyz-ring-t42209.html"])]
	SiameseGroupedXyzNiceLoop,
	#endregion

	//
	// Almost Locked Sets
	//
	#region Almost Locked Sets
	/// <summary>
	/// Indicates singly linked ALS-XZ.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0901")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AlsXz, RatingAdvanced = [7.5])]
	[TechniqueMetadata(
		Rating = 66,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Abbreviation = "ALS-XZ",
		Links = ["http://sudopedia.enjoysudoku.com/ALS-XZ.html"])]
	SinglyLinkedAlmostLockedSetsXzRule,

	/// <summary>
	/// Indicates doubly linked ALS-XZ.
	/// </summary>
	[Hodoku(Rating = 300, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0901")]
	[SudokuExplainer(RatingAdvanced = [7.5])]
	[TechniqueMetadata(
		Rating = 67,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Abbreviation = "ALS-XZ",
		Links = ["http://sudopedia.enjoysudoku.com/ALS-XZ.html"])]
	DoublyLinkedAlmostLockedSetsXzRule,

	/// <summary>
	/// Indicates ALS-XY-Wing.
	/// </summary>
	[Hodoku(Rating = 320, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0902")]
	[SudokuExplainer(Technique = SudokuExplainerTechnique.AlsXyWing, RatingAdvanced = [8.0])]
	[TechniqueMetadata(
		Rating = 69,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Abbreviation = "ALS-XY-Wing",
		Links = ["http://sudopedia.enjoysudoku.com/ALS-XY-Wing.html"])]
	AlmostLockedSetsXyWing,

	/// <summary>
	/// Indicates ALS-W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 68,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Abbreviation = "ALS-W-Wing")]
	AlmostLockedSetsWWing,

	/// <summary>
	/// Indicates ALS chain.
	/// </summary>
	[Hodoku(Rating = 340, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0903")]
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/ALS-XY-Chain.html"])]
	AlmostLockedSetsChain,

	/// <summary>
	/// Indicates grouped ALS-W-Wing.
	/// </summary>
	[TechniqueMetadata(
		Rating = 69,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.AlmostLockedSetsChainingLike,
		StepType = typeof(NormalChainStep),
		StepSearcherType = typeof(GroupedChainStepSearcher))]
	GroupedAlmostLockedSetsWWing,
	#endregion

	//
	// Empty Rectangle Intersection Pair
	//
	#region Empty Rectangle Intersection Pair
	/// <summary>
	/// Indicates empty rectangle intersection pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 60,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.EmptyRectangleIntersectionPair,
		StepType = typeof(EmptyRectangleIntersectionPairStep),
		StepSearcherType = typeof(EmptyRectangleIntersectionPairStepSearcher),
		Abbreviation = "ERIP",
		Links = ["http://forum.enjoysudoku.com/post288015.html"])]
	EmptyRectangleIntersectionPair,
	#endregion

	//
	// Death Blossom
	//
	#region Death Blossom
	/// <summary>
	/// Indicates death blossom.
	/// </summary>
	[Hodoku(Rating = 360, DifficultyLevel = HodokuDifficultyLevel.Unfair, Prefix = "0904")]
	[TechniqueMetadata(
		Rating = 82,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.DeathBlossom,
		StepType = typeof(NormalDeathBlossomStep),
		StepSearcherType = typeof(DeathBlossomStepSearcher),
		Abbreviation = "DB",
		Links = ["http://sudopedia.enjoysudoku.com/Death_Blossom.html"])]
	DeathBlossom,

	/// <summary>
	/// Indicates death blossom (house blooming).
	/// </summary>
	[TechniqueMetadata(
		Rating = 83,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.DeathBlossom,
		StepType = typeof(HouseDeathBlossomStep),
		StepSearcherType = typeof(DeathBlossomStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Death_Blossom.html"])]
	HouseDeathBlossom,

	/// <summary>
	/// Indicates death blossom (A^nLS blooming).
	/// </summary>
	[TechniqueMetadata(
		Rating = 87,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.DeathBlossom,
		StepType = typeof(NTimesAlmostLockedSetsDeathBlossomStep),
		StepSearcherType = typeof(DeathBlossomStepSearcher),
		Links = ["http://sudopedia.enjoysudoku.com/Death_Blossom.html"])]
	NTimesAlmostLockedSetsDeathBlossom,
	#endregion

	//
	// Symmetry
	//
	#region Symmetry
	/// <summary>
	/// Indicates Gurth's symmetrical placement.
	/// </summary>
	[TechniqueMetadata(
		Rating = 70,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.Symmetry,
		StepType = typeof(GurthSymmetricalPlacementStep),
		Abbreviation = "GSP",
		Features = TechniqueFeatures.HardToBeGenerated,
		SpecialFlags = TechniqueMetadataSpecialFlags.SymmetricalPlacement,
		Links = ["http://forum.enjoysudoku.com/viewtopic.php?p=32842#p32842"])]
	GurthSymmetricalPlacement,

	/// <summary>
	/// Indicates extended Gurth's symmetrical placement.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Symmetry,
		Features = TechniqueFeatures.NotImplemented | TechniqueFeatures.HardToBeGenerated)]
	ExtendedGurthSymmetricalPlacement,

	/// <summary>
	/// Indicates Anti-GSP (Anti- Gurth's Symmetrical Placement).
	/// </summary>
	[TechniqueMetadata(
		Rating = 73,
		DifficultyLevel = DifficultyLevel.Fiendish,
		ContainingGroup = TechniqueGroup.Symmetry,
		StepType = typeof(AntiGurthSymmetricalPlacementStep),
		StepSearcherType = typeof(AntiGurthSymmetricalPlacementStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/new-type-of-gsp-t40470.html"])]
	AntiGurthSymmetricalPlacement,
	#endregion

	//
	// Exocet
	//
	#region Exocet
	/// <summary>
	/// Indicates junior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Abbreviation = "JE",
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocet,

	/// <summary>
	/// Indicates junior exocet with target conjugate pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetConjugatePair,

	/// <summary>
	/// Indicates junior exocet mirror mirror conjugate pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ExocetMirrorConjugatePairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetMirrorConjugatePair,

	/// <summary>
	/// Indicates junior exocet adjacent target.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetAdjacentTargetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetAdjacentTarget,

	/// <summary>
	/// Indicates junior exocet incompatible pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetIncompatiblePairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetIncompatiblePair,

	/// <summary>
	/// Indicates junior exocet target pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetTargetPairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetTargetPair,

	/// <summary>
	/// Indicates junior exocet generalized fish.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetGeneralizedFishStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetGeneralizedFish,

	/// <summary>
	/// Indicates junior exocet mirror almost hidden set.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetMirrorAlmostHiddenSetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetMirrorAlmostHiddenSet,

	/// <summary>
	/// Indicates junior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetLockedMember,

	/// <summary>
	/// Indicates junior exocet mirror sync.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(JuniorExocetMirrorSyncStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	JuniorExocetMirrorSync,

	/// <summary>
	/// Indicates senior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 96,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Abbreviation = "SE",
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	SeniorExocet,

	/// <summary>
	/// Indicates senior exocet mirror conjugate pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ExocetMirrorConjugatePairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	SeniorExocetMirrorConjugatePair,

	/// <summary>
	/// Indicates senior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	SeniorExocetLockedMember,

	/// <summary>
	/// Indicates senior exocet true base.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(SeniorExocetTrueBaseStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	SeniorExocetTrueBase,

	/// <summary>
	/// Indicates weak exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 97,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(WeakExocetStep),
		Abbreviation = "WE",
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html", "http://forum.enjoysudoku.com/weak-exocet-t39651.html"])]
	WeakExocet,

	/// <summary>
	/// Indicates weak exocet adjacent target.
	/// </summary>
	[TechniqueMetadata(
		Rating = 97,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(WeakExocetAdjacentTargetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html", "http://forum.enjoysudoku.com/weak-exocet-t39651.html"])]
	WeakExocetAdjacentTarget,

	/// <summary>
	/// Indicates weak exocet slash.
	/// </summary>
	[TechniqueMetadata(
		Rating = 97,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(WeakExocetSlashStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html", "http://forum.enjoysudoku.com/weak-exocet-t39651.html"])]
	WeakExocetSlash,

	/// <summary>
	/// Indicates weak exocet BZ rectangle.
	/// </summary>
	[TechniqueMetadata(
		Rating = 97,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(WeakExocetBzRectangleStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html", "http://forum.enjoysudoku.com/weak-exocet-t39651.html"])]
	WeakExocetBzRectangle,

	/// <summary>
	/// Indicates lame weak exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 97,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(WeakExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html", "http://forum.enjoysudoku.com/weak-exocet-t39651.html"])]
	LameWeakExocet,

	/// <summary>
	/// Indicates franken junior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 98,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalComplexExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenJuniorExocet,

	/// <summary>
	/// Indicates franken junior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 98,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenJuniorExocetLockedMember,

	/// <summary>
	/// Indicates franken junior exocet adjacent target.
	/// </summary>
	[TechniqueMetadata(
		Rating = 98,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexJuniorExocetAdjacentTargetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenJuniorExocetAdjacentTarget,

	/// <summary>
	/// Indicates franken junior exocet mirror conjugate pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 98,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexJuniorExocetMirrorConjugatePairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenJuniorExocetMirrorConjugatePair,

	/// <summary>
	/// Indicates mutant junior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalComplexExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantJuniorExocet,

	/// <summary>
	/// Indicates mutant junior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantJuniorExocetLockedMember,

	/// <summary>
	/// Indicates mutant junior exocet adjacent target.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexJuniorExocetAdjacentTargetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantJuniorExocetAdjacentTarget,

	/// <summary>
	/// Indicates mutant junior exocet mirror conjugate pair.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexJuniorExocetMirrorConjugatePairStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantJuniorExocetMirrorConjugatePair,

	/// <summary>
	/// Indicates franken senior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalComplexExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenSeniorExocet,

	/// <summary>
	/// Indicates franken senior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 100,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	FrankenSeniorExocetLockedMember,

	/// <summary>
	/// Indicates advanced franken senior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 98,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(AdvancedComplexSeniorExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	AdvancedFrankenSeniorExocet,

	/// <summary>
	/// Indicates mutant senior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 102,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalComplexExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantSeniorExocet,

	/// <summary>
	/// Indicates mutant senior exocet locked member.
	/// </summary>
	[TechniqueMetadata(
		Rating = 102,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(ComplexExocetLockedMemberStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	MutantSeniorExocetLockedMember,

	/// <summary>
	/// Indicates advanced mutant senior exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 101,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(AdvancedComplexSeniorExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	AdvancedMutantSeniorExocet,

	/// <summary>
	/// Indicates double exocet.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(NormalDoubleExocetStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	DoubleExocet,

	/// <summary>
	/// Indicates double exocet uni-fish pattern.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		StepType = typeof(DoubleExocetGeneralizedFishStep),
		StepSearcherType = typeof(ExocetStepSearcher),
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	DoubleExocetGeneralizedFish,

	/// <summary>
	/// Indicates pattern-locked quadruple. This quadruple is a special quadruple: it can only be concluded after both JE and SK-Loop are formed.
	/// </summary>
	[TechniqueMetadata(
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.Exocet,
		Abbreviation = "PLQ",
		Features = TechniqueFeatures.NotImplemented,
		Links = ["http://forum.enjoysudoku.com/jexocet-compendium-t32370.html"])]
	PatternLockedQuadruple,
	#endregion

	//
	// Domino Loop
	//
	#region Domino Loop
	/// <summary>
	/// Indicates domino loop.
	/// </summary>
	[TechniqueMetadata(
		Rating = 96,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.DominoLoop,
		StepType = typeof(DominoLoopStep),
		StepSearcherType = typeof(DominoLoopStepSearcher),
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/domino-loops-sk-loops-beyond-t32789.html"])]
	DominoLoop,
	#endregion

	//
	// Multi-sector Locked Sets
	//
	#region Multi-sector Locked Sets
	/// <summary>
	/// Indicates multi-sector locked sets.
	/// </summary>
	[TechniqueMetadata(
		Rating = 94,
		DifficultyLevel = DifficultyLevel.Nightmare,
		ContainingGroup = TechniqueGroup.MultisectorLockedSets,
		StepType = typeof(MultisectorLockedSetsStep),
		StepSearcherType = typeof(MultisectorLockedSetsStepSearcher),
		Abbreviation = "MSLS",
		Features = TechniqueFeatures.HardToBeGenerated,
		Links = ["http://forum.enjoysudoku.com/exotic-patterns-a-resume-t30508-270.html"])]
	MultisectorLockedSets,
	#endregion

	//
	// Pattern Overlay
	//
	#region Pattern Overlay
	/// <summary>
	/// Indicates pattern overlay method.
	/// </summary>
	[TechniqueMetadata(
		Rating = 85,
		DifficultyLevel = DifficultyLevel.LastResort,
		ContainingGroup = TechniqueGroup.PatternOverlay,
		StepType = typeof(PatternOverlayStep),
		StepSearcherType = typeof(PatternOverlayStepSearcher),
		Abbreviation = "POM",
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Pattern_Overlay_Method.html"])]
	PatternOverlay,
	#endregion

	//
	// Templating
	//
	#region Templating
	/// <summary>
	/// Indicates template set.
	/// </summary>
	[Hodoku(Rating = 10000, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1201")]
	[TechniqueMetadata(
		Rating = 90,
		DifficultyLevel = DifficultyLevel.LastResort,
		ContainingGroup = TechniqueGroup.Templating,
		StepType = typeof(TemplateStep),
		StepSearcherType = typeof(TemplateStepSearcher),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Templating.html"])]
	TemplateSet,

	/// <summary>
	/// Indicates template delete.
	/// </summary>
	[Hodoku(Rating = 10000, DifficultyLevel = HodokuDifficultyLevel.Extreme, Prefix = "1202")]
	[TechniqueMetadata(
		Rating = 90,
		DifficultyLevel = DifficultyLevel.LastResort,
		ContainingGroup = TechniqueGroup.Templating,
		StepType = typeof(TemplateStep),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Templating.html"])]
	TemplateDelete,
	#endregion

	//
	// Bowman's Bingo
	//
	#region Bowman's Bingo
	/// <summary>
	/// Indicates bowman's bingo.
	/// </summary>
	[TechniqueMetadata(
		Rating = 80,
		DifficultyLevel = DifficultyLevel.LastResort,
		ContainingGroup = TechniqueGroup.BowmanBingo,
		StepType = typeof(BowmanBingoStep),
		StepSearcherType = typeof(BowmanBingoStepSearcher),
		Features = TechniqueFeatures.WillBeReplacedByOtherTechnique,
		Links = ["http://sudopedia.enjoysudoku.com/Bowman_Bingo.html"])]
	BowmanBingo,
	#endregion

	//
	// Brute Force
	//
	#region Brute Force
	/// <summary>
	/// Indicates brute force.
	/// </summary>
	[Hodoku(Rating = 10000, DifficultyLevel = HodokuDifficultyLevel.Extreme)]
	[SudokuExplainer(
		Technique = SudokuExplainerTechnique.BruteForce,
		RatingOriginal = [AnalysisResult.MaximumRatingValueTheory / 10D],
		Aliases = ["Try & Error"])]
	[TechniqueMetadata(
		Rating = AnalysisResult.MaximumRatingValueTheory,
		DifficultyLevel = DifficultyLevel.LastResort,
		ContainingGroup = TechniqueGroup.BruteForce,
		StepType = typeof(BruteForceStep),
		StepSearcherType = typeof(BruteForceStepSearcher),
		Abbreviation = "BF",
		Features = TechniqueFeatures.OnlyExistInTheory,
		Links = ["http://sudopedia.enjoysudoku.com/Trial_%26_Error.html"])]
	BruteForce,
	#endregion
}
