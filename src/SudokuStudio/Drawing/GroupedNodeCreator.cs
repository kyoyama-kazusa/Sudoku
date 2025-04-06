namespace SudokuStudio.Drawing;

/// <summary>
/// Extracted type that creates the capsule instances.
/// </summary>
/// <param name="pane">Indicates the sudoku pane control.</param>
/// <param name="converter">Indicates the position converter.</param>
#if true
internal sealed class GroupedNodeCreator(SudokuPane pane, SudokuPanePositionConverter converter) :
	CreatorBase<GroupedNodeInfo, Rectangle>(pane, converter)
{
	/// <inheritdoc/>
	public override ReadOnlySpan<Rectangle> CreateShapes(ReadOnlySpan<GroupedNodeInfo> nodes)
	{
		// Iterate on each inference to draw the links and grouped nodes (if so).
		var ((ow, _), _) = Converter;
		var drawnGroupedNodes = new List<CandidateMap>();
		var result = new List<Rectangle>();
		foreach (var n in nodes)
		{
			// If the start node or end node is a grouped node, we should append a rectangle to highlight it.
			var node = n.Map;
			if (node.Count != 1 && !drawnGroupedNodes.Contains(node))
			{
				drawnGroupedNodes.AddRef(node);
				result.Add(drawRectangle(n, node));
			}
		}
		return result.AsSpan();


		Rectangle drawRectangle(GroupedNodeInfo n, in CandidateMap nodeCandidates)
		{
			var fill = new SolidColorBrush(Pane.GroupedNodeBackgroundColor);
			var stroke = new SolidColorBrush(Pane.GroupedNodeStrokeColor);
			var result = new Rectangle
			{
				Stroke = stroke,
				StrokeThickness = 1.5,
				Fill = fill,
				RadiusX = 10,
				RadiusY = 10,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Tag = n,
				Opacity = Pane.EnableAnimationFeedback ? 0 : 1
			};

			// Try to arrange rectangle position.
			// A simple way is to record all rows and columns spanned for the candidate list,
			// in order to find four data:
			//   1) The minimal row
			//   2) The maximal row
			//   3) The minimal column
			//   4) The maximal column
			// and then find a minimal rectangle that can cover all of those candidates by those four data.
			const int logicalMaxValue = 100;
			var (minRow, minColumn, maxRow, maxColumn) = (Candidate.MaxValue, Candidate.MaxValue, Candidate.MinValue, Candidate.MinValue);
			var (minRowValue, minColumnValue, maxRowValue, maxColumnValue) = (logicalMaxValue, logicalMaxValue, -1, -1);
			foreach (var candidate in nodeCandidates)
			{
				var cell = candidate / 9;
				var digit = candidate % 9;
				var rowValue = cell / 9 * 3 + digit / 3;
				var columnValue = cell % 9 * 3 + digit % 3;
				if (rowValue <= minRowValue)
				{
					(minRowValue, minRow) = (rowValue, candidate);
				}
				if (rowValue >= maxRowValue)
				{
					(maxRowValue, maxRow) = (rowValue, candidate);
				}
				if (columnValue <= minColumnValue)
				{
					(minColumnValue, minColumn) = (columnValue, candidate);
				}
				if (columnValue >= maxColumnValue)
				{
					(maxColumnValue, maxColumn) = (columnValue, candidate);
				}
			}

			var topLeftY = Converter.GetPosition(minRow, Position.TopLeft).Y;
			var topLeftX = Converter.GetPosition(minColumn, Position.TopLeft).X;
			var bottomRightY = Converter.GetPosition(maxRow, Position.BottomRight).Y;
			var bottomRightX = Converter.GetPosition(maxColumn, Position.BottomRight).X;
			var rectanglePositionTopLeft = new Thickness(topLeftX - ow, topLeftY - ow, 0, 0);
			(result.Width, result.Height, result.Margin) = (bottomRightX - topLeftX, bottomRightY - topLeftY, rectanglePositionTopLeft);
			return result;
		}
	}
}
#else
internal sealed class GroupedNodeCreator(SudokuPane pane, SudokuPanePositionConverter converter) :
	CreatorBase<GroupedNodeInfo, Line>(pane, converter)
{
	/// <inheritdoc/>
	public override ReadOnlySpan<Line> CreateShapes(ReadOnlySpan<GroupedNodeInfo> nodes)
	{
		// Iterate on each inference to draw the links and grouped nodes (if so).
		var ((ow, _), _) = Converter;
		var drawnGroupedNodes = new HashSet<CandidateMap>();
		var result = new List<Line>();
		foreach (var n in nodes)
		{
			// If the start node or end node is a grouped node, we should append a rectangle to highlight it.
			var node = n.Map;
			if (node.Count != 1 && drawnGroupedNodes.Add(node))
			{
				result.AddRange(drawLines(n, node));
			}
		}
		return result.AsSpan();


		ReadOnlySpan<Line> drawLines(GroupedNodeInfo n, in CandidateMap nodeCandidates)
		{
			const double radius = 10D;
			var fill = new SolidColorBrush(Pane.GroupedNodeBackgroundColor);
			var stroke = new SolidColorBrush(Pane.GroupedNodeStrokeColor);
			var points = (from candidate in nodeCandidates select Converter.GetPosition(candidate)).ToArray();
			var result = new List<Line>();
			foreach (var ((x1, y1), (x2, y2)) in ConvexHullHelper.GetOuterTangentPoints(points, radius))
			{
				result.Add(
					new()
					{
						X1 = x1 - ow,
						Y1 = y1 - ow,
						X2 = x2 - ow,
						Y2 = y2 - ow,
						Stroke = stroke,
						StrokeThickness = 1.5,
						Tag = n,
						Opacity = Pane.EnableAnimationFeedback ? 0 : 1
					}
				);
			}
			return result.AsSpan();
		}
	}
}

file sealed class ConvexHullHelper
{
	private static double Cross(Point a, Point b, Point c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

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

	public static IEnumerable<(Point First, Point Second)> GetOuterTangentPoints(Point[] centers, double radius)
	{
		var convexHull = getConvexHull(centers);
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


		static Point[] getConvexHull(Point[] points)
		{
			if (points.Length <= 1)
			{
				return points;
			}

			var targetPoints = from p in points orderby p.X, p.Y select p;
			var result = new List<Point>();

			// Lower hull.
			foreach (ref readonly var pt in targetPoints)
			{
				while (result.Count >= 2 && Cross(result[^2], result[^1], pt) <= 0)
				{
					result.RemoveAt(^1);
				}
				result.Add(pt);
			}

			// Upper hull.
			var t = result.Count + 1;
			for (var i = targetPoints.Length - 2; i >= 0; i--)
			{
				ref readonly var pt = ref targetPoints[i];
				while (result.Count >= t && Cross(result[^2], result[^1], pt) <= 0)
				{
					result.RemoveAt(^1);
				}
				result.Add(pt);
			}

			result.RemoveAt(^1);
			return [.. result];
		}
	}
}
#endif
