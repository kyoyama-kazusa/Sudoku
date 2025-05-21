namespace SudokuStudio.Drawing;

/// <summary>
/// Extracted type that creates the <see cref="Path"/> instances.
/// </summary>
/// <param name="pane">Indicates the sudoku pane control.</param>
/// <param name="converter">Indicates the position converter.</param>
/// <param name="candidateNodes"><inheritdoc cref="CandidateNodes" path="/summary"/></param>
/// <param name="conclusions"><inheritdoc cref="Conclusions" path="/summary"/></param>
/// <seealso cref="Path"/>
internal sealed class PathCreator(
	SudokuPane pane,
	SudokuPanePositionConverter converter,
	ReadOnlyMemory<CandidateViewNode> candidateNodes,
	ReadOnlyMemory<Conclusion> conclusions
) : CreatorBase<ILinkViewNode, Shape>(pane, converter)
{
	/// <summary>
	/// Indicates the conclusions of the whole chain.
	/// </summary>
	public ReadOnlyMemory<Conclusion> Conclusions { get; } = conclusions;

	/// <summary>
	/// Indicates the candidate view nodes.
	/// </summary>
	public ReadOnlyMemory<CandidateViewNode> CandidateNodes { get; } = candidateNodes;


	/// <inheritdoc/>
	public override ReadOnlySpan<Shape> CreateShapes(ReadOnlySpan<ILinkViewNode> nodes)
	{
		// Iterate on each inference to draw the links and grouped nodes (if so).
		var ((ow, oh), _) = Converter;
		var ((cellSize, _), _, _, _) = Converter;
		var result = new List<Shape>();
		foreach (var node in nodes)
		{
			var (_, start, end) = node;
			var dashArray = node switch
			{
				ChainLinkViewNode { IsStrongLink: var i } => [.. i ? Pane.StrongLinkDashStyle : Pane.WeakLinkDashStyle],
				_ => default(DoubleCollection)
			};

			// Find two candidates with a minimal distance.
			var (distance, pt1, pt2) = (double.MaxValue, default(Point), default(Point));
			switch (node.Shape)
			{
				case LinkShape.Chain or LinkShape.ConjugatePair:
				{
					var startCandidates = start switch { CandidateMap c => c, Candidate c => c.AsCandidateMap() };
					var endCandidates = end switch { CandidateMap c => c, Candidate c => c.AsCandidateMap() };
					foreach (var s in startCandidates)
					{
						var tempPoint1 = Converter.GetPosition(s);
						foreach (var e in endCandidates)
						{
							var tempPoint2 = Converter.GetPosition(e);
							var d = tempPoint1.DistanceTo(tempPoint2);
							if (d <= distance)
							{
								(distance, pt1, pt2) = (d, tempPoint1, tempPoint2);
							}
						}
					}
					break;
				}
				case LinkShape.Cell:
				{
					pt1 = Converter.GetPosition((Cell)start * 9 + 4);
					pt2 = Converter.GetPosition((Cell)end * 9 + 4);
					distance = pt1.DistanceTo(pt2);
					break;
				}
			}

			// If the distance of two points is lower than the one of two adjacent candidates,
			// the link will be ignored to be drawn because of too narrow.
			var ((pt1x, pt1y), (pt2x, pt2y)) = (pt1, pt2);
			if (distance <= cellSize * SqrtOf2 || distance <= cellSize * SqrtOf2)
			{
				continue;
			}

			var (deltaX, deltaY) = (pt2.X - pt1.X, pt2.Y - pt1.Y);
			var alpha = Atan2(deltaY, deltaX);
			var linkPassesThroughUsedCandidates = (node as ChainLinkViewNode)?.IsPassedThrough(nodes, CandidateNodes.Span, Conclusions.Span, Pane.MainGrid) ?? false;

			// Now cut the link.
			cut(ref pt1, ref pt2, cellSize);

			// Check whether the link will pass through a candidate used in pattern.
			if (linkPassesThroughUsedCandidates)
			{
				// The end points are rotated 45 degrees (counterclockwise for the start point, clockwise for the end point).
				const double bezierLength = 30.0;
				var oldPt1 = new Point(pt1x, pt1y);
				var oldPt2 = new Point(pt2x, pt2y);
				rotate(oldPt1, ref pt1, -RotateAngle);
				rotate(oldPt2, ref pt2, RotateAngle);

				var interim1Alpha = alpha - RotateAngle;
				var bx1 = pt1.X + bezierLength * Cos(interim1Alpha);
				var by1 = pt1.Y + bezierLength * Sin(interim1Alpha);
				var interim2Alpha = alpha + RotateAngle;
				var bx2 = pt2.X - bezierLength * Cos(interim2Alpha);
				var by2 = pt2.Y - bezierLength * Sin(interim2Alpha);
				correctOffsetOfPoint(ref pt1, ow, oh);
				correctOffsetOfPoint(ref pt2, ow, oh);
				correctOffsetOfDouble(ref bx1, ow);
				correctOffsetOfDouble(ref bx2, oh);
				correctOffsetOfDouble(ref by1, ow);
				correctOffsetOfDouble(ref by2, oh);
				result.Add(
					new Path
					{
						Stroke = new SolidColorBrush(Pane.LinkColor),
						StrokeThickness = (double)Pane.ChainStrokeThickness,
						StrokeDashArray = dashArray,
						Data = new GeometryGroup
						{
							Children = [
								new PathGeometry
								{
									Figures = [
										new PathFigure
										{
											StartPoint = pt1,
											IsClosed = false,
											IsFilled = false,
											Segments = [new BezierSegment { Point1 = new(bx1, by1), Point2 = new(bx2, by2), Point3 = pt2 }]
										}
									]
								}
							]
						},
						Tag = node,
						Opacity = Pane.EnableAnimationFeedback ? 0 : 1
					}
				);
				result.Add(
					new Path
					{
						Stroke = new SolidColorBrush(Pane.LinkColor),
						StrokeThickness = (double)Pane.ChainStrokeThickness,
						Data = new GeometryGroup { Children = ArrowCap(new(bx2, by2), pt2) },
						Tag = node
					}
				);
			}
			else
			{
				// Draw the link.
				correctOffsetOfPoint(ref pt1, ow, oh);
				correctOffsetOfPoint(ref pt2, ow, oh);
				result.Add(
					new Path
					{
						Stroke = new SolidColorBrush(Pane.LinkColor),
						StrokeThickness = (double)Pane.ChainStrokeThickness * (node.Shape == LinkShape.ConjugatePair ? 2 : 1),
						StrokeDashArray = dashArray,
						Data = new GeometryGroup { Children = [new LineGeometry { StartPoint = pt1, EndPoint = pt2 }] },
						Tag = node,
						Opacity = Pane.EnableAnimationFeedback ? 0 : 1
					}
				);

				// Append arrow cap.
				if (node.Shape == LinkShape.Chain)
				{
					result.Add(
						new Path
						{
							Stroke = new SolidColorBrush(Pane.LinkColor),
							StrokeThickness = (double)Pane.ChainStrokeThickness,
							Data = new GeometryGroup { Children = ArrowCap(pt1, pt2) },
							Tag = node,
							Opacity = Pane.EnableAnimationFeedback ? 0 : 1
						}
					);
				}
			}


			void cut(ref Point pt1, ref Point pt2, double cs)
			{
				if (node.Shape is LinkShape.Chain or LinkShape.ConjugatePair)
				{
					var ((pt1x, pt1y), (pt2x, pt2y)) = (pt1, pt2);
					var slope = Abs((pt2y - pt1y) / (pt2x - pt1x));
					var (x, y) = (cs / Sqrt(1 + slope * slope), cs * Sqrt(slope * slope / (1 + slope * slope)));
					if (pt1y > pt2y && pt1x.NearlyEquals(pt2x))
					{
						pt1.Y -= cs / 2;
						pt2.Y += cs / 2;
					}
					else if (pt1y < pt2y && pt1x.NearlyEquals(pt2x))
					{
						pt1.Y += cs / 2;
						pt2.Y -= cs / 2;
					}
					else if (pt1y.NearlyEquals(pt2y) && pt1x > pt2x)
					{
						pt1.X -= cs / 2;
						pt2.X += cs / 2;
					}
					else if (pt1y.NearlyEquals(pt2y) && pt1x < pt2x)
					{
						pt1.X += cs / 2;
						pt2.X -= cs / 2;
					}
					else if (pt1y > pt2y && pt1x > pt2x)
					{
						pt1.X -= x / 2;
						pt1.Y -= y / 2;
						pt2.X += x / 2;
						pt2.Y += y / 2;
					}
					else if (pt1y > pt2y && pt1x < pt2x)
					{
						pt1.X += x / 2;
						pt1.Y -= y / 2;
						pt2.X -= x / 2;
						pt2.Y += y / 2;
					}
					else if (pt1y < pt2y && pt1x > pt2x)
					{
						pt1.X -= x / 2;
						pt1.Y += y / 2;
						pt2.X += x / 2;
						pt2.Y -= y / 2;
					}
					else if (pt1y < pt2y && pt1x < pt2x)
					{
						pt1.X += x / 2;
						pt1.Y += y / 2;
						pt2.X -= x / 2;
						pt2.Y -= y / 2;
					}
				}
			}
		}
		return result.AsSpan();


		static void rotate(Point pt1, ref Point pt2, double angle)
		{
			// Translate 'pt2' to (0, 0).
			pt2.X -= pt1.X;
			pt2.Y -= pt1.Y;

			// Rotate.
			var (sinAngle, cosAngle, (xAct, yAct)) = (Sin(angle), Cos(angle), pt2);
			pt2.X = xAct * cosAngle - yAct * sinAngle;
			pt2.Y = xAct * sinAngle + yAct * cosAngle;
			pt2.X += pt1.X;
			pt2.Y += pt1.Y;
		}

		static void correctOffsetOfPoint(ref Point point, double ow, double oh)
		{
			// We should correct the offset because canvas storing link view nodes are not aligned as the sudoku pane.
			point.X -= ow;
			point.Y -= oh;
		}

		static void correctOffsetOfDouble(ref double value, double offset)
			// We should correct the offset because canvas storing link view nodes are not aligned as the sudoku pane.
			=> value -= offset;
	}


	/// <summary>
	/// Creates a list of <see cref="Geometry"/> instances via two <see cref="Point"/>s indicating start and end point respectively,
	/// meaning the arrow cap lines besides the line.
	/// </summary>
	/// <param name="pt1">The start point.</param>
	/// <param name="pt2">The end point.</param>
	/// <returns>An instance of type <see cref="IEnumerable{T}"/> of <see cref="Geometry"/>.</returns>
	private static GeometryCollection ArrowCap(Point pt1, Point pt2)
	{
		var arrowLength = 10.0;
		var theta = 30.0;
		var angle = Atan2(pt1.Y - pt2.Y, pt1.X - pt2.X) * 180 / PI;
		var angle1 = (angle + theta) * PI / 180;
		var angle2 = (angle - theta) * PI / 180;
		var topX = arrowLength * Cos(angle1);
		var topY = arrowLength * Sin(angle1);
		var bottomX = arrowLength * Cos(angle2);
		var bottomY = arrowLength * Sin(angle2);
		return [
			new LineGeometry { StartPoint = new(pt2.X + topX, pt2.Y + topY), EndPoint = pt2 },
			new LineGeometry { StartPoint = new(pt2.X + bottomX, pt2.Y + bottomY), EndPoint = pt2 }
		];
	}
}
