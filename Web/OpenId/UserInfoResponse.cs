using System.Text.Json.Serialization;

namespace Web.OpenId;

public class UserInfoResponse
{
    [JsonPropertyName("sub")]
    public string Sub { get; init; } = default!;

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("preferred_username")]
    public string PreferredUsername { get; init; } = default!;

    [JsonPropertyName("given_name")]
    public string GivenName { get; init; } = default!;

    [JsonPropertyName("family_name")]
    public string FamilyName { get; init; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; init; } = default!;
}