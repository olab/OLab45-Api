using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("VPD")]
public class VpdWikiTag : WikiTag1Argument
{
  public VpdWikiTag(OLabLogger logger) : base(logger, "")
  {
  }
}