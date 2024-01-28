using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Endpoints;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Endpoints.WebApi.Player
{
  [Route("olab/api/v3/counters")]
  [ApiController]
  public partial class CountersController : OLabController
  {
    private readonly CountersEndpoint _endpoint;

    public CountersController(
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

      Logger = OLabLogger.CreateNew<ConstantsController>(loggerFactory);

      _endpoint = new CountersEndpoint(
        Logger,
        configuration,
        DbContext);
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
        var auth = GetAuthorization(HttpContext);

        var pagedResult = await _endpoint.GetAsync(auth, take, skip);
        return HttpContext.Request.CreateResponse(OLabObjectPagedListResult<CountersDto>.Result(pagedResult.Data, pagedResult.Remaining));
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
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
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        var dto = await _endpoint.GetAsync(auth, id);
        return HttpContext.Request.CreateResponse(OLabObjectResult<CountersDto>.Result(dto));
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
      }
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] CountersFullDto dto)
    {
      try
      {
        Guard.Argument(id, nameof(id)).NotZero();
        Guard.Argument(dto).NotNull(nameof(dto));

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        await _endpoint.PutAsync(auth, id, dto);
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
      }

      return NoContent();
    }

    /// <summary>
    /// Create new counter
    /// </summary>
    /// <param name="dto">Counter data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] CountersFullDto dto)
    {
      try
      {
        Guard.Argument(dto).NotNull(nameof(dto));

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        dto = await _endpoint.PostAsync(auth, dto);
        return HttpContext.Request.CreateResponse(OLabObjectResult<CountersFullDto>.Result(dto));
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
      }
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
        Guard.Argument(id, nameof(id)).NotZero();

        // validate token/setup up common properties
        var auth = GetAuthorization(HttpContext);

        await _endpoint.DeleteAsync(auth, id);
      }
      catch (Exception ex)
      {
        return ProcessException(ex, HttpContext.Request);
      }

      return NoContent();
    }

  }

}
