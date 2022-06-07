using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("MODERATOR")]
public class ModeratorWikiTag : WikiTag1Argument
{
  public ModeratorWikiTag(OLabLogger logger) : base(logger, "OlabModeratorTag")
  {
  }
}