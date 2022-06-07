using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace OLabWebAPI.Utils
{
  public static class Constants
  {
    public const string ScopeLevelNode = "Nodes";
    public const string ScopeLevelMap = "Maps";
    public const string ScopeLevelServer = "Servers";
    public const string DefaultConnectionStringName = "DefaultDatabase";
    public const string WebsitePublicFilesDirectoryName = "WebsitePublicFilesDirectory";

    public const string ReservedConstantSystemTime = "SystemTime";
    public const string ReservedConstantMapName = "MapName";
    public const string ReservedConstantMapId = "MapId";
    public const string ReservedConstantNodeName = "NodeName";
    public const string ReservedConstantNodeId = "NodeId";

    public const string AuthTokenIssuer = "OLab4";

  }
}