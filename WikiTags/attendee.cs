using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("ATTENDEE")]
public class AttendeeWikiTag : WikiTag1Argument
{
  public AttendeeWikiTag(OLabLogger logger) : base(logger, "OlabAttendeeTag")
  {
  }
}