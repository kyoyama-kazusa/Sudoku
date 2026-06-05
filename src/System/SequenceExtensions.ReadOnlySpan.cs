namespace System;

public partial class SequenceExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(ReadOnlySpan<T> @this)
	{
		/// <summary>
		/// Finds the last element satisfying the specified condition, and return its corresponding index.
		/// </summary>
		/// <param name="predicate">The condition.</param>
		/// <returns>
		/// An <see cref="int"/> indicating the found element. -1 returns if the sequence has no element satisfying the condition.
		/// </returns>
		public int LastIndex(Func<T, bool> predicate)
		{
			for (var i = @this.Length - 1; i >= 0; i--)
			{
				if (predicate(@this[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
		public int FindIndex(Func<T, bool> condition)
		{
			for (var i = 0; i < @this.Length; i++)
			{
				if (condition(@this[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Iterates on each element, in reverse order.
		/// </summary>
		/// <returns>An enumerator type that iterates on each element.</returns>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Reverse, false)]
		public ReverseEnumerator<T> EnumerateReversely() => new(@this);

		/// <summary>
		/// Retrieves all the elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <param name="match">The <see cref="Func{T, TResult}"/> that defines the conditions of the elements to search for.</param>
		/// <returns>
		/// A <see cref="ReadOnlySpan{T}"/> containing all the elements that match the conditions defined
		/// by the specified predicate, if found; otherwise, an empty <see cref="ReadOnlySpan{T}"/>.
		/// </returns>
		public ReadOnlySpan<T> FindAll(Func<T, bool> match)
		{
			var result = new List<T>(@this.Length);
			foreach (ref readonly var element in @this)
			{
				if (match(element))
				{
					result.AddRef(element);
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="op_Division{T}(ReadOnlySpan{T}, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Chunk, false)]
		public ReadOnlySpan<T[]> Chunk(int size) => @this / size;

		/// <inheritdoc cref="op_BitwiseAnd{T}(ReadOnlySpan{T}, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Subset, false)]
		public ReadOnlySpan<T[]> GetSubsets(int size) => @this & size;

		/// <inheritdoc cref="op_BitwiseOr{T}(ReadOnlySpan{T}, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_AllSubset, false)]
		public ReadOnlySpan<T[]> GetSubsets() => @this | @this.Length;

		/// <summary>
		/// Get all permutations from the collection.
		/// </summary>
		/// <returns>
		/// All possible permutations returned.
		/// </returns>
		public ReadOnlySpan<ReadOnlyMemory<T>> GetPermutations()
		{
			var result = new List<ReadOnlyMemory<T>>();
			for (var size = 1; size <= @this.Length; size++)
			{
				foreach (var element in @this.GetPermutations(size))
				{
					result.Add(element);
				}
			}
			return result.AsSpan();
		}

		/// <summary>
		/// Get all permutations from the specified number of the values to take.
		/// </summary>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>
		/// The permutations of the list.
		/// For example, if the input array is <c>[1, 2, 3]</c> and the argument <paramref name="count"/> is 2, the result will be
		/// <code><![CDATA[
		/// [[1, 2], [2, 1], [1, 3], [3, 1], [2, 3], [3, 2]]
		/// ]]></code>
		/// 6 cases.
		/// </returns>
		public ReadOnlySpan<ReadOnlyMemory<T>> GetPermutations(int count)
		{
			if (count == 0)
			{
				return [];
			}

			var result = new List<ReadOnlyMemory<T>>(Combinatorics.PermutationOf(@this.Length, count));
			var used = (stackalloc bool[@this.Length]);
			used.Clear();

			getPermutationsCore([with(count)], @this, used, count, result);
			return result.AsSpan();


			static void getPermutationsCore(
				List<T> temp,
				ReadOnlySpan<T> array,
				Span<bool> used,
				int count,
				List<ReadOnlyMemory<T>> result
			)
			{
				if (temp.Count == count)
				{
					result.Add(temp.ToArray());
					return;
				}

				for (var i = 0; i < array.Length; i++)
				{
					if (!used[i])
					{
						used[i] = true;
						temp.Add(array[i]);

						getPermutationsCore(temp, array, used, count, result);

						temp.RemoveAt(^1);
						used[i] = false;
					}
				}
			}
		}


		/// <summary>
		/// Creates a reversed collection of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The reversed collection.</returns>
		public static ReadOnlySpan<T> operator ~(scoped ReadOnlySpan<T> value)
		{
			var result = new T[value.Length];
			for (var (i, j) = (value.Length - 1, 0); i >= 0; i--, j++)
			{
				result[j] = value[i];
			}
			return result;
		}

		/// <summary>
		/// Splits a sequence into multiple sub-arrays ("chunks") of the specified size.
		/// The elements are grouped in their original order, and the last chunk may contain fewer elements
		/// if the total length of the array is not evenly divisible by the chunk size.
		/// </summary>
		/// <param name="source">The one-dimensional array to be divided into chunks.</param>
		/// <param name="size">
		/// The maximum number of elements in each chunk. Must be greater than zero.
		/// </param>
		/// <returns>
		/// An array of sub-arrays, where each sub-array contains up to <paramref name="size"/> elements
		/// in the same order as they appear in the <paramref name="source"/> array.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="size"/> is less than or equal to zero.
		/// </exception>
		public static ReadOnlySpan<T[]> operator /(ReadOnlySpan<T> source, int size)
		{
			var result = new List<T[]>((source.Length + size - 1) / size);
			for (var i = 0; i < source.Length; i += size)
			{
				var subarray = new T[size];
				for (var j = 0; j < size; j++)
				{
					subarray[j] = source[i * size + j];
				}
				result.Add(subarray);
			}
			return result.AsSpan();
		}

		/// <summary>
		/// Get all subsets from the specified number of the values to take.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="count">The number of elements you want to take.</param>
		/// <returns>
		/// The subsets of the list.
		/// For example, if the input array is <c>[1, 2, 3]</c> and the argument <paramref name="count"/> is 2, the result will be
		/// <code><![CDATA[
		/// [[1, 2], [1, 3], [2, 3]]
		/// ]]></code>
		/// 3 cases.
		/// </returns>
		/// <exception cref="ArgumentException">Throws when the argument is negative.</exception>
		public static ReadOnlySpan<T[]> operator &(ReadOnlySpan<T> value, int count)
		{
			ArgumentException.Assert(count >= 0);

			if (count == 0)
			{
				return [];
			}

			var result = new List<T[]>();
			getSubsetsCore(value.Length, count, count, stackalloc int[count], value, result);
			return result.AsSpan();


			static void getSubsetsCore(
				int last,
				int count,
				int index,
				Span<int> tempArray,
				ReadOnlySpan<T> thisCopied,
				List<T[]> resultList
			)
			{
				for (var i = last; i >= index; i--)
				{
					tempArray[index - 1] = i - 1;
					if (index > 1)
					{
						getSubsetsCore(i - 1, count, index - 1, tempArray, thisCopied, resultList);
					}
					else
					{
						var temp = new T[count];
						for (var j = 0; j < tempArray.Length; j++)
						{
							temp[j] = thisCopied[tempArray[j]];
						}
						resultList.Add(temp);
					}
				}
			}
		}

		/// <summary>
		/// Get all subsets from the limited number of the values to take,
		/// specified as maximum number of elements of each combination.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="count">The number of elements you want to take, as maximum possible value.</param>
		/// <exception cref="ArgumentException">Throws when the argument is negative.</exception>
		public static ReadOnlySpan<T[]> operator |(ReadOnlySpan<T> value, int count)
		{
			var result = new List<T[]>();
			for (var size = 1; size <= count; size++)
			{
				result.AddRange(value & size);
			}
			return result.AsSpan();
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(ReadOnlySpan<T> @this) where T : notnull
	{
		/// <summary>
		/// Creates a <see cref="PairEnumerator{T}"/> instance that iterates on each element of pair elements.
		/// </summary>
		/// <returns>An enumerable collection.</returns>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Chunk, false)]
		public PairEnumerator<T> EnumeratePaired()
			=> new(
				(@this.Length & 1) == 0
					? @this
					: throw new ArgumentException(SR.ExceptionMessage("SpecifiedValueMustBeEven"), nameof(@this))
			);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(scoped ReadOnlySpan<T> @this)
	{
		/// <summary>
		/// Returns a new <see cref="ReadOnlySpan{T}"/> instance whose internal elements are all come from the current collection,
		/// with reversed order.
		/// </summary>
		/// <returns>A new collection whose elements are in reversed order.</returns>
		[OverloadResolutionPriority(1)]
		[Obsolete(DeprecatedMessages.ExtensionOperator_Reverse, false)]
		public ReadOnlySpan<T> Reverse() => ~@this;
	}
}
