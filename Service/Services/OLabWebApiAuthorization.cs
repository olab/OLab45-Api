using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Dto;
using OLabWebAPI.Interface;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services
{
  class OLabWebApiAuthorization : IOlabAuthentication
  {
    private readonly OLabLogger logger;
    private readonly OLabDBContext context;
    private readonly HttpContext httpContext;

    public OLabWebApiAuthorization(OLabLogger logger, OLabDBContext context, HttpContext httpContext)
    {
      this.logger = logger;
      this.context = context;
      this.httpContext = httpContext;
    }

    public IActionResult HasAccess(ScopedObjectDto dto)
    {
      // test if user has access to write to parent.
      var userContext = new UserContext(logger, context, httpContext);
      if (dto.ImageableType == Constants.ScopeLevelMap)
      {
        if (!userContext.HasAccess("W", Constants.ScopeLevelMap, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }
      if (dto.ImageableType == Constants.ScopeLevelServer)
      {
        if (!userContext.HasAccess("W", Constants.ScopeLevelServer, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }
      if (dto.ImageableType == Constants.ScopeLevelNode)
      {
        if (!userContext.HasAccess("W", Constants.ScopeLevelNode, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }

      return new NoContentResult();
    }
  }
}