using System;
using System.Collections.Generic;
using System.Text;

namespace OLabWebAPI.Common
{
  [AttributeUsage(AttributeTargets.Class)]
  public class WikiTagModuleAttribute : Attribute
  {
    private string _wikiTag;

    public WikiTagModuleAttribute(string wikiTag)
    {
      _wikiTag = wikiTag;
    }

    public string WikiTag
    {
      get { return _wikiTag; }
      set { _wikiTag = value; }
    }
  }
}
