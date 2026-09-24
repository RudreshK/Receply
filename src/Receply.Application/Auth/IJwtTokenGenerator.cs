using Receply.Domain.Tenancy;

namespace Receply.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(Staff staff);
}
