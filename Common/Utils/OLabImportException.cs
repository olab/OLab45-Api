using System;

public class OLabImportException : Exception
{
  public override string Message { get; }

  public OLabImportException(string fileName, int recordId, uint sourceId, string message)
  {
    Message = $"Exception: {fileName}({recordId}:{sourceId}): {message}";
  }
}