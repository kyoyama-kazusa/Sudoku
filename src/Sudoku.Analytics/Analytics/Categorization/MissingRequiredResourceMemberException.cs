namespace Sudoku.Analytics.Categorization;

/// <summary>
/// Represents an exception type that describes an error when a resource is missing.
/// </summary>
/// <param name="_resourceKey">The resource key.</param>
/// <param name="_memberName">The member name.</param>
public abstract class MissingRequiredResourceMemberException(string _resourceKey, string _memberName) : Exception
{
	/// <inheritdoc/>
	public sealed override string Message => string.Format(SR.Get(_resourceKey), _memberName);
}
