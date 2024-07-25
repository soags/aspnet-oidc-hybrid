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

    // �Z�b�V�������g�p
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromDays(1);
        options.Cookie.HttpOnly = true;
    });

    // OpenIdConnect�̐ݒ���擾
    var openIdConnectConfig = builder.Configuration.GetSection(OpenIdConnectOptions.OpenIdConnect);
    builder.Services.Configure<OpenIdConnectOptions>(openIdConnectConfig);

    // OpenIdConnectSignInManager��DI�o�^
    builder.Services.AddScoped<OpenIdConnectSignInManager>();

    var app = builder.Build();

    // HTTPS���K�v
    app.UseHttpsRedirection();

    // HTML�t�@�C�����̃z�X�g
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // �Z�b�V�������g�p
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