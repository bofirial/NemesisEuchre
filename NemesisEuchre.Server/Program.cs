using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;

using NemesisEuchre.Server.Auth;

namespace NemesisEuchre.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var secretKey = builder.Configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey must be configured. Run: dotnet user-secrets set \"Jwt:SecretKey\" \"<random-key>\"");
        var issuer = builder.Configuration["Jwt:Issuer"] ?? "NemesisEuchre";
        var audience = builder.Configuration["Jwt:Audience"] ?? "NemesisEuchre";

        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
            })
            .AddOAuth("GitHub", options =>
            {
                options.ClientId = builder.Configuration["GitHub:ClientId"] ?? string.Empty;
                options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? string.Empty;
                options.CallbackPath = "/signin-github";
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                        context.RunClaimActions(user.RootElement);
                    },
                    OnTicketReceived = context =>
                    {
                        var tokenService = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
                        var jwt = tokenService.CreateToken(context.Principal!);
                        context.Response.Redirect($"/#token={Uri.EscapeDataString(jwt)}");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoints();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
