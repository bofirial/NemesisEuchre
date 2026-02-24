using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;

using NemesisEuchre.Server.Auth;
using NemesisEuchre.Server.Services;

namespace NemesisEuchre.Server.DependencyInjection;

public static class ServerServiceCollectionExtensions
{
    public static IServiceCollection AddNemesisEuchreServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey must be configured. Run: dotnet user-secrets set \"Jwt:SecretKey\" \"<random-key>\"");
        var issuer = configuration["Jwt:Issuer"] ?? "NemesisEuchre";
        var audience = configuration["Jwt:Audience"] ?? "NemesisEuchre";

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IBotStorageService, BotStorageService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            })
            .AddOAuth("GitHub", options =>
            {
                options.ClientId = configuration["GitHub:ClientId"] ?? string.Empty;
                options.ClientSecret = configuration["GitHub:ClientSecret"] ?? string.Empty;
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

        services.AddSignalR();
        services.AddAuthorization();

        return services;
    }
}
