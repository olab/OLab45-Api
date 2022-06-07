using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace OLabWebAPI.Model
{
  public partial class MapNodes
  {
    public static MapNodes CreateDefault()
    {
      return new MapNodes
      {
        Title = "New Node",
        Text = "<b>New node text</b>",
        X = 0,
        Y = 0,
        Locked = 0,
        Collapsed = 0,
        Height = 160,
        Width = 300,
        Probability = false,
        Info = "",
        Annotation = "",
        LinkStyleId = 5,
        LinkTypeId = 1,
        TypeId = 2,
        IsPrivate = 0,
        VisitOnce = 0,
        PriorityId = 1,
        End = false,
        Rgb = "#566e94",
        CreatedAt = DateTime.UtcNow,
      };
    }

    public static bool Reassign(Maps destinationMap, MapNodes node)
    {
      // test if node has already been re-assigned
      if (node.Id == 0)
        return false;

      node.Id = 0;
      node.MapId = destinationMap.Id;

      return true;
    }

  }
}
