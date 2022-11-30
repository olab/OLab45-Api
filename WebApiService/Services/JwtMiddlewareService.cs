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
                ValidateIssuer = false,
                ValidIssuer = _jwtIssuer,
                ValidateIssuerSigningKey = true,

                ValidateAudience = true,
                ValidAudience = _jwtAudience,

                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero,

                // validate against existing security key
                IssuerSigningKey = securityKey
            };

        }

        protected static void SetupServices(IServiceCollection services, TokenValidationParameters parameters)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = parameters;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
              {
                    var accessToken = context.Request.Query["access_token"];
                    if (string.IsNullOrEmpty(accessToken))
                        accessToken = context.Request.Headers["Authorization"];

                  // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/turktalk")))
                    {
                      // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
                };
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