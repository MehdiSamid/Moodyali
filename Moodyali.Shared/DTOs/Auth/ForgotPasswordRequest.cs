using System.ComponentModel.DataAnnotations;

namespace Moodyali.Shared.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
