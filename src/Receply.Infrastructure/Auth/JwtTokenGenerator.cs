using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Receply.Application.Auth;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(12);

    public string GenerateToken(Staff staff)
    {
        var jwt = options.Value;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new Claim("tenant_id", staff.TenantId.ToString()),
            new Claim("staff_id", staff.Id.ToString()),
            new Claim(ClaimTypes.Role, staff.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TokenLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
