namespace System.Linq;

public partial class DictionaryEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TKey, TValue>(Dictionary<TKey, TValue> source) where TKey : notnull
	{
		/// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/>
		public KeyValuePair<TKey, TValue> this[int index] => source.ElementAt(index);


		/// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/>
		public KeyValuePair<TKey, TValue> ElementAt(int index)
		{
			using var enumerator = source.GetEnumerator();
			var tempIndex = -1;
			while (enumerator.MoveNext())
			{
				if (++tempIndex == index)
				{
					return enumerator.Current;
				}
			}
			throw new IndexOutOfRangeException();
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TKey, TValue>(Dictionary<TKey, TValue> source)
		where TKey : notnull
		where TValue : IComparable<TValue>, IComparisonOperators<TValue, TValue, bool>, IMinMaxValue<TValue>
	{
		/// <summary>
		/// Get the maximal value in all <see cref="Dictionary{TKey, TValue}.Values"/>.
		/// </summary>
		/// <returns>An instance of type <typeparamref name="TValue"/> as maximal-valued one.</returns>
		/// <seealso cref="Dictionary{TKey, TValue}.Values"/>
		public TValue MaxByValue()
		{
			var result = TValue.MinValue;
			foreach (var value in source.Values)
			{
				if (value >= result)
				{
					result = value;
				}
			}
			return result;
		}

		/// <summary>
		/// Get the minimal value in all <see cref="Dictionary{TKey, TValue}.Values"/>.
		/// </summary>
		/// <returns>An instance of type <typeparamref name="TValue"/> as minimal-valued one.</returns>
		/// <seealso cref="Dictionary{TKey, TValue}.Values"/>
		public TValue MinByValue()
		{
			var result = TValue.MaxValue;
			foreach (var value in source.Values)
			{
				if (value <= result)
				{
					result = value;
				}
			}
			return result;
		}
	}
}
