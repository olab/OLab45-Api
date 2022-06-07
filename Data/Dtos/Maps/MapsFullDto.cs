using System;

namespace OLabWebAPI.Dto
{
  public class MapsFullDto : MapsDto
  {
    public bool Enabled { get; set; }
    public bool RevisableAnswers { get; set; }
    public bool SendXapiStatements { get; set; }
    public bool ShowBar { get; set; }
    public bool ShowScore { get; set; }
    public bool Timing { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public float? RendererVersion { get; set; }
    public int AuthorRights { get; set; }
    public int DeltaTime { get; set; }
    public int ReminderTime { get; set; }
    public int StartScore { get; set; }
    public int Threshold { get; set; }
    public int? AssignForumId { get; set; }
    public int? IsTemplate { get; set; }
    public string Abstract { get; set; }
    public string Author { get; set; }
    public string DevNotes { get; set; }
    public string Feedback { get; set; }
    public string Guid { get; set; }
    public string Keywords { get; set; }
    public string ReminderMsg { get; set; }
    public string Source { get; set; }
    public string Title { get; set; }
    public string Units { get; set; }
    public string Verification { get; set; }
    public uint SectionId { get; set; }
    public uint SecurityId { get; set; }
    public uint SkinId { get; set; }
    public uint SourceId { get; set; }
    public uint TypeId { get; set; }
    public uint? LanguageId { get; set; }
  }
}
