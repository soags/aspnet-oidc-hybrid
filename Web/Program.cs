using NLog;
using NLog.Web;
using Web.OpenId;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();

    builder.Services.AddControllers();

    // セッションを使用
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromDays(1);
        options.Cookie.HttpOnly = true;
    });

    // OpenIdConnectの設定を取得
    var openIdConnectConfig = builder.Configuration.GetSection(OpenIdConnectOptions.OpenIdConnect);
    builder.Services.Configure<OpenIdConnectOptions>(openIdConnectConfig);

    // OpenIdConnectSignInManagerをDI登録
    builder.Services.AddScoped<OpenIdConnectSignInManager>();

    var app = builder.Build();

    // HTTPSが必要
    app.UseHttpsRedirection();

    // HTMLファイル等のホスト
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // セッションを使用
    app.UseSession();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stoppped program because of exception.");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}