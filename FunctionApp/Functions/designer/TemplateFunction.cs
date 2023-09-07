using Dawn;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Data.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Dto.Designer;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Model;

namespace OLab.FunctionApp.Functions.Designer
{
  public class TemplateFunction : OLabFunction
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplateFunction(
      ILoggerFactory loggerFactory,
      IConfiguration configuration,
      IUserService userService,
      OLabDBContext dbContext) : base(loggerFactory, configuration, userService, dbContext)
    {
      _endpoint = new TemplateEndpoint(Logger, appSettings, DbContext);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("TemplateGetDesigner")]
    public async Task<IActionResult> TemplateGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates")] HttpRequestData request,
      FunctionContext hostContext,
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
        var auth = GetRequestContext(hostContext);

        var pagedResult = await _endpoint.GetAsync(take, skip);
        Logger.LogInformation(string.Format("Found {0} files", pagedResult.Data.Count));

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
    [Function("TemplateLinksGetDesigner")]
    public IActionResult TemplateLinksGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/links")] HttpRequest request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
    [Function("TemplateMapNodeDesigner")]
    public IActionResult TemplateMapNodeDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/nodes")] HttpRequest request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetRequestContext(hostContext);

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
