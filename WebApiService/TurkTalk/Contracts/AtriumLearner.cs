namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class AtriumLearner
  {
    private readonly string _connectionId;

    public string GroupName { get; private set; }
    public string NickName { get; private set; }

    public AtriumLearner(LearnerGroupName learner)
    {
      GroupName = learner.Group;
      NickName = learner.NickName;
      _connectionId = learner.ConnectionId;
    }

    public override string ToString()
    {
      return $"{NickName}({GroupName}) id: {_connectionId}";
    }
  }
}
