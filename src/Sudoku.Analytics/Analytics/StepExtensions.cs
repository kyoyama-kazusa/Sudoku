namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Step"/>.
/// </summary>
/// <seealso cref="Step"/>
public static class StepExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Step"/>.
	/// </summary>
	extension(Step)
	{
		/// <summary>
		/// Compare names of two <see cref="Step"/> instances.
		/// </summary>
		/// <param name="left">The left instance to be compared.</param>
		/// <param name="right">The right instance to be compared.</param>
		/// <param name="formatProvider">The culture information.</param>
		/// <returns>An <see cref="int"/> value indicating which is bigger.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompareName(Step? left, Step? right, IFormatProvider? formatProvider)
			=> (left, right) switch
			{
				(null, null) => 0,
				(not null, null) => 1,
				(null, not null) => -1,
				_ => left.NameCompareTo(right, formatProvider)
			};

		/// <summary>
		/// Zips the collection, pairing each step and corresponding grid into a <see cref="ValueTuple{T1, T2}"/>,
		/// and return the collection of pairs.
		/// </summary>
		/// <param name="interimGrids">The grids corresponded.</param>
		/// <param name="interimSteps">The steps.</param>
		/// <returns>The zipped collection.</returns>
		/// <exception cref="InvalidOperationException">
		/// Throws when the length of arguments <paramref name="interimGrids"/> and <paramref name="interimSteps"/> aren't same.
		/// </exception>
		public static ReadOnlySpan<KeyValuePair<Grid, Step>> Combine(ReadOnlySpan<Grid> interimGrids, ReadOnlySpan<Step> interimSteps)
		{
			if (interimGrids.Length != interimSteps.Length)
			{
				var message = string.Format(SR.ExceptionMessage("LengthMustBeSame"), [nameof(interimGrids), nameof(interimSteps)]);
				throw new InvalidOperationException(message);
			}

			var result = new List<KeyValuePair<Grid, Step>>(interimGrids.Length);
			for (var i = 0; i < interimGrids.Length; i++)
			{
				result.AddRef(KeyValuePair.Create(interimGrids[i], interimSteps[i]));
			}
			return result.AsSpan();
		}
	}
}
