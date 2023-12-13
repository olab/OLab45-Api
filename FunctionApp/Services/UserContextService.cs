using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using OLab.FunctionApp.Extensions;
using OLab.Data.Interface;

#nullable disable

namespace OLab.FunctionApp.Services
{
  public class UserContextService : IUserContext
  {
    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;
    public const string NonAccessAcl = "-";
    public ClaimsPrincipal User;
    public Users OLabUser;

    protected IDictionary<string, string> _claims;
    protected readonly IOLabLogger _logger;
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

    protected string _sessionId;
    protected string _role;
    public IList<string> UserRoles { get; set; }
    protected uint _userId;
    protected string _userName;
    protected string _ipAddress;
    protected string _issuer;
    protected string _referringCourse;
    protected string _accessToken;

    public string SessionId
    {
      get => _sessionId;
      set => _sessionId = value;
    }

    public string ReferringCourse
    {
      get => _referringCourse;
      set => _referringCourse = value;
    }

    public string Role
    {
      get => _role;
      set => _role = value;
    }

    public uint UserId
    {
      get => _userId;
      set => _userId = value;
    }

    public string UserName
    {
      get => _userName;
      set => _userName = value;
    }

    public string IPAddress
    {
      get => _ipAddress;
      set => _ipAddress = value;
    }

    public string Issuer
    {
      get => _issuer;
      set => _issuer = value;
    }

    public string CourseName { get { return null; } }

    // default ctor, needed for services Dependancy Injection
    public UserContextService()
    {

    }

    public UserContextService(
      IOLabLogger logger,
      FunctionContext hostContext)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(hostContext).NotNull(nameof(hostContext));

      _logger = logger;
      _logger.LogInformation($"UserContext ctor");

      LoadHostContext(hostContext);
    }

    private string GetRequestIpAddress(HttpRequestData req)
    {
      try
      {
        var headerDictionary = req.Headers.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        var key = "x-forwarded-for";

        if (headerDictionary.ContainsKey(key))
        {
          var headerValues = headerDictionary[key];
          var ipn = headerValues?.FirstOrDefault()?.Split(new char[] { ',' }).FirstOrDefault()?.Split(new char[] { ':' }).FirstOrDefault();

          _logger.LogInformation($"found ip address: {ipn}");

          return ipn;
        }

      }
      catch (Exception)
      {
        // eat all exceptions
      }

      return "<unknown>";
    }

    protected void LoadHostContext(FunctionContext hostContext)
    {
      if (!hostContext.Items.TryGetValue("headers", out var headersObjects))
        throw new Exception("unable to retrieve headers from host context");

      var headers = (Dictionary<string, string>)headersObjects;

      if (headers.TryGetValue("OLabSessionId".ToLower(), out var sessionId))
        if (!string.IsNullOrEmpty(sessionId) && sessionId != "null")
        {
          SessionId = sessionId;
          if (!string.IsNullOrWhiteSpace(SessionId))
            _logger.LogInformation($"Found sessionId '{SessionId}'.");
        }

      if (!hostContext.Items.TryGetValue("claims", out var claimsObject))
        throw new Exception("unable to retrieve claims from host context");

      _claims = (IDictionary<string, string>)claimsObject;

      if (!_claims.TryGetValue(ClaimTypes.Name, out var nameValue))
        throw new Exception("unable to retrieve user name from token claims");

      var req = hostContext.GetHttpRequestData();
      IPAddress = GetRequestIpAddress(req);

      UserName = nameValue;

      ReferringCourse = _claims[ClaimTypes.UserData];

      if (!_claims.TryGetValue("iss", out var issValue))
        throw new Exception("unable to retrieve iss from token claims");
      Issuer = issValue;

      if (!_claims.TryGetValue("id", out var idValue))
        throw new Exception("unable to retrieve user id from token claims");
      UserId = (uint)Convert.ToInt32(idValue);

      if (!_claims.TryGetValue(ClaimTypes.Role, out var roleValue))
        throw new Exception("unable to retrieve role from token claims");
      Role = roleValue;

      // separate out multiple roles, make lower case, remove spaces, and sort
      UserRoles = Role.Split(',')
        .Select(x => x.Trim())
        .Select(x => x.ToLower())
        .OrderBy(x => x)
        .ToList();
    }

    public override string ToString()
    {
      return $"{UserId} {Issuer} {UserName} {Role} {IPAddress} {ReferringCourse}";
    }

  }
}

