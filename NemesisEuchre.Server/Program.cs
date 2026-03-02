using NemesisEuchre.Server.DependencyInjection;
using NemesisEuchre.Server.Hubs;

namespace NemesisEuchre.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddNemesisEuchreServer(builder.Configuration);

        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hub/game");

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
