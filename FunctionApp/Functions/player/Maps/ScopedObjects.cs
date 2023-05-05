using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OLab.FunctionApp.Api;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using OLabWebAPI.Endpoints;
using OLab.FunctionApp.Api.Services;
using OLabWebAPI.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OLabWebAPI.Endpoints.Player;
using OLabWebAPI.Utils;

namespace OLab.Endpoints.Azure.Player
{
  public partial class MapsFunction : OLabFunction
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [FunctionName("MapScopedObjectsRawGet")]
    public async Task<IActionResult> MapScopedObjectsRawAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects/raw")] HttpRequest request,
      uint id)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsRawAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [FunctionName("MapScopedObjectsGet")]
    public async Task<IActionResult> MapScopedObjectsPutAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "maps/{id}/scopedobjects")] HttpRequest request,
      uint id)
    {
      try
      {
        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsAsync(auth, id);
        return OLabObjectResult<OLabWebAPI.Dto.ScopedObjectsDto>.Result(dto);
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
