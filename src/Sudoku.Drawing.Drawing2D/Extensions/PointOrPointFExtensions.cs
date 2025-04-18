namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Point"/> or <see cref="PointF"/>.
/// </summary>
/// <seealso cref="Point"/>
/// <seealso cref="PointF"/>
internal static class PointOrPointFExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Point"/>.
	/// </summary>
	extension(Point @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out int x, out int y) => (x, y) = (@this.X, @this.Y);
	}

	/// <summary>
	/// Provides extension members on <see cref="PointF"/>.
	/// </summary>
	extension(PointF @this)
	{
		/// <summary>
		/// To truncate the point.
		/// </summary>
		/// <returns>The result.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Point Truncate() => new((int)@this.X, (int)@this.Y);

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out float x, out float y) => (x, y) = (@this.X, @this.Y);
	}
}
