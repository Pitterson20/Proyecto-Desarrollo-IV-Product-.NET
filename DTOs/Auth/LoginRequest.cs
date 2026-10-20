using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ferreteria.DTOs.Auth;

public sealed class LoginRequest
{
    public string? Email { get; set; }

    public string? Password { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
