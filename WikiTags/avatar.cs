using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("AV")]
public class AvatarWikiTag : WikiTag1Argument
{
  public AvatarWikiTag(OLabLogger logger) : base(logger, "OlabAvatarTag")
  {
  }

}