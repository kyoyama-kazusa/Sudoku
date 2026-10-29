namespace System.Linq;

public partial class SpanEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource>(ReadOnlySpan<TSource> source) where TSource : IComparisonOperators<TSource, TSource, bool>, IMinMaxValue<TSource>
	{
#if false
		/// <inheritdoc cref="IMinMaxMethod{TSelf, TSource}.Min()"/>
		public TSource Min()
		{
			var result = TSource.MaxValue;
			foreach (ref readonly var element in source)
			{
				if (element <= result)
				{
					result = element;
				}
			}
			return result;
		}

		/// <inheritdoc cref="IMinMaxMethod{TSelf, TSource}.Max()"/>
		public TSource Max()
		{
			var result = TSource.MinValue;
			foreach (ref readonly var element in source)
			{
				if (element >= result)
				{
					result = element;
				}
			}
			return result;
		}
#endif

		/// <summary>
		/// Gets the maximum value of the sequence, and ignore elements to be compared if they are not satisfy the specified condition.
		/// </summary>
		/// <param name="predicate">The condition to be checked.</param>
		/// <returns>The maximum value.</returns>
		public TSource MaxIf(Func<TSource, bool> predicate)
		{
			var resultKey = TSource.MinValue;
			foreach (var element in source)
			{
				if (predicate(element) && element >= resultKey)
				{
					resultKey = element;
				}
			}
			return resultKey;
		}

		/// <summary>
		/// Gets the maximum value of the sequence, and ignore elements to be compared if they are not satisfy the specified condition.
		/// </summary>
		/// <param name="predicate">The condition to be checked.</param>
		/// <param name="default">The default value if all elements in sequence are ignored.</param>
		/// <returns>The maximum value.</returns>
		public TSource MaxIf(Func<TSource, bool> predicate, TSource @default)
		{
			var resultKey = TSource.MinValue;
			foreach (var element in source)
			{
				if (predicate(element) && element >= resultKey)
				{
					resultKey = element;
				}
			}
			return resultKey == TSource.MinValue ? @default : resultKey;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TKey>(ReadOnlySpan<TSource> source) where TKey : IMinMaxValue<TKey>, IComparisonOperators<TKey, TKey, bool>
	{
		/// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		public TKey Min(Func<TSource, TKey> keySelector)
		{
			var resultKey = TKey.MaxValue;
			foreach (var element in source)
			{
				var key = keySelector(element);
				if (key <= resultKey)
				{
					resultKey = key;
				}
			}
			return resultKey;
		}

		/// <inheritdoc cref="IMinMaxMethod{TSelf, TSource}.MinBy{TKey}(Func{TSource, TKey})"/>
		public TSource? MinBy(Func<TSource, TKey> keySelector)
		{
			var (resultKey, result) = (TKey.MaxValue, default(TSource));
			foreach (var element in source)
			{
				if (keySelector(element) <= resultKey)
				{
					result = element;
				}
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		public TKey Max(Func<TSource, TKey> keySelector)
		{
			var resultKey = TKey.MinValue;
			foreach (var element in source)
			{
				var key = keySelector(element);
				if (key >= resultKey)
				{
					resultKey = key;
				}
			}
			return resultKey;
		}

		/// <inheritdoc cref="IMinMaxMethod{TSelf, TSource}.MaxBy{TKey}(Func{TSource, TKey})"/>
		public TSource? MaxBy(Func<TSource, TKey> keySelector)
		{
			var (resultKey, result) = (TKey.MinValue, default(TSource));
			foreach (var element in source)
			{
				if (keySelector(element) >= resultKey)
				{
					result = element;
				}
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.Min{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		public TKey? MinUnsafe(delegate*<TSource, TKey> selector)
		{
			var result = TKey.MaxValue;
			foreach (var element in source)
			{
				var elementCasted = unsafe (selector(element));
				if (elementCasted <= result)
				{
					result = elementCasted;
				}
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.Max{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		public TKey MaxUnsafe(delegate*<TSource, TKey> selector)
		{
			var result = TKey.MinValue;
			foreach (var element in source)
			{
				var elementCasted = unsafe (selector(element));
				if (elementCasted >= result)
				{
					result = elementCasted;
				}
			}
			return result;
		}
	}
}
