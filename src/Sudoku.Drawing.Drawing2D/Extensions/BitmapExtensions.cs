namespace System.Drawing;

/// <summary>
/// Provides extension methods on <see cref="Bitmap"/>.
/// </summary>
/// <seealso cref="Bitmap"/>
internal static class BitmapExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Bitmap"/>.
	/// </summary>
	extension(Bitmap @this)
	{
		/// <summary>
		/// Zoom a picture.
		/// </summary>
		/// <param name="newWidth">The new width.</param>
		/// <param name="newHeight">The new height.</param>
		/// <returns>The new bitmap instance that has been zoomed.</returns>
		public Bitmap ZoomTo(int newWidth, int newHeight)
		{
			var b = new Bitmap(newWidth, newHeight);
			using var g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(@this, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, @this.Width, @this.Height), GraphicsUnit.Pixel);
			return b;
		}
	}
}
