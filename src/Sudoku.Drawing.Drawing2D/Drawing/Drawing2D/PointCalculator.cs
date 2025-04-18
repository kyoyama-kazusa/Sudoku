namespace Sudoku.Drawing.Drawing2D;

/// <summary>
/// Provides a serial of methods for a point calculator that interacts with the UI projects.
/// </summary>
public sealed class PointCalculator : IPointCalculator
{
	/// <summary>
	/// Indicates the number of anchors hold per house.
	/// </summary>
	/// <remarks>
	/// The sudoku grid painter will draw the outlines and the inner lines, and correct the point
	/// of each digits (candidates also included). Each row or column always contains 27 candidates,
	/// so this value is 27.
	/// </remarks>
	internal const int AnchorsCount = 27;

	/// <summary>
	/// Indicates the default padding of the sudoku grid drawn.
	/// </summary>
	private const float DefaultPadding = 10F;


	/// <summary>
	/// Initializes a <see cref="PointCalculator"/> instance via the specified picture size and padding value.
	/// </summary>
	/// <param name="size">The size of the picture.</param>
	/// <param name="padding">The padding.</param>
	public PointCalculator(float size, float padding = DefaultPadding) : this(new SizeF(size, size), padding)
	{
	}

	/// <summary>
	/// Initializes a <see cref="PointCalculator"/> instance with the specified picture size instance and padding value.
	/// </summary>
	/// <param name="size">The <see cref="SizeF"/> instance.</param>
	/// <param name="padding">
	/// Indicates the padding to set to allow the output items moving a little bit left or right,
	/// in order to correct the position on drawing. The default value is <c>10F</c>, which is defined by <see cref="DefaultPadding"/>
	/// </param>
	/// <seealso cref="DefaultPadding"/>
	private PointCalculator(SizeF size, float padding)
	{
		// Initialize sizes.
		ControlSize = size;

		var (width, height) = size;
		Width = width;
		Height = height;
		Padding = padding;

		var gridWidth = width - Padding * 2;
		var gridHeight = height - Padding * 2;
		GridSize = new(gridWidth, gridHeight);
		CellSize = new(gridWidth / (AnchorsCount / 3), gridHeight / (AnchorsCount / 3));
		CandidateSize = new(gridWidth / AnchorsCount, gridHeight / AnchorsCount);

		// Initialize points.
		const int length = AnchorsCount + 1;
		var (cw, ch) = CandidateSize;
		GridPoints = new PointF[length, length];
		for (var i = 0; i < length; i++)
		{
			for (var j = 0; j < length; j++)
			{
				GridPoints[i, j] = new(cw * i + Padding, ch * j + Padding);
			}
		}
	}


	/// <inheritdoc/>
	public float Width { get; }

	/// <inheritdoc/>
	public float Height { get; }

	/// <inheritdoc/>
	/// <remarks>The default value is <c>10</c>.</remarks>
	public float Padding { get; } = DefaultPadding;

	/// <summary>
	/// Indicates the control size.
	/// </summary>
	public SizeF ControlSize { get; }

	/// <summary>
	/// Indicates the grid size.
	/// </summary>
	public SizeF GridSize { get; }

	/// <summary>
	/// Indicates the cell size.
	/// </summary>
	public SizeF CellSize { get; }

	/// <summary>
	/// Indicates the candidate size.
	/// </summary>
	public SizeF CandidateSize { get; }

	/// <summary>
	/// Indicates the absolutely points in grid cross-lines.
	/// This property will be assigned later (and not <see langword="null"/>).
	/// </summary>
	/// <remarks>Note that the size of this 2D array is always 28 by 28.</remarks>
	public PointF[,] GridPoints { get; }

	/// <inheritdoc/>
	(float Width, float Height) IPointCalculator.ControlSize => (ControlSize.Width, ControlSize.Height);

	/// <inheritdoc/>
	(float Width, float Height) IPointCalculator.GridSize => (GridSize.Width, GridSize.Height);

	/// <inheritdoc/>
	(float Width, float Height) IPointCalculator.CellSize => (CellSize.Width, CellSize.Height);

	/// <inheritdoc/>
	(float Width, float Height) IPointCalculator.CandidateSize => (CandidateSize.Width, CandidateSize.Height);

	/// <inheritdoc/>
	(float X, float Y)[,] IPointCalculator.GridPoints => from pt in GridPoints select (pt.X, pt.Y);


	/// <summary>
	/// Get the focus cell offset via a mouse point.
	/// </summary>
	/// <param name="point">The mouse point.</param>
	/// <returns>The cell offset. Returns -1 when the current point is invalid.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Cell GetCell(PointF point)
	{
		var (x, y) = point with { X = point.X - Padding, Y = point.Y - Padding };
		if (x < 0 || x > GridSize.Width || y < 0 || y > GridSize.Height)
		{
			return -1;
		}

		var (cw, ch) = CellSize;
		var result = (int)(y / ch) * 9 + (int)(x / cw);
		return result is >= 0 and < 81 ? result : -1;
	}

	/// <summary>
	/// Get the focus candidate offset via a mouse point.
	/// </summary>
	/// <param name="point">The mouse point.</param>
	/// <returns>The candidate offset.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Candidate GetCandidate(PointF point)
	{
		var ((x, y), (cw, ch)) = (point, CandidateSize);
		var (a, b) = ((int)((y - Padding) / ch), (int)((x - Padding) / cw));
		return GetCell(point) * 9 + a % 3 * 3 + b % 3;
	}

	/// <summary>
	/// Get the center mouse point of all candidates.
	/// </summary>
	/// <param name="map">The map of candidates.</param>
	/// <returns>The center mouse point.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PointF GetMouseCenter(in CandidateMap map)
	{
		switch (map)
		{
			case [var min, .., var max]:
			{
				var (cMin, dMin, cMax, dMax) = (min / 9, min % 9, max / 9, max % 9);
				var ((x1, y1), (x2, y2)) = (GetMousePointInCenter(cMin, dMin), GetMousePointInCenter(cMax, dMax));
				return new((x1 + x2) / 2, (y1 + y2) / 2);
			}
			case [var s]:
			{
				return GetMousePointInCenter(s / 9, s % 9);
			}
			default:
			{
				throw new ArgumentException("The value contains no elements.", nameof(map));
			}
		}
	}

	/// <summary>
	/// Get the rectangle from all candidates.
	/// </summary>
	/// <param name="map">The candidates.</param>
	/// <returns>The rectangle.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectangleF GetMouseRectangle(in CandidateMap map)
	{
		switch (map, CandidateSize)
		{
			case ([var min, .., var max], var (cw, ch)):
			{
				var (cMin, dMin, cMax, dMax) = (min / 9, min % 9, max / 9, max % 9);
				var (a, b) = (GetMousePointInCenter(cMin, dMin), GetMousePointInCenter(cMax, dMax));
				var (p, q) = (a with { X = a.X - cw / 2, Y = a.Y - ch / 2 }, b with { X = b.X + cw / 2, Y = b.Y + ch / 2 });
				return RectangleF.Create(p, q);
			}
			default:
			{
				throw new ArgumentException("The value contains no elements.", nameof(map));
			}
		}
	}

	/// <summary>
	/// Get the rectangle (4 mouse points) via the specified cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The rectangle.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectangleF GetMouseRectangleViaCell(Cell cell)
	{
		var ((cw, ch), (x, y)) = (CellSize, GetMousePointInCenter(cell));
		return new(x - cw / 2, y - ch / 2, cw, ch);
	}

	/// <summary>
	/// Get the rectangle (4 mouse points) for the specified cell and digit of a candidate.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>The rectangle.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectangleF GetMouseRectangle(Cell cell, Digit digit)
	{
		var ((cw, ch), (x, y)) = (CandidateSize, GetMousePointInCenter(cell, digit));
		return new(x - cw / 2, y - ch / 2, cw, ch);
	}

	/// <summary>
	/// Get the rectangle (4 mouse points) via the specified house.
	/// </summary>
	/// <param name="house">The house.</param>
	/// <returns>The rectangle.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectangleF GetMouseRectangleViaHouse(House house)
	{
		var (l, r) = GetAnchorsViaHouse(house);
		return RectangleF.Create(l, r);
	}

	/// <summary>
	/// Gets two points that specifies and represents the anchors of this house.
	/// </summary>
	/// <param name="house">The house.</param>
	/// <returns>The anchor points.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (PointF TopLeft, PointF BottomRight) GetAnchorsViaHouse(House house)
		=> house switch
		{
			>= 0 and < 9 when (house % 3, house / 3) is var (v1, v2) => (GridPoints[v1 * 9, v2 * 9], GridPoints[v1 * 9 + 9, v2 * 9 + 9]),
			>= 9 and < 18 when house - 9 is var v => (GridPoints[0, v * 3], GridPoints[27, v * 3 + 3]),
			>= 18 and < 27 when house - 18 is var v => (GridPoints[v * 3, 0], GridPoints[v * 3 + 3, 27]),
			_ => throw new ArgumentOutOfRangeException(nameof(house))
		};

	/// <summary>
	/// Gets two points specifies and represents a line as shared border grid lines between two adjacent cells.
	/// </summary>
	/// <param name="cell1">The first cell.</param>
	/// <param name="cell2">The second cell that is adjacent with <paramref name="cell1"/> by row or column.</param>
	/// <returns>The two points representing with a line.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (PointF Start, PointF End) GetSharedLinePosition(Cell cell1, Cell cell2) => GetSharedLinePosition(cell1, cell2, false);

	/// <summary>
	/// Get the mouse point of the center of a cell via its offset.
	/// </summary>
	/// <param name="cell">The cell offset.</param>
	/// <returns>The mouse point.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PointF GetMousePointInCenter(Cell cell)
	{
		var ((cw, ch), (x, y)) = (CellSize, GridPoints[cell % 9 * 3, cell / 9 * 3]);
		return new(x + cw / 2, y + ch / 2);
	}

	/// <summary>
	/// Get the mouse point of the center of a cell via its offset and the digit.
	/// </summary>
	/// <param name="cell">The cell offset.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>The mouse point.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PointF GetMousePointInCenter(Cell cell, Cell digit)
	{
		var ((cw, ch), (x, y)) = (CandidateSize, GridPoints[cell % 9 * 3 + digit % 3, cell / 9 * 3 + digit / 3]);
		return new(x + cw / 2, y + ch / 2);
	}

	/// <inheritdoc cref="GetSharedLinePosition(Cell, Cell)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal (PointF Start, PointF End) GetSharedLinePosition(Cell cell1, Cell cell2, bool borderBarFullyOverlapsGridLine)
	{
		var ((x, y), (cw, ch)) = GetMouseRectangleViaCell(cell2);
		return (cell2 - cell1 == 1, borderBarFullyOverlapsGridLine) switch
		{
			(true, false) => (new(x, y + ch * .2F), new(x, y + ch * .8F)),
			(false, false) => (new(x + cw * .2F, y), new(x + cw * .8F, y)),
			(true, true) => (new(x, y), new(x, y + ch)),
			(false, true) => (new(x, y), new(x + cw, y))
		};
	}
}
