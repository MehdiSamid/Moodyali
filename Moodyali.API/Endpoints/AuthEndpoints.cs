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
    }
}
