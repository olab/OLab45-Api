using Dawn;
using FluentValidation;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.Api.Model;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;
using System.Threading;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{
  protected readonly TTalkDBContext TtalkDbContext;
  private readonly IConference _conference;

  public TurkTalkFunction(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext,
    IConference conference,
    IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
    IOLabModuleProvider<IFileStorageModule> fileStorageProvider) : base(
      configuration,
      dbContext,
      wikiTagProvider,
      fileStorageProvider)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));
    Guard.Argument(conference).NotNull(nameof(conference));

    Logger = OLabLogger.CreateNew<TurkTalkFunction>(loggerFactory);

    TtalkDbContext = ttalkDbContext;
    _conference = conference;
  }

}
