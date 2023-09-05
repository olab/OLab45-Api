using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using OLabWebAPI.Common.Exceptions;

namespace OLab.FunctionApp.Api;

  public static class HttpRequestDataExtensions
  {
      public static async Task<T> ParseBodyFromRequestAsync<T>([NotNull] this HttpRequestData request)
          where T : class
      {
          var (isSuccess, body, exception) = await request.TryReadBodyAsAsync<T>();
          if (!isSuccess)
              throw new OLabInvalidRequestException(exception);

          return body;
      }

      public static async Task<(bool IsSuccess, T Value, Exception Exception)> TryReadBodyAsAsync<T>(
          [NotNull] this HttpRequestData request)
          where T : class
      {
          try
          {
              using var reader = new StreamReader(request.Body);
              var json = await reader.ReadToEndAsync();
              var result = JsonConvert.DeserializeObject<T>(json);
              return (result != null, result, null);
          }
          catch (Exception e)
          {
              return (false, default, e);
          }
      }
  }