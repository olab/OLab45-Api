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
using OLabWebAPI.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using OLabWebAPI.Endpoints.Designer;
using OLabWebAPI.Dto.Designer;

namespace OLab.Endpoints.Azure.Designer
{
  public class TemplateFunction : OLabFunction
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplateFunction(
      IUserService userService,
      ILogger<ConstantsFunction> logger,
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
    [FunctionName("TemplateGetDesigner")]
    public async Task<IActionResult> TemplateGetDesignerAsync(
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

        // validate token/setup up common properties
        AuthorizeRequest(request);

        var pagedResult = await _endpoint.GetAsync(take, skip);
        logger.LogInformation(string.Format("Found {0} files", pagedResult.Data.Count));

        return OLabObjectPagedListResult<MapsDto>.Result(pagedResult.Data, pagedResult.Remaining);
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
    [FunctionName("TemplateLinksGetDesigner")]
    public IActionResult TemplateLinksGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "templates/links")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

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
    [FunctionName("TemplateMapNodeDesigner")]
    public IActionResult TemplateMapNodeDesignerAsync(
      [HttpTrigger(AuthorizationLevel.User, "get", Route = "templates/nodes")] HttpRequest request
    )
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        AuthorizeRequest(request);

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
