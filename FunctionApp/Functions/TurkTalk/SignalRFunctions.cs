using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.Data.Interface;
using OLab.FunctionApp.Functions.API;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.FunctionApp.Functions.SignalR
{
    public partial class TurkTalkFunction : OLabFunction
  {
    private readonly TTalkDBContext ttalkDbContext;

    public TurkTalkFunction(
      ILoggerFactory loggerFactory,
      IOLabConfiguration configuration,
      OLabDBContext dbContext,
      TTalkDBContext ttalkDbContext,
      IOLabModuleProvider<IWikiTagModule> wikiTagProvider,
      IOLabModuleProvider<IFileStorageModule> fileStorageProvider ) : base(
        configuration,
        dbContext,
        wikiTagProvider,
        fileStorageProvider)
    {
      Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
      Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

      Logger = OLabLogger.CreateNew<TurkTalkFunction>(loggerFactory);
      this.ttalkDbContext = ttalkDbContext;
    }

    public class NewMessage
    {
      public string ConnectionId { get; }
      public string Sender { get; }
      public string Text { get; }

      public NewMessage(SignalRInvocationContext invocationContext, string message)
      {
        Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
        ConnectionId = invocationContext.ConnectionId;
        Text = message;
      }
    }
  }
}
