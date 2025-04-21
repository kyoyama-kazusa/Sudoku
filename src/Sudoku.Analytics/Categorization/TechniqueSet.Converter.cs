namespace Sudoku.Categorization;

public partial class TechniqueSet
{
	/// <summary>
	/// Defines a JSON converter for the current type.
	/// </summary>
	private sealed class Converter : JsonConverter<TechniqueSet>
	{
		/// <inheritdoc/>
		public override TechniqueSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var (result, isInitialized) = (TechniqueSets.None, false);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.StartArray when !isInitialized:
					{
						isInitialized = true;
						break;
					}
					case JsonTokenType.String when Technique.TryParse(reader.GetString(), out var technique):
					{
						result.Add(technique);
						break;
					}
					case JsonTokenType.EndArray:
					{
						return result;
					}
					default:
					{
						throw new JsonException();
					}
				}
			}
			throw new JsonException();
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, TechniqueSet value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var technique in value)
			{
				writer.WriteStringValue(technique.ToString());
			}
			writer.WriteEndArray();
		}
	}
}
