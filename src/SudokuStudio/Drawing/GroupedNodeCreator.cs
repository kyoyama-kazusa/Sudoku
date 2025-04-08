namespace SudokuStudio.Drawing;

/// <summary>
/// Extracted type that creates the instances of grouped node outside borders.
/// </summary>
/// <param name="pane">Indicates the sudoku pane control.</param>
/// <param name="converter">Indicates the position converter.</param>
internal sealed class GroupedNodeCreator(SudokuPane pane, SudokuPanePositionConverter converter) :
	CreatorBase<GroupedNodeInfo, Path>(pane, converter)
{
	/// <inheritdoc/>
	public override ReadOnlySpan<Path> CreateShapes(ReadOnlySpan<GroupedNodeInfo> nodes)
	{
		// Iterate on each inference to draw the links and grouped nodes (if so).
		var ((ow, _), _) = Converter;
		var drawnGroupedNodes = new HashSet<CandidateMap>();
		var result = new List<Path>();
		var fill = new SolidColorBrush(Pane.GroupedNodeBackgroundColor);
		var stroke = new SolidColorBrush(Pane.GroupedNodeStrokeColor);
		foreach (var n in nodes)
		{
			// If the start node or end node is a grouped node, we should append a rectangle to highlight it.
			var node = n.Map;
			if (node.Count != 1 && drawnGroupedNodes.Add(node))
			{
				result.Add(
					new()
					{
						Data = ConvexHullHelper.BuildClosedPath(
							[.. from candidate in node select Converter.GetPosition(candidate)],
							Converter.CandidateSize.Width / 2,
							ow
						),
						Stroke = stroke,
						StrokeThickness = 1.5,
						Fill = fill,
						Tag = n,
						Opacity = Pane.EnableAnimationFeedback ? 0 : 1
					}
				);
			}
		}
		return result.AsSpan();
	}
}

/// <summary>
/// Provides an internal type that calculates for convex hull.
/// </summary>
/// <remarks>
/// A <see href="https://en.wikipedia.org/wiki/Convex_hull">Convex Hull</see> is a minimal polygon that covers all specified points.
/// </remarks>
file sealed class ConvexHullHelper
{
	/// <summary>
	/// Calculates for the cross product of points <paramref name="a"/> and <paramref name="b"/>.
	/// </summary>
	/// <param name="a">The first point.</param>
	/// <param name="b">The second point.</param>
	/// <param name="c">Used for auxiliary value as subtraction.</param>
	/// <returns>The result.</returns>
	private static double Cross(Point a, Point b, Point c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

	/// <summary>
	/// Determine whether the polygon specified as point set is clockwise.
	/// </summary>
	/// <param name="polygon">The polygon points to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private static bool IsClockwise(ReadOnlySpan<Point> polygon)
	{
		var area = 0D;
		for (var i = 0; i < polygon.Length; i++)
		{
			var j = (i + 1) % polygon.Length;
			area += polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;
		}
		return area < 0;
	}

	/// <summary>
	/// Try to build a <see cref="PathGeometry"/> instance that makes a closed path of polygon.
	/// </summary>
	/// <param name="centers">The center points.</param>
	/// <param name="radius">The radius of each vertex.</param>
	/// <param name="ow">The offset.</param>
	/// <returns>The instance.</returns>
	public static PathGeometry BuildClosedPath(Point[] centers, double radius, double ow)
	{
		var convexHull = GetConvexHull(centers);
		var isClockwise = IsClockwise(convexHull);
		var segments = GetOuterTangentPoints(centers, radius).ToArray();
		var pathGeometry = new PathGeometry();
		var pathFigure = new PathFigure { IsClosed = true };
		if (segments.Length == 0)
		{
			return pathGeometry;
		}

		var offsetPoint = new Point(ow, ow);
		pathFigure.StartPoint = Subtract(segments[0].First, offsetPoint);
		for (var i = 0; i < segments.Length; i++)
		{
			// Add straight line segments.
			var (_, segmentSecond) = segments[i];
			segmentSecond = Subtract(segmentSecond, offsetPoint);
			pathFigure.Segments.Add(new LineSegment { Point = segmentSecond });

			// Calculate for arc-related parameters.
			var nextIndex = (i + 1) % segments.Length;
			var nextStart = segments[nextIndex].First;
			var currentEnd = segmentSecond;
			var center = convexHull[(i + 1) % convexHull.Length];

			// Calculate for directions of arcs.
			var toCurrentEnd = Subtract(currentEnd, center);
			var toNextStart = Subtract(nextStart, center);
			var angleCurrent = Atan(toCurrentEnd.Y / toCurrentEnd.X) * 180 / PI;
			var angleNext = Atan(toNextStart.Y / toNextStart.X) * 180 / PI;

			// Determine the sweep direction and whether it is a large arc.
			var rotateAngle = Abs(angleNext - angleCurrent);
			var isLargeArc = rotateAngle > 180;
			var sweepDirection = !isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

			// Add the arc.
			var arcSegment = new ArcSegment
			{
				Point = Subtract(nextStart, offsetPoint),
				Size = new(radius, radius),
				SweepDirection = sweepDirection,
				IsLargeArc = isLargeArc
			};
			pathFigure.Segments.Add(arcSegment);
		}

		pathGeometry.Figures.Add(pathFigure);
		return pathGeometry;
	}

	/// <summary>
	/// Creates a sequence of pairs of <see cref="Point"/> instances indicating lines start and end point.
	/// </summary>
	/// <param name="centers">The center points.</param>
	/// <param name="radius">The radius.</param>
	/// <returns>The sequence.</returns>
	public static IEnumerable<(Point First, Point Second)> GetOuterTangentPoints(Point[] centers, double radius)
	{
		var convexHull = GetConvexHull(centers);
		if (convexHull.Length < 2)
		{
			yield break;
		}

		var isClockwise = IsClockwise(convexHull);
		for (var i = 0; i < convexHull.Length; i++)
		{
			var a = convexHull[i];
			var b = convexHull[(i + 1) % convexHull.Length];
			var dx = b.X - a.X;
			var dy = b.Y - a.Y;
			var (normalX, normalY) = isClockwise ? (-dy, dx) : (dy, -dx);
			var length = Sqrt(dx * dx + dy * dy);
			if (length.NearlyEquals(0, 1E-2))
			{
				continue;
			}

			var unitNormalX = normalX / length;
			var unitNormalY = normalY / length;
			yield return (
				new(a.X + unitNormalX * radius, a.Y + unitNormalY * radius),
				new(b.X + unitNormalX * radius, b.Y + unitNormalY * radius)
			);
		}
	}

	/// <summary>
	/// Try to get convex hull points.
	/// </summary>
	/// <param name="points">The points.</param>
	/// <returns>A list of <see cref="Point"/> instances indicating the vertex of a convex hull.</returns>
	private static Point[] GetConvexHull(Point[] points)
	{
		if (points.Length <= 1)
		{
			return points;
		}

		var result = new List<Point>();

		// Lower hull.
		foreach (ref readonly var pt in from p in points orderby p.X ascending, p.Y ascending select p)
		{
			while (result.Count >= 2 && Cross(result[^2], result[^1], pt) <= 0)
			{
				result.RemoveAt(^1);
			}
			result.Add(pt);
		}

		// Upper hull.
		var t = result.Count + 1;
		foreach (ref readonly var pt in from p in points orderby p.X descending, p.Y descending select p)
		{
			while (result.Count >= t && Cross(result[^2], result[^1], pt) <= 0)
			{
				result.RemoveAt(^1);
			}
			result.Add(pt);
		}

		result.RemoveAt(^1);
		return [.. result];
	}

	/// <summary>
	/// Make subtraction of two <see cref="Point"/> values.
	/// </summary>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <returns>A <see cref="Point"/> instance as result.</returns>
	private static Point Subtract(Point left, Point right) => new(left.X - right.X, left.Y - right.Y);
}
