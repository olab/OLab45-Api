using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLabWebAPI.Extensions;
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
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  [HttpGet("{nodeId}/scopedobjects")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetScopedObjectsAsync(uint nodeId)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  public async Task<IActionResult> GetScopedObjectsAsync(
    uint id,
    bool enableWikiTranslation)
  {
    try
    {
      var dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
      return HttpContext.Request.CreateResponse(OLabObjectResult<ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }
}