using Moq;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.ReaderWriters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq.EntityFrameworkCore;
using OLab.Test.Utils;
using Microsoft.VisualBasic;

namespace OLab.Test.ReaderWriters;

public class QuestionReaderWriterTests
{
  private readonly Mock<IOLabLogger> _mockLogger;
  private readonly Mock<OLabDBContext> _mockDbContext;
  private readonly Mock<IOLabModuleProvider<IWikiTagModule>> _mockWikiTagProvider;
  private readonly QuestionReaderWriter _questionReaderWriter;

  public QuestionReaderWriterTests()
  {
    _mockLogger = new Mock<IOLabLogger>();
    _mockDbContext = new Mock<OLabDBContext>();
    _mockWikiTagProvider = new Mock<IOLabModuleProvider<IWikiTagModule>>();

    TestUtilities.MoqSystemQuestionsFromJsonFile( _mockDbContext, "json\\SystemQuestions.json" );
    _questionReaderWriter = new QuestionReaderWriter( _mockLogger.Object, _mockDbContext.Object, _mockWikiTagProvider.Object );
  }

  [Fact]
  public void Instance_WithValidParameters_ReturnsInstance()
  {
    var instance = QuestionReaderWriter.Instance( _mockLogger.Object, _mockDbContext.Object, _mockWikiTagProvider.Object );
    Assert.NotNull( instance );
  }

  [Fact]
  public void Get_WithValidMapId_ReturnsSystemQuestions()
  {
    var result = _questionReaderWriter.Get( 0, 1604, "3574" );

    Assert.NotNull( result );
    Assert.True( result.Id == 3574 );
  }

  [Fact]
  public void Get_WithValidNodeId_ReturnsSystemQuestions()
  {
    var result = _questionReaderWriter.Get( 30895, 0, "p3NC3" );

    Assert.NotNull( result );
    Assert.True( result.Id == 3563 );
  }

  [Fact]
  public void Get_WithValidName_ReturnsSystemQuestions()
  {
    var result = _questionReaderWriter.Get( 0, 1604, "3574" );

    Assert.NotNull( result );
    Assert.Equal( "3574", result.Name );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithRadioQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:p3NC3]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 30895, 0, source );

    Assert.Contains( "[[QUSP:p3NC3]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithSliderQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:2914]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1063, source );

    Assert.Contains( "[[QUSD:2914]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithDragDropQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:3593]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1610, source );

    Assert.Contains( "[[QUDG:3593]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithDropDownQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:3597]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1610, source );

    Assert.Contains( "[[QUDP:3597]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithSingleLineQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:3619]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1614, source );

    Assert.Contains( "[[QUST:3619]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithMultiLineQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:3623]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1614, source );

    Assert.Contains( "[[QUMT:3623]]", result );
  }

  [Fact]
  public void DisambiguateWikiQuestions_WithMultiChoiceQuestion_ReturnsDisambiguatedSource()
  {
    var source = "Some text with [[QU:3644]]";
    var result = _questionReaderWriter.DisambiguateWikiQuestions( 0, 1620, source );

    Assert.Contains( "[[QUMP:3644]]", result );
  }

  [Fact]
  public async Task GetAsync_WithValidScopeLevelAndId_ReturnsSystemQuestions()
  {
    var result = await _questionReaderWriter.GetAsync( OLab.Api.Utils.Constants.ScopeLevelServer, 1 );
    Assert.Equal( 15, result.Count() );
  }
}
