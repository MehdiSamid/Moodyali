using System.ComponentModel.DataAnnotations;
using Moodyali.Shared;

namespace Moodyali.Shared.DTOs.Mood;

public class LogMoodRequest
{
    [Required]
    public string Emoji { get; set; } = string.Empty;
}
