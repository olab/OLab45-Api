using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  /// <summary>
  /// Defines a command method
  /// </summary>
  public abstract class CommandMethod : Method
  {
    public string Command { get; set; }

    public CommandMethod(string recipientGroupName, string command) : base(recipientGroupName, "Command")
    {
      Command = command;
    }
  }
}