using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzFwBaseHelpers
{
    /// <summary>
    /// JSON'da "" olarak gelen Guid? değerlerini null'a çevirir (System.Text.Json).
    /// Kayıt: services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableGuidConverter()));
    /// </summary>
    public class EmptyStringToNullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                    return null;
                if (Guid.TryParse(text, out var guid))
                    return guid;
            }

            throw new JsonException($"Geçersiz Guid değeri.");
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
