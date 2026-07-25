namespace System.Numerics;

/// <summary>
/// Represents an enumerator that iterates bits over an <see cref="nint"/> or <see cref="nuint"/> value.
/// </summary>
/// <param name="_value">The value to be iterated.</param>
public ref struct NumericIntPtrEnumerator(nint _value) : IBitEnumerator
{
	/// <inheritdoc/>
	public readonly int PopulationCount => PopCount((nuint)_value);

	/// <inheritdoc/>
	public readonly ReadOnlySpan<int> Bits => _value.AllSets;

	/// <inheritdoc cref="IEnumerator{T}.Current"/>
	public int Current { get; private set; } = -1;

	/// <inheritdoc/>
	readonly object IEnumerator.Current => Current;


	/// <inheritdoc cref="BitOperationsExtensions.SetAt(uint, int)"/>
	public readonly int this[int index] => _value[index];


	/// <inheritdoc cref="IEnumerator.MoveNext"/>
	public unsafe bool MoveNext()
	{
		while (++Current < sizeof(nuint) << 3)
		{
			if ((_value >>> Current & 1) != 0)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	[DoesNotReturn]
	readonly void IEnumerator.Reset() => throw new NotImplementedException();

	/// <inheritdoc/>
	readonly void IDisposable.Dispose()
	{
	}
}
