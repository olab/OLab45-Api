using OLab.Common.Utils;

namespace OLab.Test
{
  public class WikiTagUtilsTests
  {
    [Fact]
    public void GetTagNamePatterns_ReturnsCorrectPatterns()
    {
      var patterns = WikiTagUtils.GetTagNamePatterns();
      Assert.Equal( 3, patterns.Count );
      Assert.Contains( "\"[.A-Za-z0-9\\- ]*\"", patterns );
      Assert.Contains( "[.A-Za-z0-9\\- ]*", patterns );
      Assert.Contains( "[0-9]*", patterns );
    }

    [Fact]
    public void GetZeroArgumentTagPatterns_ReturnsCorrectPattern()
    {
      var wikiTag = "example";
      var patterns = WikiTagUtils.GetZeroArgumentTagPatterns( wikiTag );
      Assert.Single( patterns );
      Assert.Equal( "\\[\\[example\\]\\]", patterns[ 0 ] );
    }

    [Fact]
    public void GetOneArgumentTagPatterns_ReturnsCorrectPatterns()
    {
      var wikiTag = "example";
      var patterns = WikiTagUtils.GetOneArgumentTagPatterns( wikiTag );
      Assert.Equal( 3, patterns.Count );
      Assert.Contains( "\\[\\[example:[0-9]*\\]\\]", patterns );
      Assert.Contains( "\\[\\[example:\"[.A-Za-z0-9\\- ]*\"\\]\\]", patterns );
      Assert.Contains( "\\[\\[example:[.A-Za-z0-9\\- ]*\\]\\]", patterns );
    }

    [Fact]
    public void GetWikiTags_ReturnsCorrectMatches()
    {
      var wikiTag = "example";
      var source = "This is a test [[example:123]] and another [[example:\"test\"]] and [[example:abc]].";
      var matches = WikiTagUtils.GetWikiTags( wikiTag, source );
      Assert.Equal( 3, matches.Count );
      Assert.Contains( "[[example:123]]", matches );
      Assert.Contains( "[[example:\"test\"]]", matches );
      Assert.Contains( "[[example:abc]]", matches );
    }

    [Fact]
    public void GetWikiArgument1_ReturnsCorrectArgument()
    {
      var wikiTag = "[[example:123]]";
      var argument = WikiTagUtils.GetWikiArgument1( wikiTag );
      Assert.Equal( "123", argument );

      wikiTag = "[[example:\"test\"]]";
      argument = WikiTagUtils.GetWikiArgument1( wikiTag );
      Assert.Equal( "test", argument );

      wikiTag = "[[example:abc]]";
      argument = WikiTagUtils.GetWikiArgument1( wikiTag );
      Assert.Equal( "abc", argument );
    }

    [Fact]
    public void GetWikiArgument1_ReturnsNullForInvalidTag()
    {
      var wikiTag = "[[example:]]";
      var argument = WikiTagUtils.GetWikiArgument1( wikiTag );
      Assert.Empty( argument );
    }
  }
}
