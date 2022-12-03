using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace OLabWebAPI.Services
{
    public class OLabJWTService : JwtMiddlewareService
    {
        private static SymmetricSecurityKey _securityKey;

        public OLabJWTService(RequestDelegate next) : base(next)
        {
        }

        public static void Setup(IConfiguration config, IServiceCollection services)
        {
            _securityKey =
              new SymmetricSecurityKey(Encoding.Default.GetBytes(config["AppSettings:Secret"][..16]));
            SetupConfiguration(config);

            SetupServices(services, GetValidationParameters());
        }

        protected override void AttachUserToContext(HttpContext httpContext,
                                                    IUserService userService,
                                                    string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token,
                                           GetValidationParameters(),
                                           out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                string userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub").Value;
                string nickname = jwtToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
                string role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value;

                httpContext.Items["Role"] = role;
                httpContext.Items["User"] = userName;
                httpContext.Items["UserId"] = nickname;

                if (string.IsNullOrEmpty(role))
                {
                    // attach user to context on successful jwt validation
                    Model.Users user = userService.GetByUserName(userName);
                    httpContext.Items["User"] = user.Username;
                    httpContext.Items["Role"] = $"{user.Role}";
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}