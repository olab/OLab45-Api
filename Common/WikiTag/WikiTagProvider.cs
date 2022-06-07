using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Common
{
  public class WikiTagProvider
  {
    // private static List<IWikiTagModule> m_WikiTagModules;
    private Dictionary<string, IWikiTagModule> m_WikiTagModules;
    private readonly OLabLogger logger;

    public WikiTagProvider(OLabLogger logger)
    {
      this.logger = logger;
      Reload();
    }

    public string Translate(string source)
    {
      foreach (var module in m_WikiTagModules.Values)
        source = module.Translate(source);

      return source;

    }

    /// <summary>
    /// Reloads the plug-in assemblies
    /// </summary>
    private void Reload()
    {
      if (null == m_WikiTagModules)
        m_WikiTagModules = new Dictionary<string, IWikiTagModule>();
      else
        m_WikiTagModules.Clear();

      List<Assembly> plugInAssemblies = LoadPlugInAssemblies();
      m_WikiTagModules = GetPlugIns(plugInAssemblies);
    }

    /// <summary>
    /// Gets a list of assemblies in the Plugin directory
    /// </summary>
    /// <returns>List of assemblies</returns>
    private List<Assembly> LoadPlugInAssemblies()
    {
      var currentAssemblyFile = Assembly.GetExecutingAssembly().CodeBase;
      var rootPath = Path.GetDirectoryName(currentAssemblyFile);

      // logger.LogDebug($"currentAssemblyFile path: {rootPath}");

      // remove the URI specifier (both for Windows and Linux)
      rootPath = rootPath.Replace("file:\\", "");
      rootPath = rootPath.Replace("file:", "");
      rootPath = Path.Combine(rootPath, "PlugIns");

      // logger.LogDebug($"pluginAssembly path: {rootPath}");

      if (!Directory.Exists(rootPath))
        throw new DirectoryNotFoundException("Unable to load plugin path");

      var files = Directory.GetFiles(rootPath, "*.dll");

      List<Assembly> plugInAssemblyList = new List<Assembly>();

      foreach (var file in files)
        plugInAssemblyList.Add(Assembly.LoadFile(file));

      return plugInAssemblyList;
    }

    /// <summary>
    /// Get installed Wiki tag plugs-in from a list of assemblies
    /// </summary>
    /// <param name="assemblies">List of assemblies</param>
    /// <returns>Dictionary of plug-ins</returns>
    private Dictionary<string, IWikiTagModule> GetPlugIns(List<Assembly> assemblies)
    {
      List<Type> availableTypes = new List<Type>();

      foreach (Assembly currentAssembly in assemblies)
        availableTypes.AddRange(currentAssembly.GetTypes());

      // get a list of objects that implement the IWikiTagModule interface AND 
      // have the WikiTagModuleAttribute
      List<Type> wikiTagList = availableTypes.FindAll(delegate (Type t)
      {
        List<Type> interfaceTypes = new List<Type>(t.GetInterfaces());
        object[] arr = t.GetCustomAttributes(typeof(WikiTagModuleAttribute), true);
        return !(arr == null || arr.Length == 0) && interfaceTypes.Contains(typeof(IWikiTagModule));
      });

      var dict = new Dictionary<string, IWikiTagModule>();
      foreach (var item in wikiTagList)
      {
        var t = item.GetCustomAttribute<WikiTagModuleAttribute>();
        dict.Add(t.WikiTag, (IWikiTagModule)Activator.CreateInstance(item, logger));
      }
      return dict;
    }
  }
}
