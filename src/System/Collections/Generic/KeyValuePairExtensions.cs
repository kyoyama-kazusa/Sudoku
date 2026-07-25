namespace System.Collections.Generic;

/// <summary>
/// Provides with extension methods on <see cref="KeyValuePair{TKey, TValue}"/>.
/// </summary>
/// <seealso cref="KeyValuePair{TKey, TValue}"/>
public static class KeyValuePairExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension<TKey, TValue>(in KeyValuePair<TKey, TValue> @this)
	{
		/// <summary>
		/// Indicates the reference of key.
		/// </summary>
		public ref TKey KeyRef => ref Entry<TKey, TValue>.GetKey(in @this);

		/// <summary>
		/// Indicates the reference to value.
		/// </summary>
		public ref TValue ValueRef => ref Entry<TKey, TValue>.GetValue(in @this);


		/// <summary>
		/// Converts the current <see cref="KeyValuePair{TKey, TValue}"/> instance into a pair of values.
		/// </summary>
		/// <returns>The final pair of values converted.</returns>
		public (TKey, TValue) ToTuple() => (@this.Key, @this.Value);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <typeparam name="TKeyResult">The type of result key.</typeparam>
	/// <typeparam name="TValueResult">The type of result value.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<TKey, TValue, TKeyResult, TValueResult>(in KeyValuePair<TKey, TValue> @this)
		where TKey : TKeyResult
		where TValue : TValueResult
	{
		/// <summary>
		/// Casts the current instance into a new type of <see cref="KeyValuePair{TKey, TValue}"/>
		/// with type projection:
		/// <list type="bullet">
		/// <item><typeparamref name="TKey"/> -> <typeparamref name="TKeyResult"/></item>
		/// <item><typeparamref name="TValue"/> -> <typeparamref name="TValueResult"/></item>
		/// </list>
		/// </summary>
		/// <returns>The casted result.</returns>
		public KeyValuePair<TKeyResult, TValueResult> Cast()
			=> KeyValuePair.Create((TKeyResult)@this.Key, (TValueResult)@this.Value);
	}
}

/// <summary>
/// Represents an entry to call internal fields on <see cref="KeyValuePair{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The key type of each element in <see cref="KeyValuePair{TKey, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The value type of each element in <see cref="KeyValuePair{TKey, TValue}"/>.</typeparam>
/// <seealso cref="KeyValuePair{TKey, TValue}"/>
file sealed class Entry<TKey, TValue>
{
	/// <summary>
	/// Try to fetch the internal field <c>key</c> in type <see cref="KeyValuePair{TKey, TValue}"/>.
	/// </summary>
	/// <param name="this">The key-value pair.</param>
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
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = LibraryIdentifiers.KeyValuePair_Key)]
	public static extern safe ref TKey GetKey(ref readonly KeyValuePair<TKey, TValue> @this);

	/// <summary>
	/// Try to fetch the internal field <c>value</c> in type <see cref="KeyValuePair{TKey, TValue}"/>.
	/// </summary>
	/// <param name="this">The key-value pair.</param>
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
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = LibraryIdentifiers.KeyValuePair_Value)]
	public static extern safe ref TValue GetValue(ref readonly KeyValuePair<TKey, TValue> @this);
}
