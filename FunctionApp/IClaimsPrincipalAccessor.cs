using System.Security.Claims;

namespace OLab.FunctionApp;

public interface IClaimsPrincipalAccessor
{
#nullable enable
  ClaimsPrincipal? Principal { get; set; }
#nullable disable
}