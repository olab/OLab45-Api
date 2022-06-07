using System.ComponentModel.DataAnnotations;

namespace OLabWebAPI.Model
{
    public class AddUserResponse
    {
        public string Username { get; set; }
        public string Password { get; set; }        
    }
}