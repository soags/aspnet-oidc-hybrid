using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Web.Models;
using Web.OpenId;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly OpenIdConnectSignInManager _openIdConnectSignInManager;
    private readonly OpenIdConnectOptions _openIdConnectOptions;

    public AuthController(OpenIdConnectSignInManager openIdConnectSignInManager, IOptions<OpenIdConnectOptions> openIdConnectOptions)
    {
        _openIdConnectSignInManager = openIdConnectSignInManager;
        _openIdConnectOptions = openIdConnectOptions.Value;
    }

    [HttpGet("login")]
    public IActionResult RedirectLogin()
    {
        // 認可エンドポイントにリダイレクト
        var uriBuilder = new UriBuilder(_openIdConnectOptions.AuthorizationEndpoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["client_id"] = _openIdConnectOptions.ClientId;
        query["redirect_uri"] = _openIdConnectOptions.RedirectUri;
        query["response_type"] = "code";
        query["scope"] = "openid";
        uriBuilder.Query = query.ToString();

        var redirectUrl = uriBuilder.Uri.ToString();
        return Redirect(redirectUrl);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            // 認可コードから認証トークンを取得
            var tokenResponse = await _openIdConnectSignInManager.GetToken(request.Code);

            // 普通はトークンをクライアントに渡すトークンベース認証を行うが
            // 既存プロジェクトはセッションベースに強依存してるので、
            // クライアント側はセッションベース認証のまま、リフレッシュトークンをセッションに格納する
            // ※ セッションとトークンでライフタイムが異なるので注意
            HttpContext.Session.SetString("AccessToken", tokenResponse.AccessToken);
            HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);
            HttpContext.Session.SetString("IdToken", tokenResponse.IdToken);

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            // セッションを破棄
            HttpContext.Session.Clear();
            return Unauthorized();
        }
        catch
        {
            HttpContext.Session.Clear();
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("userInfo")]
    public async Task<IActionResult> UserInfo()
    {
        try
        {
            // セッションにトークンがない場合は認証されていない
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            if (accessToken is null || refreshToken is null)
            {
                // セッションを破棄
                HttpContext.Session.Clear();
                return Unauthorized();
            }

            // アクセストークンを検証
            if (!await _openIdConnectSignInManager.VerifyAccessToken(accessToken))
            {
                // アクセストークンのリフレッシュ
                var tokenResponse = await _openIdConnectSignInManager.RefreshToken(refreshToken);

                // 取得したアクセストークンを使用
                accessToken = tokenResponse.AccessToken;

                // 取得したトークンをセッションに格納
                HttpContext.Session.SetString("AccessToken", tokenResponse.AccessToken);
                HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);
                HttpContext.Session.SetString("IdToken", tokenResponse.IdToken);
            }

            // ユーザー情報を取得
            var userInfoResponse = await _openIdConnectSignInManager.GetUserInfo(accessToken);
            return Ok(userInfoResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            // セッションを破棄
            HttpContext.Session.Clear();
            return Unauthorized();
        }
        catch
        {
            HttpContext.Session.Clear();
            return new StatusCodeResult(500);
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // セッションを破棄
        HttpContext.Session.Clear();
        return Ok();
    }
}
