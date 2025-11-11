using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Moodyali.Core.Services;

namespace Moodyali.API.Endpoints;

public static class RecommendationEndpoints
{
    public static void MapRecommendationEndpoints(this IEndpointRouteBuilder app)
    {
        var recommendationGroup = app.MapGroup("/recommendation")
            .RequireAuthorization();

        recommendationGroup.MapGet("/", async (
            ClaimsPrincipal user,
            IMoodService moodService,
            IRecommendationService recommendationService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            // 1. Get the last 7 days of mood data
            var weeklyMoods = await moodService.GetLastSevenDaysMoodsAsync(userId);

            // 2. Get the recommendation from the AI service
            var recommendation = await recommendationService.GetRecommendationAsync(weeklyMoods);

            return Results.Ok(new { recommendation });
        })
        .WithName("GetRecommendation");
    }
}
