using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Common;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using IOLabAuthentication = OLabWebAPI.Data.Interface.IOLabAuthentication;

namespace OLab.FunctionApp.Api.Services
{
  public class OLabAuthorization : IOLabAuthentication
  {
    private readonly OLabLogger logger;
    private readonly OLabDBContext context;
    private readonly HttpRequest request;
    private IUserContext userContext;

    public OLabAuthorization(
      OLabLogger logger, 
      OLabDBContext context, 
      HttpRequest request
    )
    {
      this.logger = logger;
      this.context = context;
      this.request = request;

      userContext = new UserContext(logger, context, request);
    }

    public IUserContext GetUserContext()
    {
      return userContext;
    }

    public IActionResult HasAccess(string acl, ScopedObjectDto dto)
    {
      // test if user has access to write to parent.
      if (dto.ImageableType == Constants.ScopeLevelMap)
      {
        if (!HasAccess("W", Constants.ScopeLevelMap, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }
      if (dto.ImageableType == Constants.ScopeLevelServer)
      {
        if (!HasAccess("W", Constants.ScopeLevelServer, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }
      if (dto.ImageableType == Constants.ScopeLevelNode)
      {
        if (!HasAccess("W", Constants.ScopeLevelNode, dto.ImageableId))
          return OLabUnauthorizedResult.Result();
      }

      return new NoContentResult();
    }

    public bool HasAccess(string acl, string objectType, uint? objectId)
    {
      return userContext.HasAccess( acl, objectType, objectId );
    }
  }
}