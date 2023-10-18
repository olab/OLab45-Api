using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

public partial class NodesController : OLabController
{
  [HttpGet("{nodeId}/scopedobjects/raw")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetScopedObjectsRawAsync(uint nodeId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }

  [HttpGet("{nodeId}/scopedobjects")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetScopedObjectsAsync(uint nodeId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);

      ScopedObjectsDto dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
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
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }
}