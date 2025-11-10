using Moodyali.Core.Entities;
using Moodyali.Shared.DTOs.Auth;

namespace Moodyali.Core.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    string GenerateJwtToken(User user);
}
