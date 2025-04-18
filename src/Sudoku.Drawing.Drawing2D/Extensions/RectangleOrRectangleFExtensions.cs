namespace System.Drawing;

/// <summary>
/// Provides extension methods on <see cref="Rectangle"/> or <see cref="RectangleF"/>.
/// </summary>
/// <seealso cref="Rectangle"/>
/// <seealso cref="RectangleF"/>
internal static class RectangleOrRectangleFExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Rectangle"/>.
	/// </summary>
	extension(Rectangle @this)
	{
		/// <summary>
		/// Zoom in or out the rectangle by the specified offset.
		/// If the offset is positive, the rectangle will be larger; otherwise, smaller.
		/// </summary>
		/// <param name="offset">The offset to zoom in or out.</param>
		/// <returns>The new rectangle.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rectangle Zoom(int offset)
			=> @this with { X = @this.X - offset, Y = @this.Y - offset, Width = @this.Width + offset * 2, Height = @this.Height + offset * 2 };

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out Point point, out Size size) => (point, size) = (new(@this.X, @this.Y), @this.Size);
	}

	/// <summary>
	/// Provides extension members on <see cref="Rectangle"/>.
	/// </summary>
	extension(Rectangle)
	{
		/// <summary>
		/// Create an instance with two points.
		/// </summary>
		/// <param name="topLeft">The top-left point.</param>
		/// <param name="bottomRight">The bottom-right point.</param>
		/// <returns>The rectangle.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Rectangle Create(Point topLeft, Point bottomRight)
		{
			var (tx, ty) = topLeft;
			var (bx, by) = bottomRight;
			return new(tx, ty, bx - tx, by - ty);
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="RectangleF"/>.
	/// </summary>
	extension(RectangleF)
	{
		/// <summary>
		/// Create an instance with two points.
		/// </summary>
		/// <param name="topLeft">The top-left point.</param>
		/// <param name="bottomRight">The bottom-right point.</param>
		/// <returns>The rectangle.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RectangleF Create(PointF topLeft, PointF bottomRight)
		{
			var (tx, ty) = topLeft;
			var (bx, by) = bottomRight;
			return new(tx, ty, bx - tx, by - ty);
		}
	}

	extension(RectangleF @this)
	{
		/// <summary>
		/// Zoom in or out the rectangle by the specified offset.
		/// If the offset is positive, the rectangle will be larger; otherwise, smaller.
		/// </summary>
		/// <param name="offset">The offset to zoom in or out.</param>
		/// <returns>The new rectangle.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RectangleF Zoom(float offset)
		{
			var result = @this;
			result.X -= offset;
			result.Y -= offset;
			result.Width += offset * 2;
			result.Height += offset * 2;
			return result;
		}

		/// <summary>
		/// Truncate the specified rectangle.
		/// </summary>
		/// <returns>The result.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rectangle Truncate() => new((int)@this.X, (int)@this.Y, (int)@this.Width, (int)@this.Height);

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out PointF point, out SizeF size) => (point, size) = (@this.Location, @this.Size);

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out float x, out float y, out float width, out float height)
			=> (x, y, width, height) = (@this.X, @this.Y, @this.Width, @this.Height);
	}
}
