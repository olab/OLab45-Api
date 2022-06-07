using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLabWebAPI.Model;
using OLabWebAPI.Dto;
using OLabWebAPI.ObjectMapper;
using OLabWebAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

using OLabWebAPI.Services;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Controllers.Player
{
  [Route("olab/api/v3/questions")]
  [ApiController]
  public partial class QuestionsController : OlabController
  {
    public QuestionsController(ILogger<QuestionsController> logger, OLabDBContext context) : base(logger, context)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool Exists(uint id)
    {
      return context.SystemQuestions.Any(e => e.Id == id);
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
        logger.LogDebug($"QuestionsController.GetAsync([FromQuery] int? take={take}, [FromQuery] int? skip={skip})");

        var token = Request.Headers["Authorization"];
        var physList = new List<SystemQuestions>();
        var total = 0;
        var remaining = 0;

        if (!skip.HasValue)
          skip = 0;

        physList = await context.SystemQuestions.OrderBy(x => x.Name).ToListAsync();
        total = physList.Count;

        if (take.HasValue && skip.HasValue)
        {
          physList = physList.Skip(skip.Value).Take(take.Value).ToList();
          remaining = total - take.Value - skip.Value;
        }

        logger.LogDebug(string.Format("found {0} questions", physList.Count));

        var dtoList = new ObjectMapper.Questions(logger, new WikiTagProvider(logger)).PhysicalToDto(physList);
        return OLabObjectPagedListResult<QuestionsDto>.Result(dtoList, remaining);
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
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAsync(uint id)
    {
      try
      {
        logger.LogDebug($"QuestionsController.GetAsync(uint id={id})");

        if (!Exists(id))
          return OLabNotFoundResult<uint>.Result(id);

        var phys = await context.SystemQuestions.Include("SystemQuestionResponses").FirstAsync(x => x.Id == id);
        var builder = new QuestionsFull(logger);
        var dto = builder.PhysicalToDto(phys);

        // test if user has access to object
        var accessResult = HasAccessToScopedObject(dto);
        if (accessResult is UnauthorizedResult)
          return accessResult;

        AttachParentObject(dto);

        return OLabObjectResult<QuestionsFullDto>.Result(dto);
      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }

    }

    /// <summary>
    /// Saves a question edit
    /// </summary>
    /// <param name="id">question id</param>
    /// <returns>IActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PutAsync(uint id, [FromBody] QuestionsFullDto dto)
    {
      logger.LogDebug($"PutAsync(uint id={id})");

      dto.ImageableId = dto.ParentObj.Id;

      // test if user has access to object
      var accessResult = HasAccessToScopedObject(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new QuestionsFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.UpdatedAt = DateTime.Now;

        context.Entry(phys).State = EntityState.Modified;
        await context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }

      return NoContent();
    }

    /// <summary>
    /// Create new question
    /// </summary>
    /// <param name="dto">Question data</param>
    /// <returns>IActionResult</returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAsync([FromBody] QuestionsFullDto dto)
    {
      logger.LogDebug($"QuestionsController.PostAsync({dto.Stem})");

      dto.ImageableId = dto.ParentObj.Id;
      dto.Prompt = !string.IsNullOrEmpty(dto.Prompt) ? dto.Prompt : "";

      // test if user has access to object
      var accessResult = HasAccessToScopedObject(dto);
      if (accessResult is UnauthorizedResult)
        return accessResult;

      try
      {
        var builder = new QuestionsFull(logger);
        var phys = builder.DtoToPhysical(dto);

        phys.CreatedAt = DateTime.Now;

        context.SystemQuestions.Add(phys);
        await context.SaveChangesAsync();

        dto = builder.PhysicalToDto(phys);
        return OLabObjectResult<QuestionsFullDto>.Result(dto);

      }
      catch (Exception ex)
      {
        return OLabServerErrorResult.Result(ex.Message);
      }
    }

  }

}
