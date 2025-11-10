using Moodyali.Core.Entities;
using Moodyali.Shared.DTOs.Mood;
using Moodyali.Shared;

namespace Moodyali.Core.Services;

public interface IMoodService
{
    Task<Mood?> LogMoodAsync(int userId, LogMoodRequest request);
    Task<MoodResponse?> GetTodayMoodAsync(int userId);
    Task<List<MoodResponse>> GetLastSevenDaysMoodsAsync(int userId);
    Task<MoodStatsResponse> GetMoodStatsAsync(int userId);
}
