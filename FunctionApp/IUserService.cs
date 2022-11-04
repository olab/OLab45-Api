using System.Collections.Generic;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLab.FunctionApp.Api
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(LoginRequest model);
        AuthenticateResponse AuthenticateExternal(ExternalLoginRequest model);
        void ChangePassword(Users user, ChangePasswordRequest model);
        
        void AddUser( Users newUser );
        IEnumerable<Users> GetAll();
        Users GetById(int id);
        Users GetByUserName(string userName);
    }
}