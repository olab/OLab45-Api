using System;
using OLabWebAPI.Common;
using OLabWebAPI.Utils;

[WikiTagModule("QU")]
public class QuestionWikiTag : WikiTag1Argument
{
  public QuestionWikiTag(OLabLogger logger) : base(logger, "OlabQuestionTag")
  {
  }

}