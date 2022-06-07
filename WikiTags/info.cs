using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("INFO")]
public class InfoWikiTag : WikiTag1Argument
{
  public InfoWikiTag(OLabLogger logger) : base(logger, "OlabInfoTag")
  {
  }
}