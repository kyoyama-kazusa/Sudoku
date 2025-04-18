namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Graphics"/>.
/// </summary>
/// <seealso cref="Graphics"/>
internal static class GraphicsExtensions
{
	/// <summary>
	/// Draw the string representation of an instance onto the current <see cref="Graphics"/> instance.
	/// </summary>
	/// <typeparam name="TFormattable">The type of the value to draw.</typeparam>
	/// <param name="this">The graphics instance.</param>
	/// <param name="value">The value to drawing onto.</param>
	/// <param name="font">The font.</param>
	/// <param name="brush">The brush.</param>
	/// <param name="point">The point.</param>
	/// <param name="stringFormat">The string format instance.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawValue<TFormattable>(this Graphics @this, TFormattable value, Font font, Brush brush, PointF point, StringFormat stringFormat)
		where TFormattable : IFormattable, allows ref struct
		=> @this.DrawString(value.ToString(null, null), font, brush, point, stringFormat);

	/// <summary>
	/// Fills a hollow arrow.
	/// </summary>
	/// <param name="g">The graphics instance.</param>
	/// <param name="brush">The brush.</param>
	/// <param name="center">The center point.</param>
	/// <param name="length">The length.</param>
	/// <param name="width">The width.</param>
	/// <param name="height">The height.</param>
	/// <param name="angle">The angle.</param>
	/// <exception cref="ArgumentException">
	/// Throws when the argument <paramref name="length"/> is below 0 or either <paramref name="width"/> or <paramref name="height"/> is below 0.
	/// </exception>
	public static void DrawHollowArrow(this Graphics g, Brush brush, PointF center, float length, float width, float height, float angle)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(length);
		ArgumentOutOfRangeException.ThrowIfNegative(width);
		ArgumentOutOfRangeException.ThrowIfNegative(height);

		var halfWidth = width / 2;
		var totalHeight = height + length;
		var heightHalf = (height + length) / 2;
		var arrowBarWidth = (width + height) / 4;
		var (x, y) = center;
		var points = (PointF[])[
			new(x, y - (height + length) / 2),
			new(x + halfWidth, y - (height + length) / 2 + height),
			new(x + halfWidth - (width - arrowBarWidth) / 2, y - heightHalf + height),
			new(x + halfWidth - (width - arrowBarWidth) / 2, y - heightHalf + totalHeight),
			new(x - halfWidth + (width - arrowBarWidth) / 2, y - heightHalf + totalHeight),
			new(x - halfWidth + (width - arrowBarWidth) / 2, y - heightHalf + height),
			new(x - halfWidth, y - heightHalf + height)
		];

		// Rotating.
		var oldMatrix = g.Transform;
		using var newMatrix = g.Transform.Clone();
		newMatrix.RotateAt(angle, center);

		g.Transform = newMatrix;
		g.FillPolygon(brush, points);
		g.Transform = oldMatrix;
	}

	/// <summary>
	/// Draw a cross sign (<c>x</c>).
	/// </summary>
	/// <param name="this">The graphics.</param>
	/// <param name="pen">The pen.</param>
	/// <param name="rectangle">The rectangle.</param>
	/// <remarks>
	/// This method will draw a cross sign and fill with the specified color, so you don't need
	/// to find any fill methods.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCrossSign(this Graphics @this, Pen pen, RectangleF rectangle)
	{
		var (x, y, w, h) = rectangle;
		var p1 = new PointF(x, y + h);
		var p2 = new PointF(x + w, y);
		var p3 = new PointF(x, y);
		var p4 = new PointF(x + w, y + h);

		@this.DrawLine(pen, p1, p2);
		@this.DrawLine(pen, p3, p4);
	}

	/// <summary>
	/// Draw a capsule.
	/// </summary>
	/// <param name="this">The graphics.</param>
	/// <param name="pen">The pen.</param>
	/// <param name="rectangle">The rectangle.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawCapsule(this Graphics @this, Pen pen, RectangleF rectangle) => @this.DrawRoundedRectangle(pen, rectangle, 0);

	/// <summary>
	/// Fill a capsule.
	/// </summary>
	/// <param name="this">The graphics.</param>
	/// <param name="brush">The brush.</param>
	/// <param name="rectangle">The rectangle.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FillCapsule(this Graphics @this, Brush brush, RectangleF rectangle) => @this.FillRoundedRectangle(brush, rectangle, 0);

	/// <summary>
	/// Draw a rounded rectangle.
	/// </summary>
	/// <param name="this">The graphics.</param>
	/// <param name="pen">The pen.</param>
	/// <param name="rectangle">The rectangle to draw.</param>
	/// <param name="circleRadius">The radius of each vertex.</param>
	/// <exception cref="ArgumentException">
	/// Throws when <paramref name="circleRadius"/> is greater than the value in
	/// <paramref name="rectangle"/>.
	/// </exception>
	public static void DrawRoundedRectangle(this Graphics @this, Pen pen, RectangleF rectangle, float circleRadius)
	{
		var (x, y, w, h) = rectangle;
		var (l, _) = rectangle;

		ArgumentException.ThrowIfAssertionFailed(circleRadius < Max(w, h));

		PointF p1, p2, p3, p4, p5, p6, p7, p8;
		p8 = p7 = p6 = p5 = p4 = p3 = p2 = p1 = l;
		p1.X += circleRadius;
		p2.X += w - circleRadius;
		p3.Y += circleRadius;
		p4.X += w;
		p4.Y += circleRadius;
		p5.Y += h - circleRadius;
		p6.X += w;
		p6.Y += h - circleRadius;
		p7.X += circleRadius;
		p7.Y += h;
		p8.X += w - circleRadius;
		p8.Y += h;

		RectangleF r1, r2, r3, r4;
		r1 = new(x, y, circleRadius * 2, circleRadius * 2);
		r2 = new(x + w - 2 * circleRadius, y, circleRadius * 2, circleRadius * 2);
		r3 = new(x, y + h - 2 * circleRadius, circleRadius * 2, circleRadius * 2);
		r4 = new(x + w - 2 * circleRadius, y + h - 2 * circleRadius, circleRadius * 2, circleRadius * 2);

		using var path = new GraphicsPath();
		path.AddLine(p1, p2);
		path.AddArc(r2, 270, 90);
		path.AddLine(p4, p6);
		path.AddArc(r4, 0, 90);
		path.AddLine(p7, p8);
		path.AddArc(r3, 90, 90);
		path.AddLine(p5, p3);
		path.AddArc(r1, 180, 90);
		path.CloseFigure();

		@this.DrawPath(pen, path);
	}

	/// <summary>
	/// Fill a rounded rectangle.
	/// </summary>
	/// <param name="this">The graphics.</param>
	/// <param name="brush">The brush.</param>
	/// <param name="rectangle">The rectangle to fill.</param>
	/// <param name="circleRadius">The radius of each vertex.</param>
	/// <exception cref="ArgumentException">
	/// Throws when <paramref name="circleRadius"/> is greater than the value in
	/// <paramref name="rectangle"/>.
	/// </exception>
	public static void FillRoundedRectangle(this Graphics @this, Brush brush, RectangleF rectangle, float circleRadius)
	{
		var (x, y, w, h) = rectangle;
		var (l, _) = rectangle;

		ArgumentException.ThrowIfAssertionFailed(circleRadius < Max(w, h));

		PointF p1, p2, p3, p4, p5, p6, p7, p8;
		p8 = p7 = p6 = p5 = p4 = p3 = p2 = p1 = l;
		p1.X += circleRadius;
		p2.X += w - circleRadius;
		p3.Y += circleRadius;
		p4.X += w;
		p4.Y += circleRadius;
		p5.Y += h - circleRadius;
		p6.X += w;
		p6.Y += h - circleRadius;
		p7.X += circleRadius;
		p7.Y += h;
		p8.X += w - circleRadius;
		p8.Y += h;

		RectangleF r1, r2, r3, r4;
		r1 = new(x, y, circleRadius * 2, circleRadius * 2);
		r2 = new(x + w - 2 * circleRadius, y, circleRadius * 2, circleRadius * 2);
		r3 = new(x, y + h - 2 * circleRadius, circleRadius * 2, circleRadius * 2);
		r4 = new(x + w - 2 * circleRadius, y + h - 2 * circleRadius, circleRadius * 2, circleRadius * 2);

		using var path = new GraphicsPath();
		path.AddLine(p1, p2);
		path.AddArc(r2, 270, 90);
		path.AddLine(p4, p6);
		path.AddArc(r4, 0, 90);
		path.AddLine(p7, p8);
		path.AddArc(r3, 90, 90);
		path.AddLine(p5, p3);
		path.AddArc(r1, 180, 90);
		path.CloseFigure();

		@this.FillPath(brush, path);
	}
}
