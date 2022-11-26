using System;
using System.Collections.Generic;
using System.Text.Json;
using OLabWebAPI.Utils;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a Atrium Update command method
  /// </summary>
  public class AtriumUpdateCommand : CommandMethod
  {
    public IList<AtriumLearner> Data { get; set; }

    public AtriumUpdateCommand(string recipientGroupName, IList<AtriumLearner> atriumLearners) : base(recipientGroupName, "atriumupdate")
    {
      Data = atriumLearners;
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    } 
  }
}