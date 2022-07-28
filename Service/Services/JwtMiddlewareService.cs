using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Services
{
  public abstract class JwtMiddlewareService
  {
    protected readonly RequestDelegate _next;
    protected static string _jwtIssuer;
    protected static string _jwtAudience;
    protected static string _signingSecret;
    public static TokenValidationParameters _tokenParameters;

    protected abstract void AttachUserToContext(HttpContext httpContext,
                                     IUserService userService,
                                     string token);

    public JwtMiddlewareService(RequestDelegate next)
    {
      _next = next;
    }

    public static TokenValidationParameters GetValidationParameters()
    {
      return _tokenParameters;
    }

    protected static void SetupConfiguration(IConfiguration config)
    {
      _jwtIssuer = config["AppSettings:Issuer"];
      _jwtAudience = config["AppSettings:Audience"];
      _signingSecret = config["AppSettings:Secret"];

      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(_signingSecret[..16]));

      _tokenParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = _jwtIssuer,

        ValidateAudience = true,
        ValidAudience = _jwtAudience,

        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,

      };

      _tokenParameters.IssuerSigningKey = securityKey;

    }

    protected static void SetupServices(IServiceCollection services, TokenValidationParameters parameters)
    {
      services.AddAuthentication(x =>
      {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(x =>
      {
        x.RequireHttpsMetadata = true;
        x.SaveToken = true;
        x.TokenValidationParameters = parameters;
      });
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
      var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

      if (token != null)
        AttachUserToContext(context,
                            userService,
                            token);

      await _next(context);
    }
  }
}