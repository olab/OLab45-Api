using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{

    [Route("olab/api/v3/filescontent")]
    public partial class FileContentController : OlabController
    {

        public IConfiguration Configuration { get; }
        public static int defaultTokenExpiryMinutes = 1;

        public FileContentController(IConfiguration configuration, ILogger<NodesController> logger, OLabDBContext context) : base(logger, context)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("sign/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetFileContentAccessTokenAsync(uint id)
        {
            SystemFiles phys = await dbContext.SystemFiles.FirstOrDefaultAsync(x => x.Id == id);
            if (phys == null)
                return new NotFoundResult();

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyByteArray = Encoding.ASCII.GetBytes(OLabConfiguration.SIGNING_SECRET);
            var signingKey =
              new SymmetricSecurityKey(Encoding.Default.GetBytes(this.Configuration["AppSetting:Secret"][..16]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.UserData, phys.Path)
                }),

                Expires = DateTime.UtcNow.AddMinutes(defaultTokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            var response = new LoginResponseDto();

            response.AuthInfo.Token = tokenHandler.WriteToken(token);
            response.Group = "";
            response.Role = "";
            response.UserName = "";
            response.AuthInfo.Created = DateTime.UtcNow;
            response.AuthInfo.Expires = response.AuthInfo.Created.AddMinutes(defaultTokenExpiryMinutes);

            return new JsonResult(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetFileContentAsync(uint id)
        {
            logger.LogDebug($"FileContentController.GetFileContentAsync(uint id={id})");

            SystemFiles phys = await dbContext.SystemFiles.FirstOrDefaultAsync(x => x.Id == id);
            if (phys == null)
                return new NotFoundResult();

            var filesRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(filesRoot, "files");
            filePath = Path.Combine(filePath, phys.ImageableType, phys.ImageableId.ToString());
            filePath = Path.Combine(filePath, phys.Path);

            logger.LogDebug($"file generated '{filePath}')");

            if (!System.IO.File.Exists(filePath))
            {
                logger.LogDebug($"file not found");
                return new NoContentResult();
            }

            var byteArray = await System.IO.File.ReadAllBytesAsync(filePath);
            logger.LogDebug($"file size '{byteArray.Count()}')");
            return new FileContentResult(byteArray, phys.Mime);
        }
    }
}