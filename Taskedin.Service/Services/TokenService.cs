using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Taskedin.Domain.Entities;
using Taskedin.Service.Interfaces;
using Taskedin.Service.Security;

namespace Taskedin.Service.Services;

// Issues signed JWT access tokens from the "Jwt" configuration section.
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Signs a short-lived JWT carrying the user's identity and role claims.
    public (string AccessToken, DateTime ExpiresAt) CreateToken(
        User user, IEnumerable<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var signingKey = jwtSection["SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. " +
                "Set 'Jwt:SigningKey' via user secrets or environment variables.");
        }

        var issuer = jwtSection["Issuer"] ?? "Taskedin";
        var audience = jwtSection["Audience"] ?? "Taskedin";
        var lifetimeMinutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var minutes)
            && minutes > 0
            ? minutes
            : 60;

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, user.Id.ToString()),
            new(JwtClaimTypes.Name, user.FullName),
            new(JwtClaimTypes.Email, user.Email),
            new(JwtClaimTypes.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
