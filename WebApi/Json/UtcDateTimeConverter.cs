using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi.Json;

/// <summary>
/// Serializa TODAS las fechas como UTC con sufijo 'Z' (ISO 8601). Los timestamps
/// se guardan con DateTime.UtcNow, pero al leerlos de SQL Server vuelven con
/// Kind=Unspecified y se serializaban SIN 'Z', por lo que el navegador los
/// interpretaba como hora local (+6h en Guatemala). Con 'Z', `new Date(...)`
/// del frontend obtiene el instante correcto y lo convierte bien a hora local.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)   // el valor guardado ES UTC
            : value.ToUniversalTime();
        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

/// <summary>Igual que arriba pero para DateTime? (nullable).</summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) { writer.WriteNullValue(); return; }
        var v = value.Value;
        var utc = v.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(v, DateTimeKind.Utc)
            : v.ToUniversalTime();
        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
