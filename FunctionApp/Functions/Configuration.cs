using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OLabWebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OLab.FunctionApp.Functions
{
  public class Configuration
  {
    private readonly IConfiguration _configuration;
    public IOptions<AppSettings> appSettings;

    public Configuration(IConfiguration configuration)
    {
      _configuration = configuration;

      var appSettings = CreateAppSettings();
      this.appSettings = Options.Create(appSettings);

    }

    private AppSettings CreateAppSettings()
    {
      var appSettings = new AppSettings();

      var properties = appSettings.GetType().GetProperties();
      foreach (var property in properties)
      {
        var value = GetValue<string>(property.Name);

        PropertyInfo prop = appSettings.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
        if (null != prop && prop.CanWrite)
          prop.SetValue(appSettings, value, null);
      }

      return appSettings;
    }

#nullable enable
    public T? GetValue<T>(string section, string key)
#nullable disable
    {
      return _configuration.GetValue<T>($"Values:{section}:{key}");
    }

#nullable enable
    public T? GetValue<T>(string key)
#nullable disable
    {
      return _configuration.GetValue<T>($"Values:AppSettings:{key}");
    }

  }
}
