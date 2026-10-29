namespace System.Linq;

public partial class SpanEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource>(ReadOnlySpan<TSource> source)
		where TSource : IAdditiveIdentity<TSource, TSource>, IAdditionOperators<TSource, TSource, TSource>
	{
		/// <inheritdoc cref="ISumMethod{TSelf, TSource}.Sum"/>
		public TSource Sum()
		{
			var result = TSource.AdditiveIdentity;
			foreach (ref readonly var element in source)
			{
				result += element;
			}
			return result;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource, TKey>(ReadOnlySpan<TSource> source) where TKey : IAdditiveIdentity<TKey, TKey>, IAdditionOperators<TKey, TKey, TKey>
	{
		/// <summary>
		/// Totals up all elements, and return the result of the sum by the specified property calculated from each element.
		/// </summary>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <returns>The value with the sum key in the sequence.</returns>
		public TKey Sum(Func<TSource, TKey> keySelector)
		{
			var result = TKey.AdditiveIdentity;
			foreach (var element in source)
			{
				result += keySelector(element);
			}
			return result;
		}

		/// <inheritdoc cref="Sum{TSource, TKey}(ReadOnlySpan{TSource}, Func{TSource, TKey})"/>
		public TKey SumUnsafe(delegate*<TSource, TKey> selector)
		{
			var result = TKey.AdditiveIdentity;
			foreach (var element in source)
			{
				result += unsafe (selector(element));
			}
			return result;
		}
	}
}
