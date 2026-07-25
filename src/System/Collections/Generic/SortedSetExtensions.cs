namespace System.Collections.Generic;

/// <summary>
/// Provides with extension methods on <see cref="SortedSet{T}"/>.
/// </summary>
/// <seealso cref="SortedSet{T}"/>
public static class SortedSetExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each value.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(SortedSet<T> @this)
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
				var count = @this.Count;
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


		/// <summary>
		/// Adds the elements into the collection.
		/// </summary>
		/// <param name="values">The elements to be added.</param>
		/// <returns>The number of elements successfully to be added.</returns>
		public int AddRange(params ReadOnlySpan<T> values)
		{
			var result = 0;
			foreach (var element in values)
			{
				if (@this.Add(element))
				{
					result++;
				}
			}
			return result;
		}

		/// <summary>
		/// Try to convert the current instance into an array.
		/// </summary>
		/// <returns>An array of <typeparamref name="T"/> elements.</returns>
		public T[] ToArray()
		{
			var result = new T[@this.Count];
			@this.CopyTo(result);
			return result;
		}
	}
}
