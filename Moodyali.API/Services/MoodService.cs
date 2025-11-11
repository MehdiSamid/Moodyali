using Microsoft.EntityFrameworkCore;
using Moodyali.API.Data;
using Moodyali.Core.Entities;
using Moodyali.Core.Services;
using Moodyali.Shared.DTOs.Mood;
using Moodyali.Shared.Helpers;
using Moodyali.Shared;
namespace Moodyali.API.Services;

public class MoodService : IMoodService
{
    private readonly ApplicationDbContext _context;

    public MoodService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Mood?> LogMoodAsync(int userId, LogMoodRequest request)
    {
        var today = DateTime.UtcNow.Date;
        var existingMood = await _context.Moods
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Date.Date == today);

        var score = MoodHelper.GetScoreFromEmoji(request.Emoji);

        if (existingMood != null)
        {
            // Update existing mood
            existingMood.Emoji = request.Emoji;
            existingMood.Score = score;
            existingMood.Note = request.Note;
            _context.Moods.Update(existingMood);
            await _context.SaveChangesAsync();
            return existingMood;
        }
        else
        {
            // Create new mood
            var newMood = new Mood
            {
                UserId = userId,
                Date = today,
                Emoji = request.Emoji,
                Score = score,
                Note = request.Note
            };
            _context.Moods.Add(newMood);
            await _context.SaveChangesAsync();
            return newMood;
        }
    }

    public async Task<MoodResponse?> GetTodayMoodAsync(int userId)
    {
        var today = DateTime.UtcNow.Date;
        var mood = await _context.Moods
            .Where(m => m.UserId == userId && m.Date.Date == today)
            .Select(m => new MoodResponse
            {
                Emoji = m.Emoji,
                Score = m.Score,
                Date = m.Date,
                Note = m.Note
            })
            .FirstOrDefaultAsync();

        return mood;
    }

    public async Task<List<MoodResponse>> GetLastSevenDaysMoodsAsync(int userId)
    {
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);

        var moods = await _context.Moods
            .Where(m => m.UserId == userId && m.Date.Date >= sevenDaysAgo)
            .OrderByDescending(m => m.Date)
            .Select(m => new MoodResponse
            {
                Emoji = m.Emoji,
                Score = m.Score,
                Date = m.Date,
                Note = m.Note
            })
            .ToListAsync();

        // Fill in missing days with a default/null mood for the last 7 days
        var result = new List<MoodResponse>();
        for (int i = 0; i < 7; i++)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var mood = moods.FirstOrDefault(m => m.Date.Date == date);
            if (mood != null)
            {
                result.Add(mood);
            }
            else
            {
                result.Add(new MoodResponse { Date = date, Emoji = "❓", Score = 0, Note = null });
            }
        }

        return result.OrderBy(m => m.Date).ToList();
    }

    public async Task<MoodStatsResponse> GetMoodStatsAsync(int userId)
    {
        var allMoods = await _context.Moods
            .Where(m => m.UserId == userId)
            .ToListAsync();

        if (!allMoods.Any())
        {
            return new MoodStatsResponse { AverageScore = 0, HappyDays = 0, SadDays = 0 };
        }

        var totalScore = allMoods.Sum(m => m.Score);
        var averageScore = totalScore / (double)allMoods.Count;

        //var happyDays = allMoods.Count(m => MoodHelper.GetMoodEmojiType(m.Score) == Core.MoodEmoji.Happy || MoodHelper.GetMoodEmojiType(m.Score) == Core.MoodEmoji.Smile);
        //var sadDays = allMoods.Count(m => MoodHelper.GetMoodEmojiType(m.Score) == Core.MoodEmoji.Sad || MoodHelper.GetMoodEmojiType(m.Score) == Core.MoodEmoji.Frown);
        var happyDays = allMoods.Count(m => MoodHelper.GetMoodEmojiType(m.Score) == Shared.MoodEmoji.Happy || MoodHelper.GetMoodEmojiType(m.Score) == Shared.MoodEmoji.Smile);
        var sadDays = allMoods.Count(m => MoodHelper.GetMoodEmojiType(m.Score) == Shared.MoodEmoji.Sad || MoodHelper.GetMoodEmojiType(m.Score) == Shared.MoodEmoji.Frown);

        return new MoodStatsResponse
        {
            AverageScore = Math.Round(averageScore, 2),
            HappyDays = happyDays,
            SadDays = sadDays
        };
    }
}
