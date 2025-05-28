using System.ComponentModel.DataAnnotations;

namespace CryptoWalletApi.DTO
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Wrong Email adress.")]
        public string Email { get; set; } = string.Empty; // Initializes to avoid null warnings

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty; // Initializes to avoid null warnings
    }
}