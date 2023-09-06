using Microsoft.Azure.Functions.Worker;
using OLab.Data;
using OLab.Model;
using OLab.Utils;
using System.Security.Claims;

#nullable disable

namespace OLab.FunctionApp.Services
{
  public class FunctionAppUserContext : UserContext
  {
    //public const string WildCardObjectType = "*";
    //public const uint WildCardObjectId = 0;
    //public const string NonAccessAcl = "-";
    public ClaimsPrincipal Principal;
    //public Users OLabUser;

    //private readonly OLabDBContext _dbContext;
    private readonly FunctionContext _hostContext;

    // default ctor, needed for services Dependancy Injection
    public FunctionAppUserContext()
    {

    }

    public FunctionAppUserContext(
      OLabLogger logger,
      OLabDBContext dbContext,
      FunctionContext hostContext) : base(logger, dbContext)
    {
      _hostContext = hostContext;

      Session = new OLabSession(_logger.GetLogger(), dbContext, this);

      LoadHostContext();
    }

    protected override void LoadHostContext()
    {
      var headers = new Dictionary<string, string>();
      if (!_hostContext.Items.TryGetValue("headers", out var headersObjects))
        throw new Exception("unable to retrieve headers from host context");

      headers = (Dictionary<string, string>)headersObjects;

      if (headers.TryGetValue("OLabSessionId", out var sessionId))
      {
        if (!string.IsNullOrEmpty(sessionId) && sessionId != "null")
        {
          Session.SetSessionId(sessionId);
          if (!string.IsNullOrWhiteSpace(Session.GetSessionId()))
            _logger.LogInformation($"Found sessionId {Session.GetSessionId()}.");
        }
      }

      if (!_hostContext.Items.TryGetValue("claims", out object claimsObject))
        throw new Exception("unable to retrieve claims from host context");

      _claims = (IDictionary<string, string>)claimsObject;

      if (!_claims.TryGetValue(ClaimTypes.Name, out var nameValue))
        throw new Exception("unable to retrieve user name from token claims");
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
      _roles = Role.Split(',')
        .Select(x => x.Trim())
        .Select(x => x.ToLower())
        .OrderBy(x => x)
        .ToList();

      _roleAcls = _dbContext.SecurityRoles.Where(x => _roles.Contains(x.Name.ToLower())).ToList();

      if (headers.TryGetValue("x-forwarded-for", out _ipAddress))
        _logger.LogInformation($"ipaddress: {_ipAddress}");
      else
        _logger.LogWarning($"no ipaddress detected");

      // test for a local user
      var user = _dbContext.Users.FirstOrDefault(x => x.Username == UserName && x.Id == UserId);
      if (user != null)
      {
        _logger.LogInformation($"Local user '{UserName}' found");

        OLabUser = user;
        UserId = user.Id;
        _userAcls = _dbContext.SecurityUsers.Where(x => x.UserId == UserId).ToList();

        // if user is anonymous user, add user access to anon-flagged maps
        if (OLabUser.Group == "anonymous")
        {
          var anonymousMaps = _dbContext.Maps.Where(x => x.SecurityId == 1).ToList();
          foreach (var item in anonymousMaps)
            _userAcls.Add(new SecurityUsers
            {
              Id = item.Id,
              ImageableId = item.Id,
              ImageableType = Constants.ScopeLevelMap,
              Acl = "RX"
            });
        }
      }
    }
  }
}

