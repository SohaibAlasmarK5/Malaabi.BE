using System.ComponentModel.DataAnnotations;

namespace Mal3abi.Core.Resources   // ← wherever you decide this lives
{
    public class RegisterRequestResource
    {
        [Required, MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequestResource
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResource
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "player"; // "player" | "owner"
    }

    public class AuthResponseResource
    {
        public string Token { get; set; } = string.Empty;
        public UserResource User { get; set; } = null!;
    }
}
