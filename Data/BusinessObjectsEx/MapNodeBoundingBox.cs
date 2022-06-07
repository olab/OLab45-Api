using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OLabWebAPI.Model;

namespace OLabWebAPI.Utils
{
  public class MapNodeBoundingBox
  {
    public RectangleF Rect;

    public bool IsEmpty()
    {
      return Rect.IsEmpty;
    }

    /// <summary>
    /// Calculate a transfor vector that moves the current box
    /// to the left of the target box
    /// </summary>
    /// <param name="targetBox">Box to move current box to, in relation</param>
    /// <returns>PointF transformation vector</returns>
    public SizeF CalculateTransformTo(MapNodeBoundingBox targetBox)
    {
      // test if no/empty box to transform to
      if (targetBox.IsEmpty())
        return new SizeF(0, 0);

      float deltaXOrigins;
      float deltaYOrigins;

      if (Rect.Location.X >= targetBox.Rect.Location.X)
        deltaXOrigins = -(Rect.Location.X - targetBox.Rect.Location.X) - (Rect.Width + 60);
      else
        deltaXOrigins = -(targetBox.Rect.Location.X - Rect.Location.X) - (Rect.Width + 60);

      if (Rect.Location.Y >= targetBox.Rect.Location.Y)
        deltaYOrigins = Rect.Location.Y - targetBox.Rect.Location.Y;
      else
        deltaYOrigins = targetBox.Rect.Location.Y - Rect.Location.Y;

      return new SizeF(deltaXOrigins, deltaYOrigins);

    }

    public void Load(List<MapNodes> nodes)
    {
      var minX = nodes.Min(p => p.X);
      var minY = nodes.Min(p => p.Y);
      var maxX = nodes.Max(p => p.Width + p.X);
      var maxY = nodes.Max(p => p.Height + p.Y);

      Rect = RectangleF.FromLTRB((float)minX, (float)minY, (float)maxX, (float)maxY);
    }
  }
}