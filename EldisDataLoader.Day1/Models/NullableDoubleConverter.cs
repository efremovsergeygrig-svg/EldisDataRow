using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EldisDataLoader.Models;

public class NullableDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str) || str.Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            // 🛡️ ПРОВЕРЕННЫЙ ВАРИАНТ: Используем ru-RU. 
            // Он корректно читает и запятую (как десятичный разделитель), и точку.
            if (double.TryParse(str, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out var result))
                return result;

            return null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}