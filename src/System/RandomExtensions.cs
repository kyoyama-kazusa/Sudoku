namespace System;

/// <summary>
/// Provides with extension methods on <see cref="Random"/>.
/// </summary>
/// <seealso cref="Random"/>
public static class RandomExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Random @this)
	{
		/// <summary>
		/// Generates a random number obeying Gaussian's Normal Distribution,
		/// with σ value <paramref name="sigma"/> and μ value <paramref name="mu"/>.
		/// </summary>
		/// <param name="mu">Mu μ value.</param>
		/// <param name="sigma">Sigma σ value.</param>
		/// <returns>The result value.</returns>
		public double NextGaussian(double mu, double sigma)
		{
			var u1 = 1D - @this.NextDouble();
			var u2 = 1D - @this.NextDouble();
			var randStdNormal = Math.Sqrt(-2D * Math.Log(u1)) * Math.Sin(Math.Tau * u2);
			return mu + sigma * randStdNormal;
		}

		/// <summary>
		/// Randomly select one element from the collection.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="values">The values.</param>
		/// <returns>The chosen element.</returns>
		/// <exception cref="InvalidOperationException">Throws when the specified collection is empty.</exception>
		public ref readonly T Choose<T>(ReadOnlySpan<T> values) => ref values[@this.Next(0, values.Length)];

		/// <summary>
		/// Randomly select one element from the collection, with possibility specified by normalizer function.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <typeparam name="TKey">The type of value normalized.</typeparam>
		/// <param name="values">The values.</param>
		/// <param name="normalizer">The method that calculate the value (weight) of the object to be chosen.</param>
		/// <returns>The chosen element.</returns>
		/// <exception cref="InvalidOperationException">
		/// Throws when the specified collection is empty, or normalizer produces a negative number.
		/// </exception>
		public T NormalizedChoose<T, TKey>(ReadOnlySpan<T> values, Func<T, TKey> normalizer) where TKey : INumber<TKey>
		{
			InvalidOperationException.Assert(!values.IsEmpty);
			InvalidOperationException.Assert(values.All(value => normalizer(value) >= TKey.Zero));

			var totalScore = values.Sum(normalizer);
			var cumulative = new List<TKey>();
			var current = TKey.Zero;
			foreach (var value in values)
			{
				current += normalizer(value) / totalScore;
				cumulative.Add(current);
			}

			var randomValue = TKey.CreateChecked(@this.NextDouble());
			for (var i = 0; i < cumulative.Count; i++)
			{
				if (randomValue < cumulative[i])
				{
					return values[i];
				}
			}
			return values[^1];
		}

		/// <summary>
		/// Randomly select a sequence of elements from the collection.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="values">The values.</param>
		/// <param name="count">The desired number of elements to get.</param>
		/// <returns>The chosen elements.</returns>
		/// <exception cref="InvalidOperationException">
		/// Throws when the specified collection doesn't contain enough elements to get.
		/// </exception>
		public ReadOnlySpan<T> Choose<T>(ReadOnlySpan<T> values, int count)
		{
			InvalidOperationException.Assert(values.Length >= count);
			if (values.Length == count)
			{
				return values;
			}

			var result = new T[count];
			if (count <= values.Length >> 1)
			{
				var bitArray = new BitArray(values.Length);
				for (var i = 0; i < count; i++)
				{
					var index = @this.Next(0, values.Length);
					if (bitArray[index])
					{
						// Fallback.
						do
						{
							index = (index + 1) % values.Length;
						} while (bitArray[index]);
					}
					result[i] = values[index];
					bitArray[index] = true;
				}
			}
			else
			{
				var sequence = (stackalloc int[values.Length]);
				for (var i = 0; i < values.Length; i++)
				{
					sequence[i] = i;
				}
				@this.Shuffle(sequence);

				for (var i = 0; i < count; i++)
				{
					result[i] = values[sequence[i]];
				}
			}
			return result;
		}
	}
}
