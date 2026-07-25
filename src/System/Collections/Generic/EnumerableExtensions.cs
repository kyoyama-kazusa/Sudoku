namespace System.Collections.Generic;

/// <summary>
/// Provides extension members on both <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/> values.
/// </summary>
/// <seealso cref="IEnumerable"/>
/// <seealso cref="IEnumerable{T}"/>
public static class EnumerableExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="IEnumerable{T}"/> instances.
	/// </summary>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(IEnumerable<T> @this)
	{
		/// <summary>
		/// Gets the specified element at the desired index on the order of default adding.
		/// </summary>
		/// <param name="index">The desired index.</param>
		/// <returns>The target element at the specified index.</returns>
		public T this[int index]
		{
			get
			{
				var count = @this.Count();
				if (index < 0 || index >= count)
				{
					throw new IndexOutOfRangeException();
				}

				using var enumerator = @this.GetEnumerator();
				for (var i = 0; i <= index; i++)
				{
					enumerator.MoveNext();
				}
				return enumerator.Current;
			}
		}
	}
}
