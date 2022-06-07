using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("COUNTERS")]
public class CountersWikiTag : WikiTag0Argument
{
  public CountersWikiTag(OLabLogger logger) : base(logger, "OlabCountersTag")
  {
  }
}