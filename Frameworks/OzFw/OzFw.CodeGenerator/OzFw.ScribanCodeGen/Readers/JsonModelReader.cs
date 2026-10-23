using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>Ara modelin kendisi (docs/codegen-model.schema.json).</summary>
    public sealed class JsonModelReader : IModelReader
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public IReadOnlyList<string> Extensions { get; } = new[] { ".json" };

        public CodeGenModel Read(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<CodeGenModel>(content, Options)
                       ?? throw new ModelReadException("JSON boş.");
            }
            catch (JsonException ex)
            {
                throw new ModelReadException($"JSON okunamadı (satır {ex.LineNumber + 1}): {ex.Message}");
            }
        }

        public static string Write(CodeGenModel model) => JsonSerializer.Serialize(model, Options);
    }
}
