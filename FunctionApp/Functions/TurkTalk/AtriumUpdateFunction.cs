using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using OLab.TurkTalk.Endpoints;
using OLab.TurkTalk.Endpoints.Mappers;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction : OLabFunction
{
  //[Function("AtriumUpdate")]
  //public async Task<TTalkMessageQueue> AtriumUpdate([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
  //{
  //  Logger.LogInformation($"AtriumUpdate Timer trigger function executed at: {DateTime.Now}");

  //  var physConference = await TtalkDbContext.TtalkConferences
  //    .Include("TtalkConferenceTopics")
  //    .FirstOrDefaultAsync();

  //  var mapper = new ConferenceMapper(Logger);
  //  var conference = mapper.PhysicalToDto( physConference );

  //  var endpoint = new TurkTalkEndpoint(
  //    Logger,
  //    _configuration,
  //    DbContext,
  //    TtalkDbContext,
  //    conference);

  //  endpoint.GetAtriumContents();

  //  return endpoint.MessageQueue;
  //}
}
