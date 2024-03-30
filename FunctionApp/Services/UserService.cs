using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Common.Interfaces;

namespace OLab.FunctionApp.Services;

public class UserService : Access.UserService
{
  public UserService(
    ILoggerFactory loggerFactory, 
    OLabDBContext context, 
    IOLabConfiguration config) : base(loggerFactory, context, config)
  {
  }
}