using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Common;

namespace OLabWebAPI.Controllers.Player
{
  public partial class NodesController : OlabController
  {
    [HttpGet("{nodeId}/scopedobjects/raw")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsRawAsync(uint nodeId)
    {
      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, false);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);      
    }

    [HttpGet("{nodeId}/scopedobjects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetScopedObjectsAsync(uint nodeId)
    {
      var dto = await _endpoint.GetScopedObjectsAsync(nodeId, true);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);      
    }

    public async Task<IActionResult> GetScopedObjectsAsync(
      uint id,
      bool enableWikiTranslation)
    {
      var dto = await _endpoint.GetScopedObjectsAsync(id, enableWikiTranslation);
      return OLabObjectResult<ScopedObjectsDto>.Result(dto);
    }
  }
}