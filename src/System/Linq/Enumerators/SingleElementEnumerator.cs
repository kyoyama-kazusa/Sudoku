namespace System.Linq.Enumerators;

/// <summary>
/// Represents an enumerator that iterates over a single element.
/// </summary>
/// <typeparam name="T">The type of the element to enumerate.</typeparam>
/// <param name="_item">The sole element yielded by this enumerator.</param>
public ref struct SingleElementEnumerator<T>(T _item) : IEnumerator<T>
{
	/// <summary>
	/// Tracks the state of the enumerator.
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term>0</term>
	/// <description>Initial (before first <see cref="MoveNext"/>)</description>
	/// </item>
	/// <item>
	/// <term>1</term>
	/// <description>Positioned on the element (<see cref="Current"/> is valid)</description>
	/// </item>
	/// <item>
	/// <term>-1</term>
	/// <description>Exhausted (after the element has been consumed)</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <seealso cref="MoveNext"/>
	/// <seealso cref="Current"/>
	private int _state = 0;


	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">
	/// Throws when state invalid (invokes this property, but enumerator is not started).
	/// </exception>
	public readonly T Current
		=> _state == 1
			? _item
			: throw new InvalidOperationException($"Current cannot be accessed before the first '{nameof(MoveNext)}' call or after the enumeration has finished.");

	/// <inheritdoc/>
	readonly object? IEnumerator.Current => Current;


	/// <inheritdoc/>
	public bool MoveNext()
	{
		switch (_state)
		{
			case 0: // Initial -> positioned on the element.
			{
				_state = 1;
				return true;
			}
			case 1: // Positioned -> exhausted.
			{
				_state = -1;
				return false;
			}
			default: // Already exhausted.
			{
				return false;
			}
		}
	}

	/// <inheritdoc/>
	public readonly void Dispose()
	{
	}

	/// <inheritdoc/>
	void IEnumerator.Reset() => _state = 0;
}
