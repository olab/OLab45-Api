using System;
using Microsoft.AspNetCore.Http;

namespace OLabWebAPI.Dto
{

  public class FilesFullDto : FilesDto
  {
    public FilesFullDto()
    {


    }
    public FilesFullDto(IFormCollection form)
    {
      Id = Convert.ToUInt32(form["id"]);
      Name = form["name"];
      Description = form["description"];
      Copyright = form["copyright"];
      ImageableId = Convert.ToUInt32(form["parentId"]);
      ImageableType = form["scopeLevel"];
      IsMediaResource = Convert.ToBoolean(form["isMediaResource"]);
      SelectedFileName = form["selectedFileName"];
      FileSize = Convert.ToInt32(form["fileSize"]);
      CreatedAt = DateTime.UtcNow;
    }

    public IFormFile FileContents { get; set; }
    public int? FileSize { get; set; }
    public int? Height { get; set; }
    public int? IsSystem { get; set; }
    public int? Type { get; set; }
    public int? Width { get; set; }
    public sbyte IsPrivate { get; set; }
    public sbyte IsShared { get; set; }
    public sbyte? IsEmbedded { get; set; }
    public string Args { get; set; }
    public string Copyright { get; set; }
    public string FileName { get; set; }
    public string HAlign { get; set; }
    public string HeightType { get; set; }
    public string Mime { get; set; }
    public string OriginUrl { get; set; }
    public string Path { get; set; }
    public string VAlign { get; set; }
    public string WidthType { get; set; }
    public string SelectedFileName { get; set; }
    public bool IsMediaResource { get; set; }
  }
}