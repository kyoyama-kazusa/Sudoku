namespace Sudoku.Concepts;

/// <summary>
/// Defines a type that can describe a candidate is the correct or wrong digit.
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
/// <remarks>
/// Two <see cref="Mask"/> values can be compared with each other. If one of those two is an elimination
/// (i.e. holds the value <see cref="Elimination"/> as the type), the instance will be greater;
/// if those two hold same conclusion type,
/// but one of those two holds the global index of the candidate position is greater, it is greater.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct Conclusion(Mask mask) :
	IComparable<Conclusion>,
	IEqualityOperators<Conclusion, Conclusion, bool>,
	IEquatable<Conclusion>
{
	/// <summary>
	/// The field uses the mask table of length 81 to indicate the state and all possible candidates
	/// holding for each cell. Each mask uses a <see cref="Mask"/> value, but only uses 11 of 16 bits.
	/// <code>
	/// | 15  14  13  12  11  10| 9   8   7   6   5   4   3   2   1   0 |
	/// |-------------------|---|---------------------------------------|
	/// |   |   |   |   |   | 1 | 0 | 1 | 0 | 1 | 0 | 1 | 0 | 1 | 0 | 1 |
	/// '-------------------|---|---------------------------------------'
	///                      \_/ \_____________________________________/
	///                      (2)                   (1)
	/// </code>
	/// Where (1) is for candidate offset value (from 0 to 728), and (2) is for the conclusion type (assignment or elimination).
	/// Please note that the part (2) only use one bit because the target value can only be assignment (0) or elimination (1),
	/// but the real type <see cref="ConclusionType"/> uses <see cref="byte"/> as its underlying numeric type
	/// because C# cannot set "A bit" to be the underlying type. The narrowest type is <see cref="byte"/>.
	/// </summary>
	private readonly Mask _mask = mask;


	/// <summary>
	/// Initializes an instance with a conclusion type and a candidate offset.
	/// </summary>
	/// <param name="type">The conclusion type.</param>
	/// <param name="candidate">The candidate offset.</param>
	public Conclusion(ConclusionType type, Candidate candidate) : this((Mask)((int)type * 729 + candidate))
	{
	}

	/// <summary>
	/// Initializes the <see cref="Conclusion"/> instance via the specified cell, digit and the conclusion type.
	/// </summary>
	/// <param name="type">The conclusion type.</param>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	public Conclusion(ConclusionType type, Cell cell, Digit digit) : this((Mask)((int)type * 729 + cell * 9 + digit))
	{
	}


	/// <summary>
	/// Indicates the cell the current instance uses.
	/// </summary>
	public Cell Cell => Candidate / 9;

	/// <summary>
	/// Indicates the digit the current instance uses.
	/// </summary>
	public Digit Digit => Candidate % 9;

	/// <summary>
	/// Indicates the candidate the current instance uses.
	/// </summary>
	public Candidate Candidate => _mask % 729;

	/// <summary>
	/// Indicates the conclusion type of the current instance.
	/// If the type is <see cref="Assignment"/>, this conclusion will be set value (Set a digit into a cell);
	/// otherwise, a candidate will be removed.
	/// </summary>
	public ConclusionType ConclusionType => (ConclusionType)(_mask / 729);


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out ConclusionType conclusionType, out Candidate candidate)
		=> (conclusionType, candidate) = (ConclusionType, Candidate);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out ConclusionType conclusionType, out Cell cell, out Digit digit)
		=> ((conclusionType, _), cell, digit) = (this, Cell, Digit);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Conclusion comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Conclusion other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc/>
	public int CompareTo(Conclusion other) => _mask.CompareTo(_mask);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter) => converter.ConclusionConverter([this]);

	/// <summary>
	/// Try to get a new <see cref="Conclusion"/> instance which is symmetric with the current instance, with the specified symmetric type.
	/// </summary>
	/// <param name="symmetricType">The symmetric type to be checked.</param>
	/// <param name="mappingDigit">The other mapping digit.</param>
	/// <returns>The other symmetric <see cref="Conclusion"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="symmetricType"/> contains multiple (greater than 2) cells
	/// symmetric with the current cell and digit.
	/// </exception>
	public Conclusion GetSymmetricConclusion(SymmetricType symmetricType, Digit mappingDigit)
		=> symmetricType.AxisDimension switch
		{
			0 or 1 => symmetricType.CellsInSymmetryAxis.Contains(Cell)
				? new(ConclusionType, Cell, mappingDigit == -1 ? Digit : mappingDigit)
				: new(ConclusionType, ((CellMap)symmetricType.GetOrbit(Cell) - Cell)[0], mappingDigit == -1 ? Digit : mappingDigit),
			_ => throw new ArgumentOutOfRangeException(nameof(symmetricType))
		};


	/// <summary>
	/// Try to parse the string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, out Conclusion result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, out Conclusion result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out Conclusion result)
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

	/// <summary>
	/// Parses the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The instance parsed.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Conclusion Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the specified string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The instance parsed.</returns>
	public static Conclusion Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));

	/// <summary>
	/// Parses the specified string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The instance parsed.</returns>
	public static Conclusion Parse(string s, CoordinateParser converter) => converter.ConclusionParser(s)[0];


	/// <summary>
	/// Negates the current conclusion instance, changing the conclusion type from <see cref="Assignment"/> to <see cref="Elimination"/>,
	/// or from <see cref="Elimination"/> to <see cref="Assignment"/>.
	/// </summary>
	/// <param name="self">The current conclusion instance to be negated.</param>
	/// <returns>The negation.</returns>
	public static Conclusion operator ~(Conclusion self) => new((ConclusionType)(1 & (byte)~self.ConclusionType), self.Candidate);

	/// <inheritdoc/>
	public static bool operator ==(Conclusion left, Conclusion right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Conclusion left, Conclusion right) => !(left == right);
}

/// <summary>
/// Represents JSON serialization rules on type <see cref="Conclusion"/>.
/// </summary>
file sealed class Converter : JsonConverter<Conclusion>
{
	/// <inheritdoc/>
	public override bool HandleNull => false;


	/// <inheritdoc/>
	public override Conclusion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> Conclusion.Parse(reader.GetString() ?? string.Empty);

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Conclusion value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.ToString());
}
