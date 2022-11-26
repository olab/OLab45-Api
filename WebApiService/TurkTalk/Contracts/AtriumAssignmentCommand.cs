using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class AtriumAssignmentCommand : CommandMethod
  {
    public AtriumLearner Data { get; set; }

    public AtriumAssignmentCommand(string recipientGroupName, AtriumLearner atrium) : base(recipientGroupName, "atriumassignment")
    {
      Data = atrium;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    } 
  }
}