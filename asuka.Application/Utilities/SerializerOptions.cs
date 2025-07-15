using System.Text.Json;

namespace asuka.Application.Utilities;

internal static class SerializerOptions
{
    internal static JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
}
