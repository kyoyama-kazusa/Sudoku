namespace System.Numerics;

/// <summary>
/// Represents an enumerator that iterates bits over a <see cref="long"/> or <see cref="ulong"/> value.
/// </summary>
/// <param name="_value">The value to be iterated.</param>
public ref struct Int64Enumerator(long _value) : IBitEnumerator
{
	/// <inheritdoc/>
	public readonly int PopulationCount => PopCount((ulong)_value);

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
		Current = Log2((ulong)mask);
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
