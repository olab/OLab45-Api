using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("CR")]
public class CounterWikiTag : WikiTag1Argument
{
  public CounterWikiTag(OLabLogger logger) : base(logger, "OlabCounterTag")
  {
  }
}