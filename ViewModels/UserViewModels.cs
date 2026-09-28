using System.ComponentModel.DataAnnotations;

namespace HumanBirthPredictionSystem.ViewModels
{
    public class CreateUserViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "System Role")]
        public string Role { get; set; } = "User"; // "Admin" or "User"
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "New Password (leave blank to keep current)")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [Display(Name = "System Role")]
        public string Role { get; set; } = "User";
    }
}
