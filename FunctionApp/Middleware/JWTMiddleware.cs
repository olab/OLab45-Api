using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OLab.Api.Utils;
using System.Text;
using Configuration = OLab.Api.FunctionApp.Functions.Configuration;

namespace OLab.Api.FunctionApp.Middleware
{
  public abstract class JWTMiddleware : IFunctionsWorkerMiddleware
  {
    protected static Configuration Config;
    protected static OLabLogger Logger;
    protected static TokenValidationParameters TokenValidation;

    public JWTMiddleware(IConfiguration configuration)
    {
      Guard.Argument(configuration).NotNull(nameof(configuration));

      Config = new Configuration(configuration);
      TokenValidation = BuildTokenValidation();
    }

    public abstract Task Invoke(FunctionContext context, FunctionExecutionDelegate next);

    /// <summary>
    /// Builds token validation object
    /// </summary>
    /// <param name="configuration">App configuration</param>
    private static TokenValidationParameters BuildTokenValidation()
    {
      try
      {

        // get and extract the valid token issuers
        var jwtIssuers = Config.GetValue<string>("Issuer");

        var issuerParts = jwtIssuers.Split(',');
        var validIssuers = issuerParts.Select(x => x.Trim()).ToList();

        var jwtAudience = Config.GetValue<string>("Audience");
        var signingSecret = Config.GetValue<string>("Secret");

        var securityKey =
          new SymmetricSecurityKey(Encoding.Default.GetBytes(signingSecret[..40]));

        TokenValidation = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidIssuers = validIssuers,
          ValidateIssuerSigningKey = true,

          ValidateAudience = true,
          ValidAudience = jwtAudience,

          // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
          ClockSkew = TimeSpan.Zero,

          // validate against existing security key
          IssuerSigningKey = securityKey
        };

        return TokenValidation;
      }
      catch (Exception ex)
      {
        Logger.LogError(ex.Message);
        throw;
      }

    }
  }
}
