namespace System;

/// <summary>
/// Provides a way to create <see cref="Span{T}"/>.
/// </summary>
/// <seealso cref="Span{T}"/>
public static class Span
{
	/// <summary>
	/// Creates a <see cref="Span{T}"/> instance.
	/// </summary>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="value">The reference to the first element.</param>
	/// <param name="length">The length.</param>
	/// <returns><see cref="Span{T}"/> instance.</returns>
	public static Span<T> Create<T>(scoped ref T value, int length) => MemoryMarshal.CreateSpan(ref value, length);

	/// <inheritdoc cref="Create{T}(ref T, int)"/>
	public static Span<T> Create<T>(T* value, int length) => MemoryMarshal.CreateSpan(ref unsafe (*value), length);
}
