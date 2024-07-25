using System.Text.Json.Serialization;

namespace Web.OpenId;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = default!;

    [JsonPropertyName("refresh_expires")]
    public string RefreshExpires { get; init; } = default!;

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = default!;

    [JsonPropertyName("id_token")]
    public string IdToken { get; init; } = default!;
}