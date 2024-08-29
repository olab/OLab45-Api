using Dawn;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Access.Interfaces;
using OLab.Api.Common;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Contracts;
using OLab.Data.Dtos;
using OLab.Data.Interface;
using OLabWebAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace OLabWebAPI.Endpoints.WebApi;

/// <summary>
/// 
/// </summary>
[Route("olab/api/v3/[controller]/[action]")]
[ApiController]
public class AuthController : OLabController
{
  protected readonly IUserService _userService;
  private readonly IOLabAuthentication _authentication;

  public AuthController(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    IUserService userService,
    IOLabAuthentication authentication,
    OLabDBContext dbContext) : base(configuration, dbContext)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<AuthController>(loggerFactory);
    _authentication = authentication;
    _userService = userService;
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="model"></param>
  /// <returns></returns>
  [AllowAnonymous]
  [HttpPost]
  public async Task<IActionResult> Login(LoginRequest model)
  {
    var ipAddress = HttpContext.Request.Headers["x-forwarded-for"].ToString();

    if (string.IsNullOrEmpty(ipAddress))
      ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

    bool impersonateMode = false;
    try
    {
      var auth = GetAuthorization(HttpContext);
      // if have token and user is superuser, then we can impersonate requested user
      impersonateMode = await auth.IsSystemSuperuserAsync();
    }
    catch (Exception)
    {
      Logger.LogInformation($"Normal login w/o impersonate");
    }

    model.Username = model.Username.ToLower();

    Logger.LogDebug($"Login(user = '{model.Username}' ip: {ipAddress})");

    var user = _authentication.Authenticate(model, impersonateMode);
    if (user == null)
      return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result("Username or password is incorrect"));

    var response = _authentication.GenerateJwtToken(user);
    return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="mapId">map id to run</param>
  /// <returns>AuthenticateResponse</returns>
  [AllowAnonymous]
  [HttpGet("{mapId}")]
  public IActionResult LoginAnonymous(uint mapId)
  {
    Logger.LogDebug($"LoginAnonymous(mapId = '{mapId}')");

    try
    {
      var response = _authentication.GenerateAnonymousJwtToken(mapId);
      if (response == null)
        return HttpContext.Request.CreateResponse(OLabUnauthorizedObjectResult.Result("Must be Logged on to Play Map"));

      return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Interactive login
  /// </summary>
  /// <param name="model"></param>
  /// <returns>AuthenticateResponse</returns>
  [AllowAnonymous]
  [HttpPost]
  public IActionResult LoginExternal(ExternalLoginRequest model)
  {
    Logger.LogDebug($"LoginExternal(user = '{model.ExternalToken}')");

    try
    {
      var response = _authentication.GenerateExternalJwtToken(model);
      if (response == null)
        return BadRequest(new { statusCode = 401, message = "Invalid external token" });

      return HttpContext.Request.CreateResponse(OLabObjectResult<AuthenticateResponse>.Result(response));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <param name="jsonStringData">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> AddUsers([FromBody] JArray jsonStringData)
  {
    try
    {
      var items = JsonConvert.DeserializeObject<List<AddUserRequest>>(jsonStringData.ToString());
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var responses = await _userService.AddUsersAsync(items);
      return HttpContext.Request.CreateResponse(
        OLabObjectListResult<UsersDto>.Result(responses));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <param name="jsonStringData">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> AddUser([FromBody] AddUserRequest body)
  {
    try
    {
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var response = await _userService.AddUserAsync(body);
      return HttpContext.Request.CreateResponse(
        OLabObjectResult<UsersDto>.Result(response));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }


  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <param name="jsonStringData">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> DeleteUser([FromBody] JArray jsonStringData)
  {
    try
    {
      var items = JsonConvert.DeserializeObject<List<DeleteUsersRequest>>(jsonStringData.ToString());
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var responses = await _userService.DeleteUsersAsync(items);
      return HttpContext.Request.CreateResponse(
        OLabObjectListResult<AddUserResponse>.Result(responses));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }

  /// <summary>
  /// Query users
  /// </summary>
  /// <param name="request">GetUsersRequest user query</param>
  /// <returns>List of users</returns>
  [HttpGet("{name?}")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetUsers(string name = null)
  {
    try
    {
      var responses = new List<AddUserResponse>();
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var response = _userService.GetUsers(name);

      return HttpContext.Request.CreateResponse(
        OLabObjectListResult<UsersDto>.Result(response));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Adds users from CSV file
  /// </summary>
  /// <param name="file">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> AddUsers(IFormFile file)
  {
    try
    {
      var responses = new List<UsersDto>();
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var fileStream = file.OpenReadStream();
      using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileStream, false))
      {

        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        Worksheet sheet = worksheetPart.Worksheet;

        SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
        SharedStringTable sst = sstpart.SharedStringTable;

        var cells = sheet.Descendants<Cell>();
        var rows = sheet.Descendants<Row>();

        Logger.LogInformation($"Row count = {rows.LongCount()}");
        Logger.LogInformation($"Cell count = {cells.LongCount()}");

        foreach (Row row in rows)
        {
          int column = 0;
          var userRequest = new AddUserRequest(
            Logger,
            DbContext);

          var groupRoleStrings = new List<string>();

          foreach (Cell c in row.Elements<Cell>())
          {
            if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
            {
              int ssid = int.Parse(c.CellValue.Text);
              string str = sst.ChildElements[ssid].InnerText;
              Logger.LogInformation($"String: {str}");

              switch (column)
              {
                case 0:
                  userRequest.Operation = str;
                  break;
                case 1:
                  userRequest.Username = str;
                  break;
                case 2:
                  userRequest.NickName = str;
                  break;
                case 3:
                  userRequest.EMail = str;
                  break;
                default:
                  groupRoleStrings.Add(str);
                  break;
              }

            }

            column++;
          }

          userRequest.GroupRoles = string.Join(",", groupRoleStrings);

          if (string.IsNullOrEmpty(userRequest.Operation) || userRequest.Operation == "+")
          {
            var response = await _userService.AddUserAsync(userRequest);
            responses.Add(response);
          }

          else if (userRequest.Operation == "*")
          {
            var response = await _userService.EditUserAsync(userRequest);
            responses.Add(response);
          }

          else if (userRequest.Operation == "-")
          {
            var list = new List<DeleteUsersRequest>();
            list.Add(new DeleteUsersRequest { UserName = userRequest.Username });
            await _userService.DeleteUsersAsync(list);
          }
        }
      }

      //var result = new List<string>();
      //using (var reader = new StreamReader(file.OpenReadStream()))
      //{
      //  while (reader.Peek() >= 0)
      //  {
      //    var userRequestText = reader.ReadLine();
      //    var userRequest = new AddUserRequest(
      //      Logger,
      //      DbContext);

      //    userRequest.ProcessAddUserText(userRequestText);

      //    var response = await _userService.AddUserAsync(userRequest);
      //    responses.Add(response);
      //  }
      //}

      return HttpContext.Request.CreateResponse(
        OLabObjectListResult<UsersDto>.Result(responses));

    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }
  }

  /// <summary>
  /// Adds users from posted json records
  /// </summary>
  /// <param name="jsonStringData">User records</param>
  /// <returns>Array of AddUserResponse records</returns>
  [HttpPut]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public async Task<IActionResult> EditUser([FromBody] AddUserRequest request)
  {
    try
    {
      var auth = GetAuthorization(HttpContext);

      // test if user has access to add users.
      if (!await auth.IsSystemSuperuserAsync())
        return OLabUnauthorizedResult.Result();

      var response = await _userService.EditUserAsync(request);
      return HttpContext.Request.CreateResponse(
        OLabObjectResult<UsersDto>.Result(response));
    }
    catch (Exception ex)
    {
      return ProcessException(ex, HttpContext.Request);
    }

  }
}