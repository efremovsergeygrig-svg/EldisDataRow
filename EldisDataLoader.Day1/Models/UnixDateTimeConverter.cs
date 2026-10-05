using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class UnixDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            var timestamp = reader.GetInt64();
            if (timestamp > 1e12)
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            else
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            return DateTime.TryParse(str, out var dt) ? dt : null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(new DateTimeOffset(value.Value).ToUnixTimeSeconds());
        else
            writer.WriteNullValue();
    }
}