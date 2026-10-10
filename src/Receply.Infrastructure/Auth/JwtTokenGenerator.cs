using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Receply.Application.Auth;
using Receply.Domain.Platform;
using Receply.Domain.Tenancy;

namespace Receply.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(12);
    private static readonly TimeSpan PlatformAdminTokenLifetime = TimeSpan.FromHours(24);

    public string GenerateToken(Staff staff)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new Claim("tenant_id", staff.TenantId.ToString()),
            new Claim("staff_id", staff.Id.ToString()),
            new Claim(ClaimTypes.Role, staff.Role.ToString())
        };

        return WriteToken(claims, TokenLifetime);
    }

    // Deliberately no tenant_id claim - TenantResolutionMiddleware leaves ITenantContext unset for
    // this token, which makes the EF global query filter a no-op (see ReceplyDbContext), so a
    // platform admin's queries naturally span every tenant instead of just one.
    public string GenerateToken(PlatformAdmin admin)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim("platform_admin_id", admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "PlatformAdmin")
        };

        return WriteToken(claims, PlatformAdminTokenLifetime);
    }

    private string WriteToken(Claim[] claims, TimeSpan lifetime)
    {
        var jwt = options.Value;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
