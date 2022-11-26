namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class AtriumLearner
  {
    private readonly string _connectionId;

    public string GroupName { get; private set; }
    public string NickName { get; private set; }

    public AtriumLearner(string groupName, string nickName, string connectionId = null)
    {
      GroupName = groupName;
      NickName = nickName;
      _connectionId = connectionId;
    }

    public override string ToString()
    {
      return $"{NickName}({GroupName}) id: {_connectionId}";
    }
  }
}
