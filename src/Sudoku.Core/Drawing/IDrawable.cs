namespace Sudoku.Drawing;

/// <summary>
/// Represents a drawable instance that can be used for drawing, providing with base data structure to be used by drawing APIs.
/// </summary>
public interface IDrawable
{
	/// <summary>
	/// Indicates the conclusions that a step produces,
	/// meaning the specified candidates in this property can be safely set or deleted.
	/// </summary>
	public abstract ReadOnlyMemory<Conclusion> Conclusions { get; }

	/// <summary>
	/// Indicates the view collection that represents <see cref="ViewNode"/> data, used by rendering APIs.
	/// </summary>
	public abstract ReadOnlyMemory<View> Views { get; }
}
