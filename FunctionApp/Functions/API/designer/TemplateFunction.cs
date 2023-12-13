using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Endpoints.Designer;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Dtos.Designer;
using OLab.Data.Dtos.Maps;
using OLab.Data.Interface;
using OLab.Data.Models;
using OLab.FunctionApp.Extensions;
using OLab.FunctionApp.Functions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.API.designer
{
  public class TemplateFunction : OLabFunction
  {
    private readonly TemplateEndpoint _endpoint;

    public TemplateFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
        configuration,
        dbContext,
        wikiTagProvider,
        fileStorageProvider)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(wikiTagProvider).NotNull(nameof(wikiTagProvider));

      Logger = OLabLogger.CreateNew<TemplateFunction>(loggerFactory);
      _endpoint = new TemplateEndpoint(
        Logger,
        configuration,
        DbContext,
        wikiTagProvider,
        fileStorageProvider);
    }

    /// <summary>
    /// Gets map node
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("TemplateGetDesigner")]
    public async Task<HttpResponseData> TemplateGetDesignerAsync(
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
        var auth = GetAuthorization(hostContext);

        var pagedResult = await _endpoint.GetAsync(take, skip);
        Logger.LogInformation(string.Format("Found {0} files", pagedResult.Data.Count));

        response = request.CreateResponse(OLabObjectPagedListResult<MapsDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Get links for templates
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [Function("TemplateLinksGetDesigner")]
    public HttpResponseData TemplateLinksGetDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/links")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var dto = _endpoint.Links();
        response = request.CreateResponse(OLabObjectResult<MapNodeLinkTemplateDto>.Result(dto));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
        response = request.CreateResponse(ex);
      }

      return response;
    }

    /// <summary>
    /// Get template nodes
    /// </summary>
    /// <param name="id">Constant id</param>
    /// <returns></returns>
    [Function("TemplateMapNodeGetDesigner")]
    public HttpResponseData TemplateMapNodeDesignerAsync(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/nodes")] HttpRequestData request,
      FunctionContext hostContext)
    {
      try
      {
        Guard.Argument(request).NotNull(nameof(request));

        // validate token/setup up common properties
        var auth = GetAuthorization(hostContext);

        var dto = _endpoint.Nodes();
        response = request.CreateResponse(OLabObjectResult<MapNodeTemplateDto>.Result(dto));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
        response = request.CreateResponse(ex);
      }

      return response;
    }

  }
}
