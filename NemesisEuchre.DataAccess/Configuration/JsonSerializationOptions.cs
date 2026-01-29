using System.Text.Json;
using System.Text.Json.Serialization;

namespace NemesisEuchre.DataAccess.Configuration;

public static class JsonSerializationOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false,
    };
}
