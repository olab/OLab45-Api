using Dawn;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Services;

public class UserService : OLab.Access.UserService
{
  public UserService(
    ILoggerFactory loggerFactory, 
    OLabDBContext context, 
    IOLabConfiguration config) : base(loggerFactory, context, config)
  {
  }
}