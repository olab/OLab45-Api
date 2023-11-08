using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player;

public partial class MapsController : OLabController
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}/scopedobjects/raw")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }

  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}/scopedobjects")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetScopedObjectsAsync(uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetAuthorization(HttpContext);

      var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
      return HttpContext.Request.CreateResponse(OLabObjectResult<OLab.Api.Dto.ScopedObjectsDto>.Result(dto));
    }
    catch (Exception ex)
    {
      if (ex is OLabUnauthorizedException)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result(ex.Message));
      return HttpContext.Request.CreateResponse(OLabServerErrorResult.Result(ex.Message));
    }
  }

}
