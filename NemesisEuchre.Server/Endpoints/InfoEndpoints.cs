namespace NemesisEuchre.Server.Endpoints;

public static class InfoEndpoints
{
    public static void MapInfoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/version", () => Results.Ok(new { version = ThisAssembly.AssemblyInformationalVersion }));
    }
}
