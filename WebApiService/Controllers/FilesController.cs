using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OLab.Api.Common;
using OLab.Api.Common.Exceptions;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.ObjectMapper;
using OLab.Api.Endpoints;
using OLab.Api.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using Dawn;

namespace OLabWebAPI.Endpoints.WebApi.Player;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
  public void OnResourceExecuting(ResourceExecutingContext context)
  {
    System.Collections.Generic.IList<IValueProviderFactory> factories = context.ValueProviderFactories;
    factories.RemoveType<FormValueProviderFactory>();
    factories.RemoveType<JQueryFormValueProviderFactory>();
  }

  public void OnResourceExecuted(ResourceExecutedContext context)
  {
  }
}

[Route("olab/api/v3/files")]
[ApiController]
public partial class FilesController : OLabController
{
  private readonly FilesEndpoint _endpoint;

  public FilesController(ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  IUserService userService,
  OLabDBContext dbContext,
  IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
  IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
    configuration,
    userService,
    dbContext,
    wikiTagProvider,
    fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _endpoint = new FilesEndpoint(
      Logger,
      configuration,
      DbContext);
  }

  private static string CapitalizeFirstLetter(string str)
  {

    if (str.Length == 0)
      return str;

    if (str.Length == 1)
      return char.ToUpper(str[0]).ToString();
    else
      return char.ToUpper(str[0]) + str[1..];
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="take"></param>
  /// <param name="skip"></param>
  /// <returns></returns>
  [HttpGet]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync([FromQuery] int? take, [FromQuery] int? skip)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);
      return NoContent();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "FilesController.GetAsync error");

      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }

  /// <summary>
  /// Create new file
  /// </summary>
  /// <param name="dto">File data</param>
  /// <returns>IActionResult</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PostAsync()
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);
      return NoContent();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "FilesController.PostAsync error");
      return OLabServerErrorResult.Result(ex.Message);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetAsync(uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);
      return NoContent();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "FilesController.GetAsync error");

      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

  }

  /// <summary>
  /// Saves a file edit
  /// </summary>
  /// <param name="id">file id</param>
  /// <returns>IActionResult</returns>
  [HttpPut("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> PutAsync(uint id, [FromBody] FilesFullDto dto)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);
      return NoContent();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "FilesController.PutAsync error");

      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }

    return NoContent();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpDelete("{id}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> DeleteAsync(uint id)
  {
    try
    {
      // validate token/setup up common properties
      var auth = GetRequestContext(HttpContext);
      return NoContent();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "FilesController.DeleteAsync error");
      if (ex is OLabUnauthorizedException)
        return OLabUnauthorizedObjectResult.Result(ex.Message);
      return OLabServerErrorResult.Result(ex.Message);
    }
  }
}
