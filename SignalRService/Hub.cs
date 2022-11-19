using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Dawn;

namespace OLab.SignalRService.Api
{
    public partial class Hub : ServerlessHub
    {
        private static string GetEnvironmentVariable(string name, string defaultValue, ILogger logger)
        {
            string value;
            try
            {
                value = GetEnvironmentVariable(name, logger);
            }
            catch (System.ArgumentNullException)
            {
                return defaultValue;
            }

            return value;
        }

        private static string GetEnvironmentVariable(string name, ILogger logger)
        {
            Guard.Argument(name).NotEmpty(nameof(name));
            var variable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentNullException(name);

            return variable;
        }

        [FunctionName("index")]
        public IActionResult Index([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            Console.WriteLine(path);
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        {
            var accessToken = req.Headers["Authorization"];
            
            if ( string.IsNullOrEmpty(accessToken) )
                return new SignalRConnectionInfo();

            var claims = GetClaims(accessToken);
            var userName = claims.First(c => c.Type == "sub").Value;

            // ensure valid Auth0 user
            if (userName.Contains("auth0|") && userName.Length == 30)
            {
                var connectionInfo = Negotiate(
                    userName,
                    claims
                );

                return connectionInfo;
            }

            return new SignalRConnectionInfo();
        }

        [FunctionName(nameof(OnConnected))]
        public void OnConnected(
            [SignalRTrigger] InvocationContext invocationContext,
            ILogger logger,
            CancellationToken token
        )
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            logger.LogInformation($"OnConnected {invocationContext.ConnectionId}");
        }

        [FunctionName(nameof(OnDisconnected))]
        public void OnDisconnected(
            [SignalRTrigger] InvocationContext invocationContext,
            ILogger logger,
            CancellationToken token)
        {
            logger.LogInformation($"OnDisconnected {invocationContext.ConnectionId}");
        }

    }
}
