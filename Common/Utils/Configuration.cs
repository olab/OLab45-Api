using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI.Utils
{
  public static class OLabConfiguration
  {
    public static int DefaultTokenExpiryMins = 120;
    public const string SIGNING_SECRET = "1e00d4389048e02b9cf101d24110d297993511fe17dafc92a35167f915a029b7";
    public const string ENCRYPTION_SECRET = "ResistanceIsFutile001PointlessToREsist001";
  }
}