namespace Sudoku.Analytics.Categorization;

/// <summary>
/// Represents an exception thrown when a field in <see cref="Technique"/> is missing for technique group attribute.
/// </summary>
/// <param name="memberName">Indicates the field name.</param>
/// <seealso cref="Technique"/>
/// <seealso cref="TechniqueGroup"/>
/// <seealso cref="TechniqueMetadataAttribute"/>
public sealed class MissingTechniqueGroupException(string memberName) :
	MissingRequiredResourceMemberException("Message_MissingTechniqueGroupException", memberName);
