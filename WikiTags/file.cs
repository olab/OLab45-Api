using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("FILE")]
public class FileWikiTag : WikiTag1Argument
{
  public FileWikiTag(OLabLogger logger) : base(logger, "OlabFileTag")
  {
  }
}