using Moodyali.Shared.DTOs.Mood;

namespace Moodyali.Core.Services;

public interface IRecommendationService
{
    Task<string> GetRecommendationAsync(List<MoodResponse> weeklyMoods);
}
