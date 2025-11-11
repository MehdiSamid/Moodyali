namespace Moodyali.Shared.DTOs.Mood;
using Moodyali.Shared;

public class MoodResponse
{
    public string Emoji { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
}
