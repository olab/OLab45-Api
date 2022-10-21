using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

using OLabWebAPI.Services;
using OLabWebAPI.Common;
using OLabWebAPI.ObjectMapper;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/counters")]
  [ApiController]
  public partial class CountersController : OlabController
  {
    public CountersController(ILogger<CountersController> logger, OLabDBContext context) : base(logger, context)
    {
      // var token = OLabUserService.CreateJwt("sub", "jti", "issuer", "audience");
      // OLabUserService.ValidateToken(token, "issuer", "audience");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool Exists(uint id)
    {
      return context.SystemCounters.Any(e => e.Id == id);
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
      logger.LogDebug($"CountersController.GetAsync([FromQuery] int? take={take}, [FromQuery] int? skip={skip})");

      var Counters = new List<SystemCounters>();
      var total = 0;
      var remaining = 0;

      if (!skip.HasValue)
        skip = 0;

      Counters = await context.SystemCounters.OrderBy(x => x.Name).ToListAsync();
      total = Counters.Count;

      if (take.HasValue && skip.HasValue)
      {
        Counters = Counters.Skip(skip.Value).Take(take.Value).ToList();
        remaining = total - take.Value - skip.Value;
      }

      logger.LogDebug(string.Format("found {0} Counters", Counters.Count));

      var dtoList = new ObjectMapper.Counters(logger).PhysicalToDto(Counters);
      return OLabObjectPagedListResult<CountersDto>.Result(dtoList, remaining);
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
      logger.LogDebug($"CountersController.GetAsync(uint id={id})");

      if (!Exists(id))
        return OLabNotFoundResult<uint>.Result(id);

      var phys = await GetCounterAsync(id);
      var dto = new CountersFull(logger).PhysicalToDto(phys);

      // test if user has access to object
      var accessResult = HasAccess(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      AttachParentObject(dto);

      return OLabObjectResult<CountersFullDto>.Result(dto);
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
      logger.LogDebug($"CountersController.DeleteAsync(uint id={id})");

      if (!Exists(id))
        return OLabNotFoundResult<uint>.Result(id);

      try
      {
        var phys = await GetCounterAsync(id);
        var dto = new CountersFull(logger).PhysicalToDto(phys);

        // test if user has access to object
        var accessResult = HasAccess(dto);
        if (accessResult is UnauthorizedResult)
          return accessResult;

        context.SystemCounters.Remove(phys);
        await context.SaveChangesAsync();

      }
      catch (DbUpdateConcurrencyException)
      {
        var existingObject = await GetCounterAsync(id);
        if (existingObject == null)
          return OLabNotFoundResult<uint>.Result(id);
        else
        {
          throw;
        }
      }

      return NoContent();
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
      logger.LogDebug($"PutAsync(uint id={id})");

      dto.ImageableId = dto.ParentObj.Id;

      // test if user has access to object
      var accessResult = HasAccess(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new CountersFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.UpdatedAt = DateTime.Now;

        context.Entry(phys).State = EntityState.Modified;
        await context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        var existingObject = await GetCounterAsync(id);
        if (existingObject == null)
          return OLabNotFoundResult<uint>.Result(id);
        else
        {
          throw;
        }
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
      logger.LogDebug($"CountersController.PostAsync({dto.Name})");

      dto.ImageableId = dto.ParentObj.Id;
      dto.Value = dto.StartValue;

      // test if user has access to object
      var accessResult = HasAccess(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new CountersFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        context.SystemCounters.Add(phys);
        await context.SaveChangesAsync();

        dto = builder.PhysicalToDto(phys);

        AttachParentObject(dto);

        return OLabObjectResult<CountersFullDto>.Result(dto);

      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }
    }
  }

}
