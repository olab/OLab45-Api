using Moq;
using OLab.Api.Common;
using OLab.Common.Attributes;
using OLab.Common.Interfaces;

namespace OLab.Test;

public class WikiTag1ArgumentModuleTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<IOLabConfiguration> _mockConfiguration;
  private readonly TestWikiTag1ArgumentModule _wikiTagModule;

  public WikiTag1ArgumentModuleTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockConfiguration = new Mock<IOLabConfiguration>();
    _wikiTagModule = new TestWikiTag1ArgumentModule( _mockLogger.Object, _mockConfiguration.Object );

    var elementName = "testElement";
    _wikiTagModule.SetHtmlElementName( elementName );
  }

  [Fact]
  public void SetHtmlElementName_WithValidElementName_SetsHtmlElementName()
  {
    _wikiTagModule.SetHtmlElementName( "testElement" );
    Assert.Equal( "testElement", _wikiTagModule.GetHtmlElementName() );
  }

  [Fact]
  public void Set_WithValidWikiTypeAndWikiId_SetsWikiTypeAndWikiId()
  {
    var result = _wikiTagModule.Set( "QU", "testId" );
    Assert.Equal( "[[QU:testId]]", result );
  }

  [Fact]
  public void GetWikiId_ReturnsWikiId()
  {
    _wikiTagModule.SetWikiId( "testId" );
    var result = _wikiTagModule.GetWikiId();
    Assert.Equal( "testId", result );
  }

  [Fact]
  public void GetWikiArgument1_WithValidWikiTag_ReturnsArgument1()
  {
    var result = _wikiTagModule.GetWikiArgument1( "[[QU:testId]]" );
    Assert.Equal( "testId", result );
  }

  [Fact]
  public void HaveWikiTag_WithValidSource_ReturnsTrue()
  {
    var result = _wikiTagModule.HaveWikiTag( "[[QU:testId]]" );
    Assert.True( result );
  }

  [Fact]
  public void PreviewNewArgument1_WithValidArgument_ReturnsPreview()
  {
    var result = _wikiTagModule.PreviewNewArgument1( "newArgument" );
    Assert.Equal( "[[QU:newArgument]]", result );
  }

  [Fact]
  public void Translate_WithValidSource_ReplacesWikiTag()
  {
    var result = _wikiTagModule.Translate( "[[QU:testId]]" );
    Assert.Contains( "QU:testId", result );
  }

  [OLabModule( "QU" )]
  private class TestWikiTag1ArgumentModule : WikiTag1ArgumentModule
  {
    public TestWikiTag1ArgumentModule(IOLabLogger logger, IOLabConfiguration configuration) : base( logger, configuration ) { }

    public new void SetHtmlElementName(string elementName) => base.SetHtmlElementName( elementName );
    public new string Set(string wikiType, string wikiId) => base.Set( wikiType, wikiId );
    public new string GetWikiId() => base.GetWikiId();
    public new void SetWikiId(string wikiTag) => base.SetWikiId( wikiTag );
    public new string GetWikiArgument1(string wikiTag) => base.GetWikiArgument1( wikiTag );
    public new bool HaveWikiTag(string source) => base.HaveWikiTag( source );
    public new string PreviewNewArgument1(string argument) => base.PreviewNewArgument1( argument );
    public new string Translate(string source) => base.Translate( source );
  }
}
