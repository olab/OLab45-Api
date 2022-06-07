using System.ComponentModel.DataAnnotations;

namespace OLabWebAPI.Model
{
    public class AddUserRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string EMail { get; set; }
        [Required]
        public string NickName { get; set; }
        public string NewPassword { get; set; }        
        [Required]
        public string Group { get; set; }
        [Required]
        public string Role { get; set; }
    }
}