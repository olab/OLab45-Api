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
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Dto.Designer;

namespace OLab.Endpoints.Azure.Designer
{
  public class TemplateAzureEndpoint : OLabAzureEndpoint
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplateAzureEndpoint(
      IUserService userService,
      ILogger<ConstantsAzureEndpoint> logger,
      OLabDBContext context) : base(logger, userService, context)
    {
      Guard.Argument(userService).NotNull(nameof(userService));
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(context).NotNull(nameof(context));

      _endpoint = new TemplateEndpoint(this.logger, context);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName("GetTemplateDesigner")]
    public async Task<IActionResult> GetTemplateAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "templates")] HttpRequest request,
        CancellationToken cancellationToken)
    {

      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        var queryTake = Convert.ToInt32(request.Query["take"]);
        var querySkip = Convert.ToInt32(request.Query["skip"]);
        int? take = queryTake > 0 ? queryTake : null;
        int? skip = querySkip > 0 ? querySkip : null;

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        var pagedResponse = await _endpoint.GetAsync(take, skip);
        return OLabObjectPagedListResult<MapsDto>.Result(pagedResponse.Data, pagedResponse.Remaining);
      }
      catch (Exception ex)
      {
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);

        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get links for templates
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [FunctionName("GetTemplateLinksDesigner")]
    public IActionResult GetLinksAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "templates/links")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        var dto = _endpoint.Links();
        return OLabObjectResult<MapNodeLinkTemplateDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

    /// <summary>
    /// Get template nodes
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [FunctionName("GetTemplateNodesDesigner")]
    public IActionResult GetTemplateNodesAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "templates/nodes")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate user access token.  throws if not successful
        userService.ValidateToken(request);

        var auth = new OLabWebApiAuthorization(logger, context, request);
        var dto = _endpoint.Nodes();
        return OLabObjectResult<MapNodeTemplateDto>.Result(dto);
      }
      catch (Exception ex)
      {
        if (ex is OLabObjectNotFoundException)
          return OLabNotFoundResult<string>.Result(ex.Message);
        if (ex is OLabUnauthorizedException)
          return OLabUnauthorizedObjectResult<string>.Result(ex.Message);
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

  }
}
