using Dawn;
using Microsoft.AspNetCore.Http;
using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;

#nullable disable

namespace OLabWebAPI.Services;

public class WebApiUserContextService : UserContextService
{

  public WebApiUserContextService(
    IOLabLogger logger,
    HttpContext httpContext,
    OLabDBContext dbContext) : base( logger, dbContext )
  {
    Guard.Argument(httpContext).NotNull(nameof(httpContext));

    LoadHttpContext(httpContext);
  }

  protected virtual void LoadHttpContext(HttpContext hostContext)
  {
    IPAddress = hostContext.Connection.RemoteIpAddress.ToString();

    if (!hostContext.Items.TryGetValue("headers", out var headersObjects))
      throw new Exception("unable to retrieve headers from host context");
    Headers = (Dictionary<string, string>)headersObjects;

    if (!hostContext.Items.TryGetValue("claims", out var claimsObject))
      throw new Exception("unable to retrieve claims from host context");
    Claims = (IDictionary<string, string>)claimsObject;

    LoadContext();

  }
}

