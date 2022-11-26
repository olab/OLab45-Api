namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class AtriumLearner
  {
    public string GroupName { get; private set; }
    public string NickName { get; private set; }

    public AtriumLearner(string groupName, string nickName)
    {
      GroupName = groupName;
      NickName = nickName;
    }

    public override string ToString()
    {
      return $"{NickName}({GroupName})";
    }
  }
}
