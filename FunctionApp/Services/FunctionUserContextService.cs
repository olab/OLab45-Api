using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Extensions;
using System.Security.Claims;

#nullable disable

namespace OLab.FunctionApp.Services;

public class FunctionUserContextService : UserContextService
{
  // default ctor, needed for services Dependancy Injection
  public FunctionUserContextService()
  {

  }

  public FunctionUserContextService(
    IOLabLogger logger,
    FunctionContext hostContext,
    OLabDBContext dbContext) : base( logger, dbContext )
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(hostContext).NotNull(nameof(hostContext));

    GetLogger().LogInformation($"UserContext ctor");

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

        GetLogger().LogInformation($"found ip address: {ipn}");

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
    var req = hostContext.GetHttpRequestData();
    IPAddress = GetRequestIpAddress(req);

    if (!hostContext.Items.TryGetValue("headers", out var headersObjects))
      throw new Exception("unable to retrieve headers from host context");
    Headers = (Dictionary<string, string>)headersObjects;

    if (!hostContext.Items.TryGetValue("claims", out var claimsObject))
      throw new Exception("unable to retrieve claims from host context");
    Claims = (IDictionary<string, string>)claimsObject;

    LoadContext();

  }

}

