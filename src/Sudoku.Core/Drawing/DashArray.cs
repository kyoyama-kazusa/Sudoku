namespace Sudoku.Drawing;

/// <summary>
/// Defines a dash array of <see cref="double"/> values. The values can be used in UI as dash array of a double collection.
/// </summary>
[JsonConverter(typeof(Converter))]
public readonly partial struct DashArray() : IEnumerable<double>, IEquatable<DashArray>, IEqualityOperators<DashArray, DashArray, bool>
{
	/// <summary>
	/// Indicates the invalid value.
	/// </summary>
	public static readonly DashArray InvalidValue = [0];


	/// <summary>
	/// The double values.
	/// </summary>
	private readonly List<double> _doubles = [];


	/// <summary>
	/// Indicates the number of values.
	/// </summary>
	[JsonIgnore]
	public int Count => _doubles.Count;


	/// <summary>
	/// Adds a new value into the collection.
	/// </summary>
	/// <param name="value">The value.</param>
	public void Add(double value) => _doubles.Add(value);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is DashArray comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(DashArray other) => _doubles.SequenceEqual(other._doubles);

	/// <summary>
	/// Converts the current collection into an array of <see cref="double"/> values.
	/// </summary>
	/// <returns>An array of <see cref="double"/> values.</returns>
	public double[] ToArray() => [.. _doubles];

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode()
	{
		var result = default(HashCode);
		foreach (var element in _doubles)
		{
			result.Add(element);
		}
		return result.ToHashCode();
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"[{string.Join(", ", _doubles)}]";

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(_doubles);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _doubles.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<double> IEnumerable<double>.GetEnumerator() => _doubles.AsEnumerable().GetEnumerator();


	/// <inheritdoc/>
	public static bool operator ==(DashArray left, DashArray right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(DashArray left, DashArray right) => !(left == right);
}

/// <summary>
/// Represents JSON serialization rules on type <see cref="DashArray"/>.
/// </summary>
file sealed class Converter : JsonConverter<DashArray>
{
	/// <inheritdoc/>
	public override DashArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert != typeof(DashArray) || reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException();
		}

		var targetCollection = new List<double>();
	loop_readerRead:
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number when options.NumberHandling == JsonNumberHandling.Strict:
				{
					targetCollection.Add(reader.GetDouble());
					break;
				}
				case JsonTokenType.String when options.NumberHandling is var o && o.HasFlag(JsonNumberHandling.AllowReadingFromString):
				{
					if (reader.TryGetDouble(out var value))
					{
						targetCollection.Add(value);
						break;
					}

					bool optionPredicate() => o.HasFlag(JsonNumberHandling.AllowNamedFloatingPointLiterals);
					targetCollection.Add(
						reader.GetString() switch
						{
							nameof(double.NaN) when optionPredicate() => double.NaN,
							"Infinity" when optionPredicate() => double.PositiveInfinity,
							"-Infinity" when optionPredicate() => double.NegativeInfinity,
							{ } s => double.Parse(s),
							_ => throw new JsonException()
						}
					);

					break;
				}
				case JsonTokenType.EndArray:
				{
					break loop_readerRead;
				}
				default:
				{
					throw new JsonException();
				}
			}
		}

		return [.. targetCollection];
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, DashArray value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var element in value)
		{
			switch (options)
			{
				case { NumberHandling: var o } when o.HasFlag(JsonNumberHandling.WriteAsString):
				{
					writer.WriteStringValue(element.ToString());
					break;
				}
				case { NumberHandling: var o } when o.HasFlag(JsonNumberHandling.AllowNamedFloatingPointLiterals):
				{
					writer.WriteStringValue(
						element switch
						{
							double.NaN => nameof(double.NaN),
							double.NegativeInfinity => "-Infinity",
							double.PositiveInfinity => "Infinity",
							_ => element.ToString()
						}
					);
					break;
				}
				default:
				{
					writer.WriteNumberValue(element);
					break;
				}
			}
		}

		writer.WriteEndArray();
	}
}
