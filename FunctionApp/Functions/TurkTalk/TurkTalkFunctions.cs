using Dawn;
using DocumentFormat.OpenXml.InkML;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Models;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Data.Models;
using OLab.FunctionApp.Functions.API;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;
using System.Net;
using System.Security.Claims;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class TurkTalkFunction : OLabFunction
  {
    private readonly TTalkDBContext _ttalkDbContext;
    private readonly IConference _conference;

    public TurkTalkFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext,
      TTalkDBContext ttalkDbContext,
      IConference conference,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider ) : base(
        configuration,
        dbContext,
        wikiTagProvider,
        fileStorageProvider)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));
      Guard.Argument(conference).NotNull(nameof(conference));

      Logger = OLabLogger.CreateNew<TurkTalkFunction>(loggerFactory);
      _ttalkDbContext = ttalkDbContext;
      _conference = conference;
    }

    public Learner CreateFromContext(SignalRInvocationContext hostContext)
    {
      var learner = new Learner();

      //var nickName = "";
      //if (nameClaim != null)
      //  nickName = nameClaim.Value;
      //else
      //  nickName = userId;

      Guard.Argument(hostContext.ConnectionId).NotNull(nameof(hostContext.ConnectionId));
      Guard.Argument(hostContext.UserId).NotNull(nameof(hostContext.UserId));
      //Guard.Argument(nickName).NotNull(nameof(nickName));

      learner.ConnectionId = hostContext.ConnectionId;
      //learner.NickName = nickName;
      learner.UserId = hostContext.UserId;


      //RemoteIpAddress = httpContext.Connection.RemoteIpAddress.ToString();

      //IPAddress clientIp;
      //IPAddress.TryParse(httpContext.Request.Headers["cf-connecting-ip"], out clientIp);

      return learner;
    }

  }
}
