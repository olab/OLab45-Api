using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OLabWebAPI.Services.TurkTalk.Venue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OLabWebAPI.Services.TurkTalk
{
    // [Route("olab/api/v3/turktalk")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public partial class TurkTalkHub : Hub
    {
        private readonly ILogger _logger;
        private readonly Conference _conference;

        /// <summary>
        /// TurkTalkHub constructor
        /// </summary>
        /// <param name="logger">Dependancy-injected logger</param>
        public TurkTalkHub(ILogger<TurkTalkHub> logger, Conference conference)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _conference = conference ?? throw new ArgumentNullException(nameof(conference));

            _logger.LogDebug($"TurkTalkHub ctor");
        }

        /// <summary>
        /// A connection was established with hubusing Microsoft.AspNetCore.SignalR;
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            try
            {
                _logger.LogDebug($"OnConnectedAsync: '{Context.ConnectionId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"OnConnectedAsync exception: {ex.Message}");
            }

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _logger.LogDebug($"OnDisconnectedAsync: '{Context.ConnectionId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"OnDisconnectedAsync exception: {ex.Message}");
            }

            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Broadcast message to all participants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void BroadcastMessage(string sender, string message)
        {
            try
            {
                _logger.LogDebug($"Broadcast message received from '{sender}': '{message}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"BroadcastMessage exception: {ex.Message}");
            }
        }

    }
}
