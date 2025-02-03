using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OLab.Api.Utils;
using System.Collections.Generic;
using Xunit;

namespace OLab.Test
{
  public class QueryStringUtilsTests
  {
    private QueryStringUtils CreateQueryStringUtils(Dictionary<string, StringValues> queryParams)
    {
      var queryCollection = new QueryCollection( queryParams );
      var httpRequest = new DefaultHttpContext().Request;
      httpRequest.Query = queryCollection;
      return new QueryStringUtils( httpRequest );
    }

    [Fact]
    public void Get_WithExistingKey_ReturnsValue()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "value1" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      var result = queryStringUtils.Get( "key1" );

      Assert.Equal( "value1", result );
    }

    [Fact]
    public void Get_WithNonExistingKey_ThrowsKeyNotFoundException()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "value1" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      Assert.Throws<KeyNotFoundException>( () => queryStringUtils.Get( "key2" ) );
    }

    [Fact]
    public void GetOptional_WithExistingKey_ReturnsValue()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "value1" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      var result = queryStringUtils.GetOptional( "key1", "default" );

      Assert.Equal( "value1", result );
    }

    [Fact]
    public void GetOptional_WithNonExistingKey_ReturnsDefaultValue()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "value1" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      var result = queryStringUtils.GetOptional( "key2", "default" );

      Assert.Equal( "default", result );
    }

    [Fact]
    public void GetOptionalInt_WithExistingKey_ReturnsValue()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "123" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      var result = queryStringUtils.GetOptional( "key1", 0 );

      Assert.Equal( 123, result );
    }

    [Fact]
    public void GetOptionalInt_WithNonExistingKey_ReturnsDefaultValue()
    {
      var queryParams = new Dictionary<string, StringValues> { { "key1", "123" } };
      var queryStringUtils = CreateQueryStringUtils( queryParams );

      var result = queryStringUtils.GetOptional( "key2", 0 );

      Assert.Equal( 0, result );
    }
  }
}
