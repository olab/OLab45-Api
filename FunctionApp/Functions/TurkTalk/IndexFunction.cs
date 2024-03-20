using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.FunctionApp.Extensions;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions.SignalR;

public partial class TurkTalkFunction
{

  [Function("index")]
  public async Task<HttpResponseData> GetWebPage(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "index")] HttpRequestData req,
    FunctionContext hostContext,
    CancellationToken token)
  {

    // get html file from file module storage
    var fileSystemModuleName = _configuration.GetAppSettings().FileStorageType;
    if (string.IsNullOrEmpty(fileSystemModuleName))
      throw new ConfigurationErrorsException($"missing FileStorageType");

    var fileStorageModule = _fileStorageProvider.GetModule(fileSystemModuleName);

    var response = req.CreateResponse(HttpStatusCode.OK);

    using (var stream = new MemoryStream())
    {
      await fileStorageModule.ReadFileAsync(
        stream, "", "turktalk", "index.html", token);

      var reader = new StreamReader(stream);
      response.WriteString(reader.ReadToEnd());

      response.Headers.Add("Content-Type", "text/html");
      return response;
    }
  }
}
