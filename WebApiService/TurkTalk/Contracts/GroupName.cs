using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public abstract class GroupName
  {
    private string _topicName;
    private string _userId;
    private string _nickName;
    private int? _roomIndex;
    private string _prefix;
    private readonly string _connectionId;

    public string UserId { get { return _userId; } }
    public string TopicName { get { return _topicName; } }
    public string NickName { get { return _nickName; } }
    public string ConnectionId { get { return _connectionId; } }

    public string Group
    {
      get
      {
        if (_roomIndex.HasValue)
          return $"{TopicName}/{_roomIndex.Value}/{_prefix}/{UserId}";
        return $"{TopicName}//{_prefix}/{UserId}";
      }
    }

    public GroupName(string prefix, string topicName, string userName, string nickName, string connectionId)
    {
      _prefix = prefix;
      _connectionId = connectionId;

      var topicNameParts = topicName.Split("/");

      // test not a multipart topic, then this learner is for the atrium
      if (topicNameParts.Length == 1)
      {
        _topicName = topicName;
        _userId = userName;
        _nickName = nickName;
      }
      else
      {
        _topicName = topicNameParts[0];
        // if not room index passed in, this this is an 
        // atrium group, not an actual room
        if (!string.IsNullOrEmpty(topicNameParts[1]))
          _roomIndex = Convert.ToInt32(topicNameParts[1]);
        _userId = topicNameParts[3];
      }

    }

    public void AssignToRoom(int index)
    {
      _roomIndex = index;
    }

    public override string ToString()
    {
      return $"{Group} Id: {ConnectionId}";
    }

  }
}