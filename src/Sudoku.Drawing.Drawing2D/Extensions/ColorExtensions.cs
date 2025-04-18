namespace System.Drawing;

/// <summary>
/// Provides with extension methods on <see cref="Color"/>.
/// </summary>
/// <seealso cref="Color"/>
internal static class ColorExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Color"/>.
	/// </summary>
	extension(Color @this)
	{
		/// <summary>
		/// Gets a target <see cref="Color"/> whose <see cref="Color.A"/> value is a quarter of the original one.
		/// </summary>
		public Color QuarterAlpha => Color.FromArgb(@this.A >> 2, @this);


		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Deconstruct(out byte a, out byte r, out byte g, out byte b) => (a, r, g, b) = (@this.A, @this.R, @this.G, @this.B);
	}
}
