namespace System.Collections.Generic;

/// <summary>
/// Provides with extension methods on <see cref="HashSet{T}"/>.
/// </summary>
/// <seealso cref="HashSet{T}"/>
public static class HashSetExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of each value in the collection.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(HashSet<T> @this)
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
				if (index < 0 || index >= @this.Count)
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
		/// Add a new instance into the collection.
		/// </summary>
		/// <param name="value">The value.</param>
		public bool AddRef(in T value) => Entry<T>.AddIfNotPresent(@this, value, out _);

		/// <summary>
		/// Try to convert a <see cref="HashSet{T}"/> into an array, without any conversions among internal values.
		/// </summary>
		/// <returns>An array converted.</returns>
		public T[] ToArray()
		{
			var result = new T[@this.Count];
			var enumerator = @this.GetEnumerator();
			var i = 0;
			while (enumerator.MoveNext())
			{
				var currentRef = Entry<T>.EnumeratorEntry.GetCurrentFieldRef(ref enumerator);
				result[i++] = currentRef;
			}
			return result;
		}

		/// <summary>
		/// Try to convert a <see cref="HashSet{T}"/> into a <see cref="ReadOnlySpan{T}"/>,
		/// without any conversions among internal values.
		/// </summary>
		/// <returns>A <see cref="ReadOnlySpan{T}"/> converted.</returns>
		public ReadOnlySpan<T> AsSpan() => @this.ToArray();
	}
}

/// <summary>
/// Represents an entry to call internal fields on <see cref="HashSet{T}"/>.
/// </summary>
/// <typeparam name="T">The type of each element in <see cref="HashSet{T}"/>.</typeparam>
/// <seealso cref="HashSet{T}"/>
file static class Entry<T>
{
	/// <summary>
	/// Adds the specified element to the set if it's not already contained.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="value">The element to add to the set.</param>
	/// <param name="location">The index into <c>_entries</c> of the element.</param>
	/// <returns>
	/// <see langword="true"/> if the element is added to the <see cref="HashSet{T}"/> object;
	/// <see langword="false"/> if the element is already present.
	/// </returns>
	/// <remarks>
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@name='others']"/>
	/// </remarks>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(AddIfNotPresent))]
	public static extern safe bool AddIfNotPresent(HashSet<T> @this, T value, out int location);


	/// <summary>
	/// Represents an entry to call internal fields on <see cref="HashSet{T}.Enumerator"/>.
	/// </summary>
	/// <seealso cref="HashSet{T}.Enumerator"/>
	public static class EnumeratorEntry
	{
		/// <summary>
		/// Try to fetch the internal field <c>_current</c> in type <see cref="HashSet{T}.Enumerator"/>.
		/// </summary>
		/// <param name="this">The set.</param>
		/// <returns>The reference to the internal field.</returns>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@name='others']"/>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@name='field-related-method']"/>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@type='struct']"/>
		/// </remarks>
		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = LibraryIdentifiers.Enumerator_Current)]
		public static extern safe ref T GetCurrentFieldRef(ref HashSet<T>.Enumerator @this);
	}
}
