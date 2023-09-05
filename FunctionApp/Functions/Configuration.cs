using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions
{
  public class Configuration
  {
    private readonly IConfiguration _configuration;

    public Configuration(IConfiguration configuration)
    {
      _configuration = configuration;
    }

#nullable enable
    public T? GetValue<T>( string section, string key )
#nullable disable
    {
      return _configuration.GetValue<T>($"Values:{section}:{key}");
    }

#nullable enable
    public T? GetValue<T>( string key )
#nullable disable
    {
      return _configuration.GetValue<T>($"Values:AppSettings:{key}");
    }

  }
}
