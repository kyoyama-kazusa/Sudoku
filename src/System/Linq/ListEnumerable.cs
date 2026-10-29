namespace System.Linq;

/// <summary>
/// Represents with LINQ methods for <see cref="List{T}"/>.
/// </summary>
/// <seealso cref="List{T}"/>
public static class ListEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of each element.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource>(List<TSource> source)
	{
		/// <summary>
		/// Totals up the number of elements that satisfy the specified condition.
		/// </summary>
		/// <param name="predicate">The condition.</param>
		/// <returns>The number of elements satisfying the specified condition.</returns>
		public int Count(Func<TSource, bool> predicate)
		{
			var result = 0;
			foreach (var element in source)
			{
				if (predicate(element))
				{
					result++;
				}
			}
			return result;
		}

		/// <inheritdoc cref="extension{TSource}(List{TSource}).Count(Func{TSource, bool})"/>
		public int CountUnsafe(delegate*<TSource, bool> predicate)
		{
			var result = 0;
			foreach (var element in source)
			{
				if (unsafe (predicate(element)))
				{
					result++;
				}
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
		public ReadOnlySpan<TSource> Where(Func<TSource, bool> condition)
		{
			var result = new List<TSource>(source.Count);
			foreach (var element in source)
			{
				if (condition(element))
				{
					result.AddRef(element);
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			var result = new TResult[source.Count];
			var i = 0;
			foreach (var element in source)
			{
				result[i++] = selector(element);
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> SelectMany<TCollection, TResult>(
			Func<TSource, ReadOnlySpan<TCollection>> collectionSelector,
			Func<TSource, TCollection, TResult> resultSelector
		)
		{
			var result = new List<TResult>(source.Count << 1);
			foreach (var element in source)
			{
				foreach (ref readonly var collectionElement in collectionSelector(element))
				{
					result.AddRef(resultSelector(element, collectionElement));
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> SelectMany<TCollection, TResult>(
			Func<TSource, IEnumerable<TCollection>> collectionSelector,
			Func<TSource, TCollection, TResult> resultSelector
		)
		{
			var result = new List<TResult>(source.Count << 1);
			foreach (var element in source)
			{
				foreach (var collectionElement in collectionSelector(element))
				{
					result.AddRef(resultSelector(element, collectionElement));
				}
			}
			return result.AsSpan();
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource>(List<TSource> source)
		where TSource : IAdditiveIdentity<TSource, TSource>, IAdditionOperators<TSource, TSource, TSource>
	{
		/// <inheritdoc cref="Enumerable.Sum(IEnumerable{int})"/>
		public TSource Sum()
		{
			var result = TSource.AdditiveIdentity;
			foreach (ref readonly var element in source.AsSpan())
			{
				result += element;
			}
			return result;
		}
	}
}
