using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;
using System.Text;
using OLabWebAPI.Model;
using OLabWebAPI.Common;

namespace OLabWebAPI.Controllers.Player
{
  public partial class MapsController : OlabController
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint id)
    {
      logger.LogDebug($"MapsController.GetScopedObjectsRawAsync(uint id={id})");

      // test if user has access to map.
      var userContext = new UserContext(logger, context, HttpContext);
      if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, id))
        return OLabUnauthorizedObjectResult<uint>.Result(id);

      return await GetScopedObjectsAsync(id, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint id)
    {
      try
      {
        logger.LogDebug($"MapsController.GetScopedObjectsTranslatedAsync(uint id={id})");

        // test if user has access to map.
        var userContext = new UserContext(logger, context, HttpContext);
        if (!userContext.HasAccess("R", Utils.Constants.ScopeLevelMap, id))
          return OLabUnauthorizedObjectResult<uint>.Result(id);

        return await GetScopedObjectsAsync(id, true);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "GetScopedObjectsAsync");
        throw;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enableWikiTranslation"></param>
    /// <returns></returns>
    private async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      var map = GetSimple(context, id);
      if (map == null)
        return OLabNotFoundResult<uint>.Result( id );

      var phys = await GetScopedObjectsAllAsync(map.Id, Utils.Constants.ScopeLevelMap);

      phys.Constants.Add(new SystemConstants
      {
        Id = 0,
        Name = Utils.Constants.ReservedConstantMapId,
        ImageableId = map.Id,
        ImageableType = Utils.Constants.ScopeLevelMap,
        IsSystem = 1,
        Value = Encoding.ASCII.GetBytes(map.Id.ToString())
      });

      phys.Constants.Add(new SystemConstants
      {
        Id = 0,
        Name = Utils.Constants.ReservedConstantMapName,
        ImageableId = map.Id,
        ImageableType = Utils.Constants.ScopeLevelMap,
        IsSystem = 1,
        Value = Encoding.ASCII.GetBytes(map.Name)
      });

      var builder = new ObjectMapper.ScopedObjects(logger, enableWikiTranslation);

      var dto = builder.PhysicalToDto(phys);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);
    }

  }
}
