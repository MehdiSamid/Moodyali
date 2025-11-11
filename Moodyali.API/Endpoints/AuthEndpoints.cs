using Microsoft.AspNetCore.Mvc;
using Moodyali.Core.Services;
using Moodyali.Shared.DTOs.Auth;

namespace Moodyali.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            [FromBody] RegisterRequest request,
            IAuthService authService) =>
        {
            var user = await authService.RegisterAsync(request);

            if (user == null)
            {
                return Results.Conflict("Username or email already exists.");
            }

            return Results.Ok(new { message = "Registration successful." });
        })
        .WithName("Register")
        .AllowAnonymous();

        app.MapPost("/auth/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request);

            if (response == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(response);
        })
        .WithName("Login")
        .AllowAnonymous();

        app.MapPost("/auth/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            IAuthService authService) =>
        {
            // The service handles the logic and avoids revealing if the email exists.
            await authService.ForgotPasswordAsync(request.Email);

            // Always return a success message to prevent user enumeration
            return Results.Ok(new { message = "If an account with this email exists, a password reset link has been sent." });
        })
        .WithName("ForgotPassword")
        .AllowAnonymous();
    }
}
