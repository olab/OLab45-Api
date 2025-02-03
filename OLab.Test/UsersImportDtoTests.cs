using OLab.Api.Model;
using OLab.Data.Dtos;
using Xunit;

namespace OLab.Test;

public class UsersImportDtoTests
{
  [Fact]
  public void UsersImportDto_DefaultConstructor_SetsStatusToTrue()
  {
    var dto = new UsersImportDto();
    Assert.True( dto.Status );
  }

  [Fact]
  public void UsersImportDto_ParameterizedConstructor_CopiesPropertiesFromUsersDto()
  {
    var userDto = new UsersDto
    {
      Id = 1,
      NickName = "Nick",
      UserName = "User",
      Email = "user@example.com",
      Roles = new List<UserGroupRolesDto> { new UserGroupRolesDto { Id = 1, RoleId = 1, GroupId = 1 } }
    };

    var dto = new UsersImportDto( userDto );

    Assert.Equal( userDto.Id, dto.Id );
    Assert.Equal( userDto.NickName, dto.NickName );
    Assert.Equal( userDto.UserName, dto.UserName );
    Assert.Equal( userDto.Email, dto.Email );
    Assert.Equal( userDto.Roles.Count, dto.Roles.Count );
    Assert.Equal( userDto.Roles[ 0 ].Id, dto.Roles[ 0 ].Id );
  }

  [Fact]
  public void UsersImportDto_MessageProperty_CanBeSetAndGet()
  {
    var dto = new UsersImportDto();
    var message = "Test message";
    dto.Message = message;
    Assert.Equal( message, dto.Message );
  }
}
