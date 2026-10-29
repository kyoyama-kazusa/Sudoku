namespace System.Numerics;

/// <summary>
/// Represents an enumerator that iterates bits over a <see cref="Int128"/> or <see cref="UInt128"/> value.
/// </summary>
/// <param name="_value">The value to be iterated.</param>
public ref struct Int128Enumerator(Int128 _value) : IBitEnumerator
{
	/// <inheritdoc/>
	public readonly int PopulationCount
		=> PopCount((ulong)(_value >>> 64)) + PopCount((ulong)(_value & (Int128.One << 64) - 1));

	/// <inheritdoc/>
	public readonly ReadOnlySpan<int> Bits => _value.AllSets;

	/// <inheritdoc cref="IEnumerator{T}.Current"/>
	public int Current { get; private set; } = -1;

	/// <inheritdoc/>
	readonly object IEnumerator.Current => Current;


	/// <inheritdoc/>
	public readonly int this[int index] => _value[index];


	/// <inheritdoc cref="IEnumerator.MoveNext"/>
	public bool MoveNext()
	{
		if (_value == 0)
		{
			return false;
		}

		var mask = _value & -_value;
		Current = (int)Int128.Log2(mask);
		_value &= ~mask;
		return true;
	}

	/// <inheritdoc/>
	[DoesNotReturn]
	readonly void IEnumerator.Reset() => throw new NotImplementedException();

	/// <inheritdoc/>
	readonly void IDisposable.Dispose()
	{
	}
}
