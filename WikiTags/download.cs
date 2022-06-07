using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("DL")]
public class DownloadWikiTag : WikiTag1Argument
{
  public DownloadWikiTag(OLabLogger logger) : base(logger, "OlabDownloadTag")
  {
  }
}