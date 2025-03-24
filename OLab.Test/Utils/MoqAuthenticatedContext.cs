using OLab.Access;
using OLab.Api.Model;
using OLab.Common.Interfaces;

namespace OLab.Test.Utils;
internal class MoqAuthenticatedContext : AuthenticatedContext
{
  public MoqAuthenticatedContext()
  {

  }
  public MoqAuthenticatedContext(IOLabLogger logger, OLabDBContext dbContext) : base( logger, dbContext )
  {

  }

}
