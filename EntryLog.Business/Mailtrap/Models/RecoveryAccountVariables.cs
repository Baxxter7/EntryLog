using System.Text.Json.Serialization;

namespace EntryLog.Business.Mailtrap.Models;

internal class RecoveryAccountVariables
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
