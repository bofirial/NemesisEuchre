using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace NemesisEuchre.Server.Auth;

public interface IJwtTokenService
{
    string CreateToken(ClaimsPrincipal principal);
}

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string CreateToken(ClaimsPrincipal principal)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "NemesisEuchre";
        var audience = configuration["Jwt:Audience"] ?? "NemesisEuchre";
        var expiryHours = configuration.GetValue("Jwt:ExpiryHours", 24);
        var admins = configuration.GetSection("Auth:Admins").Get<string[]>() ?? [];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(principal.Claims);
        var login = principal.FindFirstValue(ClaimTypes.Name);
        var role = login is not null && admins.Contains(login, StringComparer.OrdinalIgnoreCase)
            ? "Admin"
            : "Player";
        claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
