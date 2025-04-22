namespace SudokuStudio.Drawing;

internal partial class DrawableFactory
{
	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for conclusions.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="conclusion">The conclusion to be displayed.</param>
	/// <param name="overlapped">A collection that stores for overlapped candidates.</param>
	private static partial void ForConclusion(DrawingContext context, Conclusion conclusion, List<Conclusion> overlapped)
	{
		var (sudokuPane, animatedResults) = context;
		var (type, candidate) = conclusion;
		if (sudokuPane._children[candidate / 9] is not { } paneCellControl)
		{
			return;
		}

		var isOverlapped = overlapped.Exists(conclusion => conclusion.Candidate == candidate);
		var id = (type, isOverlapped) switch
		{
			(Assignment, true) => ColorIdentifierKind.OverlappedAssignment,
			(Assignment, _) => ColorIdentifierKind.Assignment,
			(Elimination, true) => ColorIdentifierKind.Cannibalism,
			_ => ColorIdentifierKind.Elimination
		};
		ForCandidateNodeCore(
			id,
			IdentifierConversion.GetColor(id),
			candidate,
			null,
			paneCellControl,
			animatedResults,
			true,
			conclusion.ConclusionType == Elimination,
			isOverlapped
		);
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="CellViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="cellNode">The node to be displayed.</param>
	/// <seealso cref="CellViewNode"/>
	private static partial void ForCellNode(DrawingContext context, CellViewNode cellNode)
	{
		var (sudokuPane, animatedResults) = context;
		var (id, cell) = cellNode;
		if (sudokuPane._children[cell] is not { } paneCellControl)
		{
			return;
		}

		var control = new Border
		{
			BorderThickness = new(0),
			Tag = cellNode,
			Opacity = 0,
			Background = new SolidColorBrush(IdentifierConversion.GetColor(id)),
			CornerRadius = sudokuPane.CellsInnerCornerRadius,
			Margin = sudokuPane.CellsInnerPadding
		};

		GridLayout.SetRowSpan(control, 3);
		GridLayout.SetColumnSpan(control, 3);
		Canvas.SetZIndex(control, -2);

		if (sudokuPane.EnableAnimationFeedback)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add(
			(
				() => paneCellControl.MainGrid.Children.Add(control),
				() => control.Opacity = (double)sudokuPane.HighlightBackgroundOpacity
			)
		);
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="IconViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="iconNode">The node to be displayed.</param>
	/// <seealso cref="IconViewNode"/>
	private static partial void ForIconNode(DrawingContext context, IconViewNode iconNode)
	{
		var (sudokuPane, animatedResults) = context;
		var id = iconNode.Identifier;
		var cell = iconNode.Cell;
		if (sudokuPane._children[cell] is not { } paneCellControl)
		{
			return;
		}

		var control = create(
			iconNode switch
			{
				CircleViewNode => static () => new CircleRing(),
				CrossViewNode => static () => new Cross(),
				TriangleViewNode => static () => new Triangle(),
				DiamondViewNode => static () => new Diamond()
			}
		);

		GridLayout.SetRowSpan(control, 3);
		GridLayout.SetColumnSpan(control, 3);
		Canvas.SetZIndex(control, -2);

		if (sudokuPane.EnableAnimationFeedback)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add((() => paneCellControl.MainGrid.Children.Add(control), () => control.Opacity = 1));


		Control create(Func<Control> instanceCreator)
		{
			var result = instanceCreator();
			result.BorderThickness = new(0);
			result.Tag = iconNode;
			result.Background = new SolidColorBrush(IdentifierConversion.GetColor(id));
			result.Opacity = 0;
			result.Margin = result switch { Star or Triangle or Diamond => new(3, 0, 0, 0), _ => new(6) };
			return result;
		}
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="CandidateViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="candidateNode">Indicates the candidate view nodes.</param>
	/// <param name="conclusions">Indicates the conclusion collection. The argument is used for checking cannibalism.</param>
	/// <param name="overlapped">
	/// Indicates the collection that returns a possible <see cref="Conclusion"/> value indicating
	/// what candidate conflicts with the current node while displaying. If no overlapped conclusion, <see langword="null"/>.
	/// </param>
	/// <seealso cref="CandidateViewNode"/>
	private static partial void ForCandidateNode(DrawingContext context, CandidateViewNode candidateNode, ReadOnlyMemory<Conclusion> conclusions, out Conclusion? overlapped)
	{
		var (sudokuPane, animatedResults) = context;
		(overlapped, var (id, candidate)) = (null, candidateNode);
		var cell = candidate / 9;
		if (sudokuPane._children[cell] is not { } paneCellControl)
		{
			return;
		}

		if (conclusions.ConflictWith(candidate, out var conclusionOverlapped))
		{
			// This will be rendered as cannibalism or assignment overlapping cases. We may not handle on this here.
			overlapped = conclusionOverlapped;
			return;
		}

		ForCandidateNodeCore(id, IdentifierConversion.GetColor(id), candidate, candidateNode, paneCellControl, animatedResults);
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="HouseViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="houseNode">Indicates the house view nodes.</param>
	/// <exception cref="ArgumentException">
	/// Throws when the argument <paramref name="houseNode"/> stores invalid data of property <see cref="HouseViewNode.House"/>.
	/// </exception>
	/// <seealso cref="HouseViewNode"/>
	private static partial void ForHouseNode(DrawingContext context, HouseViewNode houseNode)
	{
		var (sudokuPane, animatedResults) = context;
		var (id, house) = houseNode;
		if (sudokuPane.MainGrid is not { } gridControl)
		{
			return;
		}

		var (row, column, rowSpan, columnSpan) = house switch
		{
			>= 0 and < 9 => (house / 3 * 3 + 2, house % 3 * 3 + 2, 3, 3),
			>= 9 and < 18 => (house - 9 + 2, 2, 1, 9),
			>= 18 and < 27 => (2, house - 18 + 2, 9, 1),
			_ => T<(int, int, int, int)>(house, 27)
		};

		var control = new Border
		{
			Background = new SolidColorBrush(IdentifierConversion.GetColor(id)),
			Tag = houseNode,
			Opacity = sudokuPane.EnableAnimationFeedback ? 0 : (double)sudokuPane.HighlightBackgroundOpacity,
			Margin = house switch
			{
				>= 0 and < 9 => new(12),
				>= 9 and < 18 => new(6, 12, 6, 12),
				>= 18 and < 27 => new(12, 6, 12, 6),
				_ => T<Thickness>(house, 27)
			},
			CornerRadius = house switch { >= 0 and < 9 => new(12), >= 9 and < 27 => new(18), _ => T<CornerRadius>(house, 27) },
			BorderThickness = new(0)
		};

		GridLayout.SetRow(control, row);
		GridLayout.SetColumn(control, column);
		GridLayout.SetRowSpan(control, rowSpan);
		GridLayout.SetColumnSpan(control, columnSpan);
		Canvas.SetZIndex(control, -3);

		if (sudokuPane.EnableAnimationFeedback)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add((() => gridControl.Children.Add(control), () => control.Opacity = (double)sudokuPane.HighlightBackgroundOpacity));
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="ChuteViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="chuteNode">Indicates the chute view nodes.</param>
	/// <exception cref="ArgumentException">
	/// Throws when the argument <paramref name="chuteNode"/> stores invalid data of property <see cref="ChuteViewNode.ChuteIndex"/>.
	/// </exception>
	/// <seealso cref="ChuteViewNode"/>
	private static partial void ForChuteNode(DrawingContext context, ChuteViewNode chuteNode)
	{
		var (sudokuPane, animatedResults) = context;
		if (sudokuPane.MainGrid is not { } gridControl)
		{
			return;
		}

		var (id, chute) = chuteNode;
		var (row, column, rowSpan, columnSpan) = chute switch
		{
			>= 0 and < 3 => (chute * 3 + 2, 2, 3, 9),
			>= 3 and < 6 => (2, (chute - 3) * 3 + 2, 9, 3),
			_ => T<(int, int, int, int)>(chute, 6)
		};

		var control = new Border
		{
			Background = new SolidColorBrush(IdentifierConversion.GetColor(id)),
			Tag = chuteNode,
			Opacity = sudokuPane.EnableAnimationFeedback ? 0 : (double)sudokuPane.HighlightBackgroundOpacity,
			Margin = chute switch { >= 0 and < 3 => new(6, 12, 6, 12), >= 3 and < 6 => new(12, 6, 12, 6), _ => T<Thickness>(chute, 6) },
			CornerRadius = new(18),
			BorderThickness = new(0)
		};

		GridLayout.SetRow(control, row);
		GridLayout.SetColumn(control, column);
		GridLayout.SetRowSpan(control, rowSpan);
		GridLayout.SetColumnSpan(control, columnSpan);
		Canvas.SetZIndex(control, -4);

		if (sudokuPane.EnableAnimationFeedback)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add((() => gridControl.Children.Add(control), () => control.Opacity = (double)sudokuPane.HighlightBackgroundOpacity));
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="BabaGroupViewNode"/>.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="babaGroupNode">Indicates the baba grouping view nodes.</param>
	/// <seealso cref="BabaGroupViewNode"/>
	private static partial void ForBabaGroupNode(DrawingContext context, BabaGroupViewNode babaGroupNode)
	{
		var (sudokuPane, animatedResults) = context;
		var (id, cell, @char) = babaGroupNode;
		if (sudokuPane._children[cell] is not { } paneCellControl)
		{
			return;
		}

		var control = new Border
		{
			BorderThickness = new(0),
			Tag = babaGroupNode,
			Opacity = sudokuPane.EnableAnimationFeedback ? 0 : (double)sudokuPane.HighlightBackgroundOpacity,
			Child = new TextBlock
			{
				Text = @char.ToString(),
				FontSize = PencilmarkTextConversion.GetFontSizeSimple(sudokuPane.ApproximateCellWidth, sudokuPane.BabaGroupLabelFontScale) * 1.618,
				FontFamily = sudokuPane.BabaGroupLabelFont,
				Foreground = new SolidColorBrush(sudokuPane.BabaGroupLabelColor),
				FontWeight = FontWeights.Bold,
				FontStyle = FontStyle.Italic,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				TextAlignment = TextAlignment.Center
			}
		};

		GridLayout.SetRowSpan(control, 3);
		GridLayout.SetColumnSpan(control, 3);
		Canvas.SetZIndex(control, -1);

		if (sudokuPane.EnableAnimationFeedback)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add((() => paneCellControl.MainGrid.Children.Add(control), () => control.Opacity = 1));
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for <see cref="ILinkViewNode"/> instances.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="linkNodes">Indicates the link view nodes.</param>
	/// <param name="candidateNodes">Indicates the candidate view nodes.</param>
	/// <param name="conclusions">Indicates the conclusions. The value is used for appending links between tail node and conclusion.</param>
	/// <remarks>
	/// This method is special: We should handle all <see cref="ILinkViewNode"/> instances together.
	/// </remarks>
	private static partial void ForLinkNodes(DrawingContext context, ReadOnlySpan<ILinkViewNode> linkNodes, ReadOnlySpan<CandidateViewNode> candidateNodes, ReadOnlyMemory<Conclusion> conclusions)
	{
		var (sudokuPane, animatedResults) = context;
		if (sudokuPane.MainGrid is not { } gridControl)
		{
			return;
		}

		foreach (var control in new PathCreator(sudokuPane, new(gridControl), candidateNodes.ToArray(), conclusions).CreateShapes(linkNodes))
		{
			GridLayout.SetRow(control, 2);
			GridLayout.SetColumn(control, 2);
			GridLayout.SetRowSpan(control, 9);
			GridLayout.SetColumnSpan(control, 9);
			Canvas.SetZIndex(control, -1);

			if (sudokuPane.EnableAnimationFeedback)
			{
				control.OpacityTransition = new();
			}
			animatedResults.Add((() => gridControl.Children.Add(control), () => control.Opacity = 1));
		}
	}

	/// <summary>
	/// Create <see cref="FrameworkElement"/>s that displays for grouped node instances.
	/// </summary>
	/// <param name="context">Indicates the drawing context.</param>
	/// <param name="nodes">Indicates the nodes to be drawn.</param>
	private static partial void ForGroupedNodes(DrawingContext context, ReadOnlySpan<GroupedNodeInfo> nodes)
	{
		var (sudokuPane, animatedResults) = context;
		if (sudokuPane.MainGrid is not { } gridControl)
		{
			return;
		}

		foreach (var control in new GroupedNodeCreator(sudokuPane, new(gridControl)).CreateShapes(nodes))
		{
			GridLayout.SetRow(control, 2);
			GridLayout.SetColumn(control, 2);
			GridLayout.SetRowSpan(control, 9);
			GridLayout.SetColumnSpan(control, 9);
			Canvas.SetZIndex(control, -1);

			if (sudokuPane.EnableAnimationFeedback)
			{
				control.OpacityTransition = new();
			}
			animatedResults.Add((() => gridControl.Children.Add(control), () => control.Opacity = 1));
		}
	}

	/// <summary>
	/// The core method called by <see cref="ForCandidateNode(DrawingContext, CandidateViewNode, ReadOnlyMemory{Conclusion}, out Conclusion?)"/>.
	/// </summary>
	/// <param name="id">The color identifier.</param>
	/// <param name="color">The color to be used on rendering.</param>
	/// <param name="candidate">The candidate to be rendered.</param>
	/// <param name="candidateNode">The back candidate node.</param>
	/// <param name="paneCellControl">The pane cell control that stores the rendered control.</param>
	/// <param name="animatedResults"><inheritdoc cref="DrawingContext.ControlAddingActions" path="/summary"/></param>
	/// <param name="isForConclusion">Indicates whether the operation draws for a conclusion.</param>
	/// <param name="isForElimination">Indicates whether the operation draws for an elimination.</param>
	/// <param name="isOverlapped">Indicates whether the operation draws for an overlapped conclusion.</param>
	private static void ForCandidateNodeCore(
		ColorIdentifier id,
		Color color,
		Candidate candidate,
		CandidateViewNode? candidateNode,
		SudokuPaneCell paneCellControl,
		AnimatedResultCollection animatedResults,
		bool isForConclusion = false,
		bool isForElimination = false,
		bool isOverlapped = false
	)
	{
		const float epsilon = 1E-2F;
		if (paneCellControl is not
			{
				ActualSize: var size,
				BasePane:
				{
					Width: var fallbackSize, // WinUI issue makes 'paneCellControl.ActualSize' equal to 0 in help message window.
					HighlightCandidateCircleScale: var highlightScale,
					EnableAnimationFeedback: var enableAnimation,
					CandidateViewNodeDisplayMode: var candidateDisplayMode,
					EliminationDisplayMode: var eliminationDisplayMode,
					AssignmentDisplayMode: var assignmentDisplayMode
				}
			})
		{
			return;
		}

		var converter = new RxCyConverter();
		var resultSize = size is var (x, y) && (x.NearlyEquals(0, epsilon) || y.NearlyEquals(0, epsilon)) && fallbackSize / 9 is var f
			? new((float)f, (float)f)
			: size;
		var (width, height) = resultSize / 3F * (float)highlightScale;
		var control = (isForConclusion, isForElimination, candidateDisplayMode, eliminationDisplayMode, assignmentDisplayMode) switch
		{
			(true, true, _, EliminationDisplay.CircleSolid, _) or (true, false, _, _, AssignmentDisplay.CircleSolid) => new Ellipse
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Fill = getFillBrush(color),
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1
			},
			(true, true, _, EliminationDisplay.CircleHollow, _) or (true, false, _, _, AssignmentDisplay.CircleHollow) => new Ellipse
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = (width + height) / 2 * 3 / 20,
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1
			},
			(true, true, _, EliminationDisplay.Cross or EliminationDisplay.Slash or EliminationDisplay.Backslash, _) => new Cross
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Background = new SolidColorBrush(color),
				StrokeThickness = (width + height) / 2 * 3 / 20,
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1,
				ForwardLineVisibility = eliminationDisplayMode switch
				{
					EliminationDisplay.Cross or EliminationDisplay.Slash => Visibility.Visible,
					_ => Visibility.Collapsed
				},
				BackwardLineVisibility = eliminationDisplayMode switch
				{
					EliminationDisplay.Cross or EliminationDisplay.Backslash => Visibility.Visible,
					_ => Visibility.Collapsed
				}
			},
			(true, _, _, _, _) or (_, _, CandidateViewNodeDisplay.CircleSolid, _, _) => new Ellipse
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Fill = getFillBrush(color),
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1
			},
			(_, _, CandidateViewNodeDisplay.CircleHollow, _, _) => new Ellipse
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = (width + height) / 2 * 3 / 20,
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1
			},
			(_, _, CandidateViewNodeDisplay.SquareHollow, _, _) => new Rectangle
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = (width + height) / 2 * 3 / 20,
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1
			},
			(_, _, CandidateViewNodeDisplay.SquareSolid, _, _) => new Rectangle
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Fill = getFillBrush(color),
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1,
			},
			(_, _, CandidateViewNodeDisplay.RoundedRectangleHollow, _, _) => new Rectangle
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Fill = new SolidColorBrush(color),
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1,
				RadiusX = width / 3,
				RadiusY = height / 3
			},
			(_, _, CandidateViewNodeDisplay.RoundedRectangleSolid, _, _) => new Rectangle
			{
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Fill = getFillBrush(color),
				Tag = c(),
				Opacity = enableAnimation ? 0 : 1,
				RadiusX = width / 3,
				RadiusY = height / 3
			},
			_ => default(FrameworkElement)!
		};

		var digit = candidate % 9;
		GridLayout.SetRow(control, digit / 3);
		GridLayout.SetColumn(control, digit % 3);
		Canvas.SetZIndex(control, -1);

		if (enableAnimation)
		{
			control.OpacityTransition = new();
		}

		animatedResults.Add((() => paneCellControl.MainGrid.Children.Add(control), () => control.Opacity = 1));


		static Brush getFillBrush(Color color) => new SolidColorBrush(color);

		IDrawableItem c()
			=> candidateNode is not null
				? candidateNode
				: new Conclusion(isForElimination ? Elimination : Assignment, candidate);
	}

	[DoesNotReturn]
	private static T? T<T>(object? o, int range, [CallerArgumentExpression(nameof(o))] string? s = null)
		where T : allows ref struct
		=> throw new InvalidOperationException($"The {s} index configured is invalid - it must be between 0 and {range}.");
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// <para>Fast determines whether the specified conclusion list contains the specified candidate.</para>
	/// <para>This method is used for checking cannibalism.</para>
	/// </summary>
	/// <param name="conclusions">The conclusion collection.</param>
	/// <param name="candidate">The candidate to be determined.</param>
	/// <param name="conclusion">The overlapped result.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool ConflictWith(this ReadOnlyMemory<Conclusion> conclusions, Candidate candidate, [NotNullWhen(true)] out Conclusion? conclusion)
	{
		foreach (var current in conclusions)
		{
			if (current.Candidate == candidate)
			{
				conclusion = current;
				return true;
			}
		}

		conclusion = null;
		return false;
	}
}
