using Moq;
using OLab.Api.Common;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;

namespace OLab.Test;

public class WikiTag0ArgumentModuleTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<IOLabConfiguration> _mockConfiguration;
  private readonly TestWikiTag0ArgumentModule _wikiTagModule;

  public WikiTag0ArgumentModuleTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockConfiguration = new Mock<IOLabConfiguration>();
    _wikiTagModule = new TestWikiTag0ArgumentModule( _mockLogger.Object, _mockConfiguration.Object );
  }

  [Fact]
  public void SetHtmlElementName_SetsElementNameAndPatterns()
  {
    // Arrange
    var elementName = "testElement";

    // Act
    _wikiTagModule.SetHtmlElementName( elementName );

    // Assert
    Assert.Equal( elementName, _wikiTagModule.GetHtmlElementName() );
    Assert.NotEmpty( _wikiTagModule.WikiTagPatterns );
  }

  [Fact]
  public void BuildWikiTagHTMLElement_ReturnsCorrectHtmlElement()
  {
    // Arrange
    _wikiTagModule.SetHtmlElementName( "testElement" );

    // Act
    var result = _wikiTagModule.BuildWikiTagHTMLElement();

    // Assert
    Assert.Contains( "<testElement", result );
    Assert.Contains( "class=\"testElement\"", result );
    Assert.Contains( "props={props}", result );
  }

  [Fact]
  public void Translate_WithNoWikiTag_ReturnsSourceUnchanged()
  {
    // Arrange
    var source = "No wiki tag here";

    // Act
    var result = _wikiTagModule.Translate( source );

    // Assert
    Assert.Equal( source, result );
  }

  [Fact]
  public void Translate_WithWikiTag_ReplacesWithHtmlElement()
  {
    // Arrange
    var source = "This is a [[LINKS]] tag";
    _wikiTagModule.SetHtmlElementName( "testElement" );

    // Act
    var result = _wikiTagModule.Translate( source );

    // Assert
    Assert.Contains( "<testElement", result );
    Assert.Contains( "class=\"testElement\"", result );
    Assert.Contains( "props={props}", result );
  }

  [OLabModule( "LINKS" )]
  private class TestWikiTag0ArgumentModule : WikiTag0ArgumentModule
  {
    public TestWikiTag0ArgumentModule(IOLabLogger logger, IOLabConfiguration configuration)
        : base( logger, configuration ) { }

    public new string BuildWikiTagHTMLElement() => base.BuildWikiTagHTMLElement();
    public new string GetHtmlElementName() => base.GetHtmlElementName();
    public new void SetHtmlElementName(string elementName) => base.SetHtmlElementName( elementName );
    public new string Translate(string source) => base.Translate( source );
    public List<string> WikiTagPatterns => wikiTagPatterns;
  }
}
