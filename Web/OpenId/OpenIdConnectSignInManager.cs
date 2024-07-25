using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using RestSharp.Authenticators;

namespace Web.OpenId;

public class OpenIdConnectSignInManager
{
    private readonly OpenIdConnectOptions _options;
    private readonly RestClient _client;

    public OpenIdConnectSignInManager(IOptions<OpenIdConnectOptions> options)
    {
        _options = options.Value;
        _client = new RestClient(new RestClientOptions(_options.Issuer));
    }

    /// <summary>
    /// 認可コードから認証トークンを取得します。
    /// </summary>
    /// <param name="authCode"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<TokenResponse> GetToken(string authCode)
    {
        var request = new RestRequest(_options.TokenEndpoint, Method.Post)
        {
            // クライアント秘密鍵で認証
            Authenticator = new HttpBasicAuthenticator(_options.ClientId, _options.ClientSecret),
        };

        // 認可コードを送信してアクセストークンを取得
        request.AddParameter("grant_type", "authorization_code");　
        request.AddParameter("code", authCode);
        request.AddParameter("redirect_uri", _options.RedirectUri);
        
        var response = await _client.ExecuteAsync<TokenResponse>(request);
        if (response.IsSuccessful && response.Data is not null)
        {
            return response.Data;
        }

        // TODO: エラー処理
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }        
        throw new Exception();
    }

    /// <summary>
    /// アクセストークンのリフレッシュを行います。
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task<TokenResponse> RefreshToken(string refreshToken)
    {
        var request = new RestRequest(_options.TokenEndpoint, Method.Post)
        {
            // クライアント秘密鍵で認証
            Authenticator = new HttpBasicAuthenticator(_options.ClientId, _options.ClientSecret),
        };

        // リフレッシュトークンを送信してアクセストークンを取得
        request.AddParameter("grant_type", "refresh_token");
        request.AddParameter("refresh_token", refreshToken);

        var response = await _client.ExecuteAsync<TokenResponse>(request);
        if (response.IsSuccessful && response.Data is not null)
        {
            return response.Data;
        }

        // TODO: エラー処理
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }        
        throw new Exception();
    }

    /// <summary>
    /// ユーザー情報を取得します。
    /// </summary>
    /// <param name="accessToken">アクセストークン。</param>
    /// <returns>取得したユーザー情報。</returns>
    /// <exception cref="Exception">ユーザー情報の取得に失敗した場合にスローされます。</exception>
    public async Task<UserInfoResponse> GetUserInfo(string accessToken)
    {
        var request = new RestRequest(_options.UserInfoEndpoint, Method.Get)
        {
            // アクセストークン認証
            Authenticator = new JwtAuthenticator(accessToken)
        };

        // ユーザー情報を取得
        var response = await _client.ExecuteAsync<UserInfoResponse>(request);
        if (response.IsSuccessful && response.Data is not null)
        {
            return response.Data;
        }

        // TODO: エラー処理
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }        
        throw new Exception();
    }

    /// <summary>
    /// アクセストークンの検証を行います。
    /// </summary>
    /// <param name="accessToken">検証するアクセストークン。</param>
    /// <returns>トークンが有効であれば true を、そうでなければ false を返します。</returns>
    public Task<bool> VerifyAccessToken(string accessToken)
    {
        return VerifyToken(accessToken, accessToken);
    }

    /// <summary>
    /// トークンの検証を行います。
    /// アクセストークンとIDトークンの検証を行うことができます。
    /// </summary>
    /// <param name="accessToken">アクセストークン。JWKSを取得するために使用されます。</param>
    /// <param name="targetToken">検証するトークン（IDトークンまたはアクセストークン）。</param>
    /// <returns>トークンが有効であれば true を、そうでなければ false を返します。</returns>
    /// <exception cref="Exception">トークンの検証中に例外が発生した場合にスローされます。</exception>
    private async Task<bool> VerifyToken(string accessToken, string targetToken)
    {
        // JWKSエンドポイントからキー情報を取得
        var jwks = await GetJwks(accessToken);

        // RS256署名アルゴリズムを使用するキーを選択（Keycloakのデフォルト署名がRS256）
        var rs256Key = jwks.Keys.FirstOrDefault(x => x.Alg == "RS256");
        if (rs256Key == null)
        {
            throw new Exception("適切な署名キーが見つかりません。");
        }

        // トークン検証設定
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rs256Key,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            // トークン検証
            var principal = tokenHandler.ValidateToken(
                targetToken,
                validationParameters,
                out var validatedToken);

            // トークンの有効期限を検証
            return validatedToken.ValidTo > DateTime.UtcNow;
        }
        catch (SecurityTokenException ex)
        {
            throw new Exception("トークンの検証中にエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// JWKSを取得します。
    /// JWKSは、トークン署名に使用される公開鍵を含んでいます。
    /// </summary>
    /// <param name="accessToken">アクセストークン。</param>
    /// <returns>取得した JsonWebKeySet オブジェクト。</returns>
    private async Task<JsonWebKeySet> GetJwks(string accessToken)
    {
        var request = new RestRequest(_options.JwksUri, Method.Get)
        {
            // アクセストークン認証
            Authenticator = new JwtAuthenticator(accessToken)
        };

        // JWKSを取得
        var response = await _client.ExecuteAsync(request);
        var jwks = new JsonWebKeySet(response.Content);
        return jwks;
    }
}
