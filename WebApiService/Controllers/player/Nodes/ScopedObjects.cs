using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Services;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  public partial class NodesController : OlabController
  {
    [HttpGet("{nodeId}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint nodeId)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    [HttpGet("{nodeId}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint nodeId)
    {
      try
      {
        var auth = new OLabAuthorization(logger, dbContext, HttpContext);
        ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    public async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      try
      {
        ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }
  }
}