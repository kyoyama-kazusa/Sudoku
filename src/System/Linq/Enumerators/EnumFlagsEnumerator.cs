namespace System.Linq.Enumerators;

/// <summary>
/// Defines an enumerator that iterates the possible fields of an enumeration type.
/// </summary>
/// <typeparam name="TEnum">The type of the enumeration type, that is marked the attribute <see cref="FlagsAttribute"/>.</typeparam>
/// <param name="baseField"><inheritdoc cref="_baseField" path="/summary"/></param>
public ref struct EnumFlagsEnumerator<TEnum>(TEnum baseField) : IEnumerator<TEnum> where TEnum : unmanaged, Enum
{
	/// <summary>
	/// Indicates the base field.
	/// </summary>
	private readonly TEnum _baseField = baseField;

	/// <summary>
	/// Indicates the fields of the type to iterate.
	/// </summary>
	private readonly TEnum[] _fields = Enum.GetValues<TEnum>();

	/// <summary>
	/// Indicates the current index being iterated.
	/// </summary>
	private int _index = -1;


	/// <inheritdoc cref="IEnumerator.Current"/>
	public TEnum Current { get; private set; } = default;

	/// <inheritdoc/>
	readonly object IEnumerator.Current => Current;


	/// <inheritdoc cref="IEnumerator.MoveNext"/>
	public bool MoveNext()
	{
		var size = sizeof(TEnum);
		for (var index = _index + 1; index < _fields.Length; index++)
		{
			var field = _fields[index];
			switch (size)
			{
				case 1 or 2 or 4 when IsPow2(Unsafe.As<TEnum, uint>(ref field)) && _baseField.HasFlag(field):
				{
					Current = _fields[_index = index];
					return true;
				}
				case 8 when IsPow2(Unsafe.As<TEnum, ulong>(ref field)) && _baseField.HasFlag(field):
				{
					Current = _fields[_index = index];
					return true;
				}
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
