using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Common
{
  public abstract class WikiTagModule : IWikiTagModule
  {
    protected List<string> wikiTagPatterns = new List<string>();
    private string _wikiType;
    private string _wiki;
    private readonly string _htmlElementName;

    protected int wikiStart = 0;
    protected int wikiEnd = 0;
    protected OLabLogger _logger;

    public WikiTagModule(OLabLogger logger, string htmlElementName)
    {
      Type t = GetType();
      WikiTagModuleAttribute attribute =
          (WikiTagModuleAttribute)Attribute.GetCustomAttribute(t, typeof(WikiTagModuleAttribute));
      _wikiType = attribute.WikiTag;
      _htmlElementName = htmlElementName;

      _logger = logger;
    }

    public string GetWikiType() { return _wikiType; }
    public void SetWikiType(string wikiTag) { _wikiType = wikiTag; }
    public string GetHtmlElementName() { return _htmlElementName; }
    public abstract string Translate(string source);
    protected abstract string BuildWikiTagHTMLElement();

    public string GetWiki() { return _wiki; }
    public string BuildWiki(uint id, string name = "")
    {
      if (string.IsNullOrEmpty(name))
        return $"[[{GetWikiType()}:{id}]]";
      return $"[[{GetWikiType()}:{name}]]";
    }
    
    protected string ReplaceWikiTag(string original, string replaceString)
    {
      var preWikiPart = original[..wikiStart];
      var postWikiPart = original[wikiEnd..];
      return preWikiPart + replaceString + postWikiPart;
    }

    /// <summary>
    /// Test if have wiki tag in source string
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>true/false</returns>
    public bool HaveWikiTag(string source)
    {
      foreach (var pattern in wikiTagPatterns)
      {
        Regex regex = new Regex(pattern);
        Match match = regex.Match(source);
        if (match.Success)
        {
          wikiStart = match.Index;
          wikiEnd = match.Index + match.Length;
          _wiki = match.Value;
          return true;
        }
      }

      return false;
    }
  }
}