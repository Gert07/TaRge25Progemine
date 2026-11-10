using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.Account
{
    public class RegisterVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]

        public string ConfirmPassword { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}