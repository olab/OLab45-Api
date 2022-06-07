using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("SUSPEND")]
public class SuspendWikiTag : WikiTag0Argument
{
  public SuspendWikiTag(OLabLogger logger) : base(logger, "OlabSuspendTag")
  {
  }
}