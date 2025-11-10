using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moodyali.Core.Services;
using Moodyali.Shared.DTOs.Mood;

namespace Moodyali.API.Endpoints;

public static class MoodEndpoints
{
    public static void MapMoodEndpoints(this IEndpointRouteBuilder app)
    {
        var moodGroup = app.MapGroup("/mood")
            .RequireAuthorization();

        moodGroup.MapPost("/", async (
            [FromBody] LogMoodRequest request,
            ClaimsPrincipal user,
            IMoodService moodService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var mood = await moodService.LogMoodAsync(userId, request);
                return Results.Created($"/mood/{mood?.Id}", new MoodResponse
                {
                    Emoji = mood!.Emoji,
                    Score = mood.Score,
                    Date = mood.Date
                });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("LogMood");

        moodGroup.MapGet("/today", async (
            ClaimsPrincipal user,
            IMoodService moodService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var mood = await moodService.GetTodayMoodAsync(userId);
            return mood != null ? Results.Ok(mood) : Results.NotFound("No mood logged today.");
        })
        .WithName("GetTodayMood");

        moodGroup.MapGet("/week", async (
            ClaimsPrincipal user,
            IMoodService moodService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var moods = await moodService.GetLastSevenDaysMoodsAsync(userId);
            return Results.Ok(moods);
        })
        .WithName("GetWeeklyMoods");

        moodGroup.MapGet("/stats", async (
            ClaimsPrincipal user,
            IMoodService moodService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var stats = await moodService.GetMoodStatsAsync(userId);
            return Results.Ok(stats);
        })
        .WithName("GetMoodStats");
    }
}
