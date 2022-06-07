using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

using OLabWebAPI.Services;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/constants")]
  [ApiController]
  public partial class ConstantsController : OlabController
  {
    public ConstantsController(ILogger<ConstantsController> logger, OLabDBContext context) : base(logger, context)
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
      return context.SystemConstants.Any(e => e.Id == id);
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
      logger.LogDebug($"ConstantsController.GetAsync([FromQuery] int? take={take}, [FromQuery] int? skip={skip})");

      var token = Request.Headers["Authorization"];
      var Constants = new List<SystemConstants>();
      var total = 0;
      var remaining = 0;

      if (!skip.HasValue)
        skip = 0;

      Constants = await context.SystemConstants.OrderBy(x => x.Name).ToListAsync();
      total = Constants.Count;

      if (take.HasValue && skip.HasValue)
      {
        Constants = Constants.Skip(skip.Value).Take(take.Value).ToList();
        remaining = total - take.Value - skip.Value;
      }

      logger.LogDebug(string.Format("found {0} Constants", Constants.Count));

      var dtoList = new ObjectMapper.Constants(logger).PhysicalToDto(Constants);
      return OLabObjectPagedListResult<ConstantsDto>.Result(dtoList, remaining);
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
      logger.LogDebug($"ConstantsController.GetAsync(uint id={id})");

      if (!Exists(id))
        return OLabNotFoundResult<uint>.Result(id);

      var phys = await context.SystemConstants.FirstAsync(x => x.Id == id);
      var dto = new ObjectMapper.Constants(logger).PhysicalToDto(phys);

      // test if user has access to object
      var accessResult = HasAccessToScopedObject(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      AttachParentObject(dto);

      return OLabObjectResult<ConstantsDto>.Result(dto);
    }

    /// <summary>
    /// Saves a object edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] ConstantsDto dto)
    {
      logger.LogDebug($"PutAsync(uint id={id})");

      dto.ImageableId = dto.ParentObj.Id;

      // test if user has access to object
      var accessResult = HasAccessToScopedObject(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new ConstantsFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.UpdatedAt = DateTime.Now;

        context.Entry(phys).State = EntityState.Modified;
        await context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        var existingObject = await GetConstantAsync(id);
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
    /// Create new object
    /// </summary>
    /// <param name="dto">object data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] ConstantsDto dto)
    {
      logger.LogDebug($"ConstantsController.PostAsync({dto.Name})");

      dto.ImageableId = dto.ParentObj.Id;

      // test if user has access to object
      var accessResult = HasAccessToScopedObject(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new ConstantsFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        context.SystemConstants.Add(phys);
        await context.SaveChangesAsync();

        dto = builder.PhysicalToDto(phys);
        return OLabObjectResult<ConstantsDto>.Result(dto);

      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
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
      logger.LogDebug($"ConstantsController.DeleteAsync(uint id={id})");

      if (!Exists(id))
        return OLabNotFoundResult<uint>.Result(id);

      try
      {
        var phys = await GetConstantAsync(id);
        var dto = new ConstantsFull(logger).PhysicalToDto(phys);

        // test if user has access to object
        var accessResult = HasAccessToScopedObject(dto);
        if (accessResult is UnauthorizedResult)
          return accessResult;

        context.SystemConstants.Remove(phys);
        await context.SaveChangesAsync();

      }
      catch (DbUpdateConcurrencyException)
      {
        var existingObject = await GetConstantAsync(id);
        if (existingObject == null)
          return OLabNotFoundResult<uint>.Result(id);
        else
        {
          throw;
        }
      }

      return NoContent();
    }

  }

}
