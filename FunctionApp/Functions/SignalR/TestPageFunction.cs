using Dawn;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using OLab.FunctionApp.Extensions;
using System.Configuration;
using System.Net;

namespace OLab.FunctionApp.Functions.SignalR
{
  public partial class OLabSignalRFunction
  {

    [Function("testpage")]
    public async Task<HttpResponseData> GetWebPage(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "turktalk/testpage")] HttpRequestData req,
      FunctionContext hostContext,
      CancellationToken token)
    {

      var fileSystemModuleName = _configuration.GetAppSettings().FileStorageType;
      if (string.IsNullOrEmpty(fileSystemModuleName))
        throw new ConfigurationErrorsException($"missing FileStorageType");

      var fileStorageModule = _fileStorageProvider.GetModule(fileSystemModuleName);

      var response = req.CreateResponse(HttpStatusCode.OK);

      using (var stream = new MemoryStream())
      {
        await fileStorageModule.ReadFileAsync(stream, "turktalk", "index.html", token);

        var reader = new StreamReader(stream);
        response.WriteString(reader.ReadToEnd());

        response.Headers.Add("Content-Type", "text/html");
        return response;
      }
    }
  }
}
