using System.Text.Json;

namespace InterlockLedger.Rest.Client
{
    internal static class Globals
    {
        public static readonly JsonSerializerOptions JsonSettings = new() {
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

    }
}
