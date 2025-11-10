//using Moodyali.Core;
using Moodyali.Shared;
namespace Moodyali.Shared.Helpers;

public static class MoodHelper
{
    public static int GetScoreFromEmoji(string emoji)
    {
        return emoji switch
        {
            "😢" => 1, // 0-2 range, picking 1
            "🙁" => 3, // 3-4 range, picking 3
            "😐" => 5, // 5 range, picking 5
            "🙂" => 7, // 6-7 range, picking 7
            "😄" => 9, // 8-10 range, picking 9
            _ => throw new ArgumentException($"Unknown emoji: {emoji}")
        };
    }

    public static MoodEmoji GetMoodEmojiType(int score)
    {
        return score switch
        {
            >= 8 => MoodEmoji.Happy, // 😄 (8-10)
            >= 6 => MoodEmoji.Smile, // 🙂 (6-7)
            5 => MoodEmoji.Neutral, // 😐 (5)
            >= 3 => MoodEmoji.Frown, // 🙁 (3-4)
            _ => MoodEmoji.Sad // 😢 (0-2)
        };
    }
}
