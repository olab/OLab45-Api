using OLab.Api.Utils;

namespace OLab.Test;

public class GenericValidationsTests
{
  [Theory]
  [InlineData( "validUsername", true )]
  [InlineData( "user_name", true )]
  [InlineData( "user-name", true )]
  [InlineData( "user123", true )]
  [InlineData( "u", false )]
  [InlineData( "", false )]
  [InlineData( " ", false )]
  [InlineData( null, false )]
  public void IsValidUsername_ReturnsExpectedResult(string username, bool expected)
  {
    var result = GenericValidations.IsValidUsername( username );
    Assert.Equal( expected, result );
  }

  [Theory]
  [InlineData( "test@example.com", true )]
  [InlineData( "user.name+tag+sorting@example.com", true )]
  [InlineData( "user@sub.example.com", true )]
  [InlineData( "user@localserver", false )]
  [InlineData( "user@.com", false )]
  [InlineData( "user@com", false )]
  [InlineData( "user@.com.com", false )]
  [InlineData( "user@com..com", false )]
  [InlineData( "user@-example.com", false )]
  [InlineData( "", false )]
  [InlineData( " ", false )]
  [InlineData( null, false )]
  public void IsValidEmail_ReturnsExpectedResult(string email, bool expected)
  {
    var result = GenericValidations.IsValidEmail( email );
    Assert.Equal( expected, result );
  }
}
