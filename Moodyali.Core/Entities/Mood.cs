using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moodyali.Core.Entities;

public class Mood
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Emoji { get; set; } = string.Empty; // Store the actual emoji string (e.g., "🙂")

    [Required]
    public int Score { get; set; } // Score from 0 to 10

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
