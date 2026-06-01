namespace Sudoku.Drawing;

/// <summary>
/// Represents a canvas that allows drawing sudoku-related items onto it.
/// </summary>
public sealed partial class GridCanvas : IDisposable
{
	/// <summary>
	/// Represents a radian value equivalent to 45 degree. This field is used for rotate the chains if overlapped.
	/// </summary>
	private const float RotateAngle = PI / 4;

	/// <summary>
	/// Represents square root of 2. This field is used for rotate the chains.
	/// </summary>
	private const float SqrtOf2 = 1.4142135F;


	/// <summary>
	/// Indicates whether the footer text can be drawn in this canvas, making canvas reserve footer.
	/// </summary>
	private readonly bool _needFooterText;

	/// <summary>
	/// Indicates the backing point calculator to be used.
	/// </summary>
	private readonly PointCalculator _calculator;

	/// <summary>
	/// Represents a <see cref="StringFormat"/> instance that locates the text drawn, created by method
	/// <see cref="Graphics.DrawString(string?, Font, Brush, RectangleF, StringFormat?)"/>,
	/// with centering the text with both horizontal aligning and vertical aligning.
	/// </summary>
	/// <seealso cref="Graphics.DrawString(string?, Font, Brush, RectangleF, StringFormat?)"/>
	private readonly StringFormat _stringAligner = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

	/// <summary>
	/// Indicates the backing bitmap to be used.
	/// </summary>
	private readonly Bitmap _backingBitmap;

	/// <summary>
	/// Indicates the backing <see cref="Graphics"/> instance to draw elements.
	/// </summary>
	private readonly Graphics _g;

	/// <summary>
	/// Indicates whether the object is disposed.
	/// </summary>
	private bool _isDisposed;


	/// <summary>
	/// Initializes a <see cref="GridCanvas"/> instance via some values.
	/// </summary>
	/// <param name="size">Indicates the picture size to be used.</param>
	/// <param name="padding">Indicates the padding of the inner grid.</param>
	/// <param name="settings">Indicates settings that can be used by drawing items.</param>
	/// <param name="needFooterText">Indicates whether footer text is enabled to be drawn.</param>
	public GridCanvas(int size, int padding, GridCanvasSettings? settings = null, bool needFooterText = false)
	{
		Settings = settings ?? new();
		_calculator = new(size, padding);
		_needFooterText = needFooterText;
		_g = CreateGraphics(_needFooterText, size, settings, out _backingBitmap);
		Clear();
	}


	/// <summary>
	/// Indicates canvas settings to be used. The value can be changed if you want to change.
	/// </summary>
	public GridCanvasSettings Settings { get; }


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_stringAligner.Dispose();
		_backingBitmap.Dispose();
		_g.Dispose();

		_isDisposed = true;
	}

	/// <summary>
	/// Try to save the picture into the local path.
	/// </summary>
	/// <param name="filePath">Indicates the file path.</param>
	/// <exception cref="ArgumentException">Throws when the file doesn't contain any valid file extension.</exception>
	/// <exception cref="NotSupportedException">Throws when the specified file extension is not supported.</exception>
	public void SavePictureTo(string filePath)
	{
		const string error_ContainsNoExtension = "The file path is not supported because the file doesn't contain any valid extension.";
		const string error_NotSupportedExtension = "The file path doesn't have a valid file extension to be output picture.";
		switch ((Path.GetExtension(filePath) is { Length: not 0 } p ? p : null)?.ToLower())
		{
			case null:
			{
				throw new ArgumentException(error_ContainsNoExtension, nameof(filePath));
			}
			case ".wmf":
			{
				using var tempBitmap = new Bitmap((int)_calculator.Width, (int)_calculator.Height);
				using var tempGraphics = Graphics.FromImage(tempBitmap);
				using var metaFile = new Metafile(filePath, tempGraphics.GetHdc());
				using var g = Graphics.FromImage(metaFile);
				g.DrawImage(_backingBitmap, 0, 0);
				tempGraphics.ReleaseHdc();
				break;
			}
			case { Length: >= 4 } e and (".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif"):
			{
				_backingBitmap.Save(
					filePath,
					e switch
					{
						".jpg" or ".jpeg" => ImageFormat.Jpeg,
						".png" => ImageFormat.Png,
						".bmp" => ImageFormat.Bmp,
						".gif" => ImageFormat.Gif
					}
				);
				break;
			}
			default:
			{
				throw new NotSupportedException(error_NotSupportedExtension);
			}
		}
	}

	/// <summary>
	/// Try to get the result color value.
	/// </summary>
	/// <param name="paletteColorIndex">The value of ID.</param>
	/// <param name="result">The result color got.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	/// <exception cref="InvalidOperationException">Throws when the ID is invalid.</exception>
	private bool TryGetPaletteColorByIndex(int paletteColorIndex, out Color result)
	{
		var palette = Settings.ColorPalette;
		return (result = palette.Count > paletteColorIndex ? palette[paletteColorIndex] : Color.Transparent) != Color.Transparent;
	}

	/// <summary>
	/// Gets the color value.
	/// </summary>
	/// <param name="id">The identifier instance.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException">Throws when the specified value is invalid.</exception>
	private Color GetColor(ColorDescriptor id)
		=> id switch
		{
			(byte a, byte r, byte g, byte b) => Color.FromArgb(a, r, g, b),
			int value when TryGetPaletteColorByIndex(value, out var color) => Color.FromArgb(64, color),
			ColorDescriptorAlias alias => alias switch
			{
				ColorDescriptorAlias.Normal => Settings.NormalColor,
				ColorDescriptorAlias.Assignment => Settings.AssignmentColor,
				ColorDescriptorAlias.OverlappedAssignment => Settings.OverlappedAssignmentColor,
				ColorDescriptorAlias.Elimination => Settings.EliminationColor,
				ColorDescriptorAlias.Cannibalism => Settings.CannibalismColor,
				ColorDescriptorAlias.Exofin => Settings.ExofinColor,
				ColorDescriptorAlias.Endofin => Settings.EndofinColor,
				ColorDescriptorAlias.Link => Settings.ChainColor,
				ColorDescriptorAlias.Auxiliary1 => Settings.AuxiliaryColorSet[0],
				ColorDescriptorAlias.Auxiliary2 => Settings.AuxiliaryColorSet[1],
				ColorDescriptorAlias.Auxiliary3 => Settings.AuxiliaryColorSet[2],
				ColorDescriptorAlias.AlmostLockedSet1 => Settings.AlmostLockedSetColorSet[0],
				ColorDescriptorAlias.AlmostLockedSet2 => Settings.AlmostLockedSetColorSet[1],
				ColorDescriptorAlias.AlmostLockedSet3 => Settings.AlmostLockedSetColorSet[2],
				ColorDescriptorAlias.AlmostLockedSet4 => Settings.AlmostLockedSetColorSet[3],
				ColorDescriptorAlias.AlmostLockedSet5 => Settings.AlmostLockedSetColorSet[4],
				ColorDescriptorAlias.Rectangle1 => Settings.RectangleColorSet[0],
				ColorDescriptorAlias.Rectangle2 => Settings.RectangleColorSet[1],
				ColorDescriptorAlias.Rectangle3 => Settings.RectangleColorSet[2],
				_ => throw new ArgumentException("The specified kind is invalid.", nameof(id))
			},
			_ => throw new ArgumentException("The specified kind is invalid.", nameof(id))
		};


	/// <summary>
	/// Try to get extra height that footer text will use.
	/// </summary>
	/// <param name="footerText">Indicates the footer text.</param>
	/// <param name="size">Indicates the size to be used.</param>
	/// <param name="settings">Indicates the settings to be used.</param>
	/// <returns>Indicates the measured extra height.</returns>
	private static float MeasureFooterTextExtraHeight(string footerText, int size, GridCanvasSettings? settings)
	{
		using var tempBitmap = new Bitmap(size, size);
		using var tempGraphics = Graphics.FromImage(tempBitmap);
		using var font = GetFooterTextFont(size, settings);
		return tempGraphics.MeasureString(footerText, font).Height;
	}

	/// <summary>
	/// Get the font via the specified name, size and the scale.
	/// </summary>
	/// <param name="size">The size that decides the default font size.</param>
	/// <param name="settings">Indicates the settings.</param>
	/// <returns>The font.</returns>
	private static Font GetFooterTextFont(float size, GridCanvasSettings? settings)
		=> new((settings ??= new()).FooterTextFontName!, size / 9 * (float)settings.FooterTextScale, settings.FooterTextFontStyle);

	/// <summary>
	/// Get the font via the specified name, size and the scale.
	/// </summary>
	/// <param name="fontName">The font name that decides the font to use and presentation.</param>
	/// <param name="size">The size that decides the default font size.</param>
	/// <param name="scale">The scale that decides the result font size.</param>
	/// <param name="style">The style that decides the font style of the text in the picture.</param>
	/// <returns>The font.</returns>
	/// <exception cref="ArgumentNullException">Throws when <paramref name="fontName"/> is <see langword="null"/>.</exception>
	private static Font GetFont(string fontName, float size, decimal scale, FontStyle style)
		=> new(fontName, size * (float)scale, style);

	/// <summary>
	/// Creates a <see cref="Graphics"/> instance via values.
	/// </summary>
	/// <param name="needFooterText">Indicates whether canvas can draw footer text.</param>
	/// <param name="size">Indicates the size.</param>
	/// <param name="settings">Indicates the settings.</param>
	/// <param name="bitmap">Indicates the bitmap instance that returns after <see cref="Graphics"/> instance is created.</param>
	/// <returns>A <see cref="Graphics"/> instance.</returns>
	private static Graphics CreateGraphics(bool needFooterText, int size, GridCanvasSettings? settings, out Bitmap bitmap)
	{
		var g = Graphics.FromImage(bitmap = new(size, (int)(needFooterText ? size + MeasureFooterTextExtraHeight("a", size, settings) : size)));
		g.SmoothingMode = SmoothingMode.HighQuality;
		g.TextRenderingHint = TextRenderingHint.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.CompositingQuality = CompositingQuality.HighQuality;
		return g;
	}


	public partial void Clear();
	public partial void DrawBackground();
	public partial void DrawBorderLines();
	public partial void DrawFooterText(string footerText);
	public partial void DrawGrid<TGrid>(in TGrid grid) where TGrid : unmanaged, IInlineArrayGrid<TGrid>;
	public partial void DrawCellViewNodes(ReadOnlySpan<CellViewNode> nodes);
	public partial void DrawCandidateViewNodes(ReadOnlySpan<CandidateViewNode> nodes, ReadOnlySpan<Conclusion> conclusions);
	public partial void DrawHouseViewNodes(ReadOnlySpan<HouseViewNode> nodes);
	public partial void DrawLinkViewNodes(ReadOnlySpan<ILinkViewNode> nodes, ReadOnlySpan<Conclusion> conclusions);
	public partial void DrawChuteViewNodes(ReadOnlySpan<ChuteViewNode> nodes);
	public partial void DrawBabaGroupingViewNodes(ReadOnlySpan<BabaGroupViewNode> nodes);
	public partial void DrawEliminations(ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<CandidateViewNode> nodes);
	public partial void DrawIconViewNodes(ReadOnlySpan<IconViewNode> nodes);
}
