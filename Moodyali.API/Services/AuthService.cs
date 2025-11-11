using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Moodyali.Core.Entities;
using Moodyali.Core.Services;
using Moodyali.Shared.DTOs.Auth;
using Moodyali.Shared.Helpers;
using Moodyali.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Moodyali.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        // Check if user or email already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
        {
            return null; // User already exists
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null; // Invalid credentials
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Expiration = DateTime.UtcNow.AddHours(1) // Token expiration time
        };
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Do not reveal if the user exists for security reasons.
            // In a real app, we would still send a mock email or log the attempt.
            return;
        }

        // --- Simple Mock Implementation ---
        // In a real application, this would involve:
        // 1. Generating a secure, time-limited token.
        // 2. Storing the token and its expiry time in the database (e.g., in the User entity or a separate table).
        // 3. Sending an email to the user's email address with a link containing the token.
        // 4. The link would point to a front-end page where the user can enter a new password and the token.
        // 5. A new API endpoint would handle the password reset with the token.

        // For this project, we will simply log a message to simulate the process.
        Console.WriteLine($"[FORGOT PASSWORD MOCK] Password reset process initiated for user: {user.Username} ({email}). A link would be sent now.");
        
        // We return without throwing an exception to allow the front-end to display the success message.
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
