using System.ComponentModel.DataAnnotations;

namespace DocumentFrontend.Models
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Email must be between 4 and 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(15, ErrorMessage = "Password must be at least 15 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z]).+$", ErrorMessage = "Password must contain at least one uppercase and one lowercase letter.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
