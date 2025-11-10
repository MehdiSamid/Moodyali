namespace Moodyali.Shared.DTOs.Mood;
using Moodyali.Shared;
public class MoodStatsResponse
{
    public double AverageScore { get; set; }
    public int HappyDays { get; set; }
    public int SadDays { get; set; }
}
