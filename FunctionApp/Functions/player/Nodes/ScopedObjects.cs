using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using OLabWebAPI.Common;
using OLabWebAPI.Common.Exceptions;
using OLabWebAPI.Dto;
using System;
using System.Threading.Tasks;

namespace OLab.Endpoints.Azure.Player
{
  public partial class NodesFunction : OLabFunction
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [FunctionName("MapNodeScopedObjectsRawGet")]
    public async Task<IActionResult> MapScopedObjectsRawGetAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "nodes/{nodeId}/scopedobjects/raw")] HttpRequest request,
      uint nodeId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
        return OLabObjectResult<ScopedObjectsDto>.Result(dto);
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
    [FunctionName("MapNodeScopedObjectsGet")]
    public async Task<IActionResult> MapScopedObjectsGetAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "nodes/{nodeId}/scopedobjects")] HttpRequest request,
      uint nodeId)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
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
