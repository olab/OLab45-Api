using System.ComponentModel.DataAnnotations;

namespace OLabWebAPI.Model
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}