namespace Sudoku.Descriptors;

/// <summary>
/// Represents a descriptor that describes for a color that will be used in drawing.
/// There're 3 possible kinds of colors:
/// <list type="number">
/// <item>
/// <b>Alias</b>: Define an enumeration field of type <see cref="ColorDescriptorAlias"/>
/// to describe an item should be colored as the specified kind of items predefined in the coloring system.
/// </item>
/// <item>
/// <b>ID</b>: Define an integer value indicating the desired ID defined in a palette (a global color pool)
/// predefined in the coloring system.
/// </item>
/// <item><b>ARGB</b>: Define a quadruple of bytes indicating alpha, red, green and blue values as an ARGB color.</item>
/// </list>
/// </summary>
/// <param name="mask">The 64-bit signed integer as a mask.</param>
/// <remarks>
/// The type uses 5 of 8 bytes, 34 of 64 bits.
/// </remarks>
/// <seealso cref="ColorDescriptorAlias"/>
[JsonConverter(typeof(Converter))]
[Union]
public readonly struct ColorDescriptor(long mask) :
	IEquatable<ColorDescriptor>,
	IEqualityOperators<ColorDescriptor, ColorDescriptor, bool>,
	IUnion
{
	/// <summary>
	/// Indicates the shift bits amount.
	/// </summary>
	private const int TypeShift = 32;


	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via the specified an integer ID.
	/// </summary>
	/// <param name="id">The ID value.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ColorDescriptor(int id) : this((long)ColorDescriptorType.Id << TypeShift | (long)id)
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via ARGB values.
	/// </summary>
	/// <param name="value">The value.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ColorDescriptor((byte A, byte R, byte G, byte B) value) :
		// Explicitly cast into <see cref="uint"/> to prevent negative-bit extension.
		this((long)ColorDescriptorType.Argb << TypeShift | (uint)(value.A << 24 | value.R << 16 | value.G << 8 | value.B))
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via well-known item.
	/// </summary>
	/// <param name="item">The well-known item.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ColorDescriptor(ColorDescriptorAlias item) : this((long)ColorDescriptorType.Alias << TypeShift | (long)item)
	{
	}


	/// <inheritdoc/>
	public object? Value
		=> this switch
		{
			int v => v,
			ValueTuple<byte, byte, byte, byte> v => v,
			ColorDescriptorAlias v => v,
			_ => null
		};

	/// <summary>
	/// Indicates alpha value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Alpha => (byte)(ArgbMask >>> 24 & 255);

	/// <summary>
	/// Indicates red value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Red => (byte)(ArgbMask >>> 16 & 255);

	/// <summary>
	/// Indicates green value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Green => (byte)(ArgbMask >>> 8 & 255);

	/// <summary>
	/// Indicates blue value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Blue => (byte)(ArgbMask & 255);

	/// <summary>
	/// Indicates an integer that describes the palette ID that a user has chosen.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Id"/> but no exceptions thrown.
	/// </summary>
	public int Id => (int)ValueMask;

	/// <summary>
	/// Indicates an integer that represents ARGB values.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public uint ArgbMask => (uint)(Mask & uint.MaxValue);

	/// <summary>
	/// Indicates the mask that only represents color data.
	/// </summary>
	public long ValueMask => Mask & (1L << TypeShift) - 1;

	/// <summary>
	/// Indicates the type of the color identifier.
	/// </summary>
	public ColorDescriptorType Type => (ColorDescriptorType)(Mask >>> TypeShift);

	/// <summary>
	/// Indicates an aliased value that directly points to an item that you want to color it to.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Alias"/> but no exceptions thrown.
	/// </summary>
	public ColorDescriptorAlias AliasedItem => (ColorDescriptorAlias)ValueMask;

	/// <summary>
	/// Indicates the whole 64-bit mask.
	/// </summary>
	public long Mask { get; } = mask;


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
	public bool TryGetValue(out int value)
	{
		if (Type == ColorDescriptorType.Id)
		{
			value = Id;
			return true;
		}
		value = default;
		return false;
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
	public bool TryGetValue(out (byte A, byte R, byte G, byte B) value)
	{
		if (Type == ColorDescriptorType.Argb)
		{
			value = (Alpha, Red, Green, Blue);
			return true;
		}
		value = default;
		return false;
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
	public bool TryGetValue(out ColorDescriptorAlias value)
	{
		if (Type == ColorDescriptorType.Alias)
		{
			value = AliasedItem;
			return true;
		}
		value = default;
		return false;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorDescriptor comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(ColorDescriptor other) => Mask == other.Mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => Mask.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> this switch
		{
			int v => v.ToString(),
			ValueTuple<byte, byte, byte, byte> v => v.ToString(),
			ColorDescriptorAlias v => v.ToString(),
			_ => string.Empty
		};


	/// <inheritdoc/>
	public static bool operator ==(ColorDescriptor left, ColorDescriptor right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(ColorDescriptor left, ColorDescriptor right) => !(left == right);
}

/// <summary>
/// Represents a JSON converter that can serialize and deserialize with instances of this type.
/// </summary>
file sealed class Converter : JsonConverter<ColorDescriptor>
{
	/// <summary>
	/// Indicates whether we ignore case parsing on enumeration fields.
	/// </summary>
	public bool IgnoreCase { get; init; } = true;


	/// <inheritdoc/>
	public override ColorDescriptor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.StartArray:
			{
				var argb = (stackalloc byte[4]);
				var index = 0;
				reader.Read();
				while (reader.TokenType != JsonTokenType.EndArray)
				{
					if (index >= 4)
					{
						throw new JsonException("Expected array of length 4 for bytes variant.");
					}

					if (reader.TokenType != JsonTokenType.Number || !reader.TryGetByte(out var value))
					{
						throw new JsonException("Bytes array must contain numbers 0..255.");
					}

					argb[index++] = value;
					reader.Read();
				}
				if (index != 4)
				{
					throw new JsonException("Expected exactly 4 bytes.");
				}

				return (argb[0], argb[1], argb[2], argb[3]);
			}

			case JsonTokenType.Number:
			{
				if (!reader.TryGetInt32(out var id))
				{
					throw new JsonException("Invalid integer value.");
				}
				return id;
			}

			case JsonTokenType.String:
			{
				var s = reader.GetString();
				if (ColorDescriptorAlias.TryParse(s, IgnoreCase, out var e))
				{
					return e;
				}
				if (int.TryParse(s, out var numeric))
				{
					return (ColorDescriptorAlias)numeric;
				}
				throw new JsonException($"Invalid enum value: '{s}'.");
			}

			default:
			{
				throw new JsonException($"Unexpected JSON token for this type: {reader.TokenType}.");
			}
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, ColorDescriptor value, JsonSerializerOptions options)
	{
		switch (value.Type)
		{
			case ColorDescriptorType.Argb:
			{
				writer.WriteStartArray();
				writer.WriteNumberValue(value.Alpha);
				writer.WriteNumberValue(value.Red);
				writer.WriteNumberValue(value.Green);
				writer.WriteNumberValue(value.Blue);
				writer.WriteEndArray();
				break;
			}

			case ColorDescriptorType.Id:
			{
				writer.WriteNumberValue(value.Id);
				break;
			}

			case ColorDescriptorType.Alias:
			{
				writer.WriteStringValue(value.AliasedItem.ToString());
				break;
			}

			default:
			{
				throw new JsonException("Invalid format.");
			}
		}
	}
}
