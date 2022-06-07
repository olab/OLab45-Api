using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("MR")]
public class MediaResourceWikiTag : WikiTag1Argument
{
  public MediaResourceWikiTag(OLabLogger logger) : base(logger, "OlabMediaResourceTag")
  {
  }
}