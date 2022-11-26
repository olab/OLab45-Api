using System;

namespace OLabWebAPI.Services.TurkTalk.Contracts
{
  public class LearnerGroupName
  {
    private string _topicName;
    private string _userName;
    private string _nickName;
    private int? _roomIndex;
    private readonly string _connectionId;

    public string Name { get { return _userName; } }
    public string TopicName { get { return _topicName; } }
    public string NickName { get { return _nickName; } }

    public string GroupName
    {
      get
      {
        if (_roomIndex.HasValue)
          return $"{_topicName}/{_roomIndex.Value}/learner/{_userName}";
        return $"{_topicName}//learner/{_userName}";
      }
    }

    public LearnerGroupName(string topicName, string userName = null, string nickName = null, string connectionId = null)
    {
      var topicNameParts = topicName.Split("/");

      // test not a multipart topic, then this learner is for the atrium
      if (topicNameParts.Length == 1)
      {
        _topicName = topicName;
        _userName = userName;
        _nickName = nickName;
      }
      else
      {
        _topicName = topicNameParts[0];
        // if not room index passed in, this this is an 
        // atrium group, not an actual room
        if (!string.IsNullOrEmpty(topicNameParts[1]))
          _roomIndex = Convert.ToInt32(topicNameParts[1]);
        _userName = topicNameParts[3];
      }

      _connectionId = connectionId;

    }

    public void AssignToRoom(int index)
    {
      _roomIndex = index;
    }

    public override string ToString()
    {
      if ( string.IsNullOrEmpty( _connectionId ))
        return GroupName;
      return $"{GroupName} id: {_connectionId}";
    }
  }
}