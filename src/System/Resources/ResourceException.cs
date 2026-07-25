namespace System.Resources;

/// <summary>
/// Represents an exception type that relates to resource dictionary.
/// </summary>
/// <param name="assembly"><inheritdoc cref="_assembly" path="/summary"/></param>
public closed class ResourceException(Assembly? assembly) : Exception
{
	/// <summary>
	/// Indicates the target assembly.
	/// </summary>
	protected readonly Assembly? _assembly = assembly;


	/// <inheritdoc/>
	public abstract override string Message { get; }

	/// <inheritdoc/>
	public abstract override IDictionary Data { get; }
}
