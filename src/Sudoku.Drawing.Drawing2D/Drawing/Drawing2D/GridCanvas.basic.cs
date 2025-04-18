namespace Sudoku.Drawing.Drawing2D;

public partial class GridCanvas
{
	/// <summary>
	/// Clear the canvas, removing all drawn elements.
	/// </summary>
	/// <remarks>
	/// This will invalidate drawing items, and draw background and border lines.
	/// </remarks>
	public partial void Clear()
	{
		DrawBackground();
		DrawBorderLines();
	}

	/// <summary>
	/// Draw the background.
	/// </summary>
	public partial void DrawBackground() => _g.Clear(Settings.BackgroundColor);

	/// <summary>
	/// Draw border lines onto the canvas.
	/// </summary>
	public partial void DrawBorderLines()
	{
		const int anchorsCountTotal = PointCalculator.AnchorsCount + 1;
		var gridPoints = _calculator.GridPoints;
		using var pg = new Pen(Settings.GridLineColor, Settings.GridLineWidth);
		using var pb = new Pen(Settings.BlockLineColor, Settings.BlockLineWidth);
		for (var i = 0; i < anchorsCountTotal; i += PointCalculator.AnchorsCount / 9)
		{
			_g.DrawLine(pg, gridPoints[i, 0], gridPoints[i, PointCalculator.AnchorsCount]);
			_g.DrawLine(pg, gridPoints[0, i], gridPoints[PointCalculator.AnchorsCount, i]);
		}
		for (var i = 0; i < anchorsCountTotal; i += PointCalculator.AnchorsCount / 3)
		{
			_g.DrawLine(pb, gridPoints[i, 0], gridPoints[i, PointCalculator.AnchorsCount]);
			_g.DrawLine(pb, gridPoints[0, i], gridPoints[PointCalculator.AnchorsCount, i]);
		}
	}

	/// <summary>
	/// Draw footer text onto the canvas.
	/// </summary>
	/// <param name="footerText">The footer text.</param>
	/// <exception cref="InvalidOperationException">Throws when the canvas doesn't support drawing footer text.</exception>
	public partial void DrawFooterText(string footerText)
	{
		if (!_needFooterText)
		{
			throw new InvalidOperationException();
		}

		var w = _calculator.Width;
		using var brush = new SolidBrush(Settings.FooterTextColor);
		using var font = GetFooterTextFont(w, Settings);
		var extraHeight = _g.MeasureString(footerText, font).Height;
		_g.DrawString(footerText, font, brush, new RectangleF(0, w, w, extraHeight), _stringAligner);
	}

	/// <summary>
	/// Draw a <see cref="Grid"/> or <see cref="MarkerGrid"/> instance onto the canvas.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="grid">The grid to be drawn.</param>
	public partial void DrawGrid<TGrid>(in TGrid grid) where TGrid : unmanaged, IGrid<TGrid>
	{
		if (Settings is not
			{
				GivenColor: var gColor,
				ModifiableColor: var mColor,
				CandidateColor: var cColor,
				GivenFontName: { } gFontName,
				ModifiableFontName: { } mFontName,
				CandidateFontName: { } cFontName,
				ValueScale: var vScale,
				CandidateScale: var cScale,
				GivenFontStyle: var gFontStyle,
				ModifiableFontStyle: var mFontStyle,
				CandidateFontStyle: var cFontStyle,
				ShowCandidates: var showCandidates
			})
		{
			return;
		}

		var cellWidth = _calculator.CellSize.Width;
		var candidateWidth = _calculator.CandidateSize.Width;
		var vOffsetValue = cellWidth / (PointCalculator.AnchorsCount / 3); // The vertical offset of drawing each value.
		var vOffsetCandidate = candidateWidth / (PointCalculator.AnchorsCount / 3); // The vertical offset of drawing each candidate.
		var halfWidth = cellWidth / 2F;
		using var bGiven = new SolidBrush(gColor);
		using var bModifiable = new SolidBrush(mColor);
		using var bCandidate = new SolidBrush(cColor);
		using var bCandidateLighter = new SolidBrush(cColor.QuarterAlpha);
		using var fGiven = GetFont(gFontName, halfWidth, vScale, gFontStyle);
		using var fModifiable = GetFont(mFontName, halfWidth, vScale, mFontStyle);
		using var fCandidate = GetFont(cFontName, halfWidth, cScale, cFontStyle);
		for (var cell = 0; cell < 81; cell++)
		{
			var mask = grid[cell];
			switch (MaskOperations.MaskToCellState(mask))
			{
				case 0 when showCandidates: // Draw candidates.
				{
					// This block is use when user draw candidates from undefined grid.
					foreach (var digit in (Mask)(mask & Grid.MaxCandidatesMask))
					{
						var originalPoint = _calculator.GetMousePointInCenter(cell, digit);
						var point = originalPoint with { Y = originalPoint.Y + vOffsetCandidate };
						_g.DrawValue<Digit>(digit + 1, fCandidate, bCandidate, point, _stringAligner);
					}
					break;
				}
				case CellState.Empty when showCandidates: // Draw candidates.
				{
					foreach (var digit in (Mask)(mask & Grid.MaxCandidatesMask))
					{
						var originalPoint = _calculator.GetMousePointInCenter(cell, digit);
						var point = originalPoint with { Y = originalPoint.Y + vOffsetCandidate };
						_g.DrawValue<Digit>(digit + 1, fCandidate, bCandidate, point, _stringAligner);
					}
					break;
				}
				case var state and (CellState.Modifiable or CellState.Given):
				{
					// Draw values.
					var originalPoint = _calculator.GetMousePointInCenter(cell);
					var point = originalPoint with { Y = originalPoint.Y + vOffsetValue };
					_g.DrawValue<Digit>(grid.GetDigit(cell) + 1, f(state, fGiven, fModifiable), f(state, bGiven, bModifiable), point, _stringAligner);
					break;


					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					static T f<T>(CellState state, T given, T modifiable) where T : allows ref struct
						=> state == CellState.Given ? given : modifiable;
				}
			}
		}
	}
}
