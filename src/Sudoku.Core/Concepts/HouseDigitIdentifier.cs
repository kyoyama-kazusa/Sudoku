namespace Sudoku.Concepts;

/// <summary>
/// Encapsulates house and digit into one type.
/// </summary>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
[method: JsonConstructor]
public readonly struct HouseDigitIdentifier(House house, Digit digit) :
	IAdditionOperators<HouseDigitIdentifier, byte, HouseDigitIdentifier>,
	IAdditiveIdentity<HouseDigitIdentifier, HouseDigitIdentifier>,
	IComparable<HouseDigitIdentifier>,
	IComparisonOperators<HouseDigitIdentifier, HouseDigitIdentifier, bool>,
	IDecrementOperators<HouseDigitIdentifier>,
	IEquatable<HouseDigitIdentifier>,
	IEqualityOperators<HouseDigitIdentifier, HouseDigitIdentifier, bool>,
	IIncrementOperators<HouseDigitIdentifier>,
	IMinMaxValue<HouseDigitIdentifier>,
	ISubtractionOperators<HouseDigitIdentifier, byte, HouseDigitIdentifier>
{
	/// <summary>
	/// Indicates the raw representation of maximum value.
	/// </summary>
	public const byte MaxRawValue = 27 * 9;


	/// <summary>
	/// Indicates the minimum value of the current type.
	/// </summary>
	public static readonly HouseDigitIdentifier MinValue = new(0, 0);

	/// <summary>
	/// Indicates the maximum value of the current type.
	/// </summary>
	public static readonly HouseDigitIdentifier MaxValue = new(^1, ^1);


	/// <summary>
	/// Represents the backing mask value.
	/// </summary>
	private readonly byte _mask = (byte)(house * 9 + digit);


	/// <summary>
	/// Initializes a <see cref="HouseDigitIdentifier"/> instance via the specified house index and digit index;
	/// from-end cases are also supported.
	/// </summary>
	/// <param name="houseIndex">Indicates the house index.</param>
	/// <param name="digitIndex">Indicates the digit index.</param>
	/// <remarks>
	/// This constructor allows you using index syntax to create a <see cref="HouseDigitIdentifier"/> instance,
	/// like <c><see langword="new"/> <see cref="HouseDigitIdentifier"/>(^1, ^1)</c>.
	/// </remarks>
	public HouseDigitIdentifier(Index houseIndex, Index digitIndex) : this(houseIndex.GetOffset(27), digitIndex.GetOffset(9))
	{
	}


	/// <summary>
	/// Indicates the house.
	/// </summary>
	public House House => _mask / 9;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public Digit Digit => _mask % 9;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[JsonIgnore]
	public ref readonly CellMap Cells => ref HousesMap[House];

	/// <summary>
	/// Indicates the candidates used.
	/// </summary>
	[JsonIgnore]
	public CandidateMap Candidates => Cells * Digit;


	/// <inheritdoc/>
	static HouseDigitIdentifier IMinMaxValue<HouseDigitIdentifier>.MinValue => MinValue;

	/// <inheritdoc/>
	static HouseDigitIdentifier IMinMaxValue<HouseDigitIdentifier>.MaxValue => MaxValue;

	/// <inheritdoc/>
	static HouseDigitIdentifier IAdditiveIdentity<HouseDigitIdentifier, HouseDigitIdentifier>.AdditiveIdentity => MinValue;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out House house, out Digit digit) => (house, digit) = (House, Digit);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is HouseDigitIdentifier comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(HouseDigitIdentifier other) => _mask == other._mask;

	/// <inheritdoc/>
	public int CompareTo(HouseDigitIdentifier other) => _mask.CompareTo(other._mask);

	/// <inheritdoc/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter)
	{
		var houseString = converter.HouseConverter(1 << House);
		var digitString = converter.DigitConverter((Mask)(1 << Digit));
		return $"{houseString}({digitString})";
	}


	/// <inheritdoc cref="TryParse(string?, CoordinateParser, out HouseDigitIdentifier)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out HouseDigitIdentifier result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out HouseDigitIdentifier result)
	{
		try
		{
			if (s is null)
			{
				goto ReturnFalse;
			}

			result = Parse(s, converter);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = default;
		return false;
	}

	/// <inheritdoc cref="Parse(string, CoordinateParser)"/>
	public static HouseDigitIdentifier Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <inheritdoc/>
	public static HouseDigitIdentifier Parse(string s, CoordinateParser converter)
	{
		var indexOfLeftBrace = s.IndexOf('(');
		var indexOfRightBrace = s.IndexOf(')');
		var houseString = s[..indexOfLeftBrace];
		var digitString = s[(indexOfLeftBrace + 1)..indexOfRightBrace];
		var house = converter.HouseParser(houseString);
		var digit = converter.DigitParser(digitString);
		return new(house, digit);
	}


	/// <inheritdoc/>
	public static HouseDigitIdentifier operator ++(HouseDigitIdentifier value) => unchecked(value + 1);

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator checked ++(HouseDigitIdentifier value) => checked(value + 1);

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator --(HouseDigitIdentifier value) => unchecked(value - 1);

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator checked --(HouseDigitIdentifier value) => checked(value - 1);

	/// <inheritdoc/>
	public static bool operator ==(HouseDigitIdentifier left, HouseDigitIdentifier right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(HouseDigitIdentifier left, HouseDigitIdentifier right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(HouseDigitIdentifier left, HouseDigitIdentifier right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(HouseDigitIdentifier left, HouseDigitIdentifier right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(HouseDigitIdentifier left, HouseDigitIdentifier right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(HouseDigitIdentifier left, HouseDigitIdentifier right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator +(HouseDigitIdentifier left, byte right)
		=> unchecked((HouseDigitIdentifier)(byte)((left._mask + right) % MaxRawValue));

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator checked +(HouseDigitIdentifier left, byte right)
		=> right is >= 0 and < MaxRawValue ? checked((HouseDigitIdentifier)(byte)(left._mask + right)) : throw new OverflowException();

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator -(HouseDigitIdentifier left, byte right)
		=> unchecked((HouseDigitIdentifier)(byte)((left._mask + MaxRawValue - right % MaxRawValue) % MaxRawValue));

	/// <inheritdoc/>
	public static HouseDigitIdentifier operator checked -(HouseDigitIdentifier left, byte right)
		=> right is >= 0 and < MaxRawValue ? checked((HouseDigitIdentifier)(byte)(left._mask - right)) : throw new OverflowException();


	/// <summary>
	/// Explicit cast from <see cref="byte"/> to <see cref="HouseDigitIdentifier"/>.
	/// </summary>
	/// <param name="mask">The mask.</param>
	public static explicit operator HouseDigitIdentifier(byte mask)
	{
		mask %= MaxRawValue;
		return new(mask / 9, mask % 9);
	}

	/// <summary>
	/// Explicit cast from <see cref="byte"/> to <see cref="HouseDigitIdentifier"/>, with boundary check.
	/// </summary>
	/// <param name="mask">The mask.</param>
	/// <exception cref="OverflowException">Throws when value is less than 0 or greater than <see cref="MaxRawValue"/> - 1.</exception>
	public static explicit operator checked HouseDigitIdentifier(byte mask)
		=> mask is >= 0 and < MaxRawValue ? new(mask / 9, mask % 9) : throw new OverflowException();

	/// <summary>
	/// Implicit cast from <see cref="HouseDigitIdentifier"/> to <see cref="byte"/>.
	/// </summary>
	/// <param name="value">The instance.</param>
	public static implicit operator byte(HouseDigitIdentifier value) => value._mask;
}
