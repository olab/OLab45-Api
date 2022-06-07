using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("JUMP")]
public class JumpWikiTag : WikiTag1Argument
{
  public JumpWikiTag(OLabLogger logger) : base(logger, "OlabJumpTag")
  {
  }
}