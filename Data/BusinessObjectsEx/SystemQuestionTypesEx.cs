using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLabWebAPI.Model
{
  public partial class SystemQuestionTypes
  {
    public enum Type
    {
      SingleLineText = 1,
      MultiLineText = 2,
      MultipleChoice = 3,
      Radio = 4,
      Slider = 5,
      DragAndDrop = 6,
      SCT = 7,
      SJT = 8,
      Area = 9,
      Ttalk = 11,
      DropDown = 12,
      MCQGrid = 13,
      PCQGrid = 14
    };
  }
}
