namespace Web.OpenId;

public class OpenIdConnectOptions
{
    public const string OpenIdConnect = "OpenIdConnect";

    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string AuthorizationEndpoint { get; set; } = default!;
    public string TokenEndpoint { get; set; } = default!;
    public string UserInfoEndpoint { get; set; } = default!;
    public string JwksUri { get; set; } = default!;
}
