using OLab.Api.Importer;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace OLab.Test;

public class DynamicXmlTests
{
  [Fact]
  public void Parse_ValidXmlString_ReturnsDynamicXml()
  {
    string xmlString = "<root><element>value</element></root>";
    var dynamicXml = DynamicXml.Parse( xmlString );

    Assert.NotNull( dynamicXml );
    Assert.Equal( "value", dynamicXml._root.Element( "element" ).Value );
  }

  [Fact]
  public void Load_ValidFileName_ReturnsDynamicXml()
  {
    string fileName = "test.xml";
    File.WriteAllText( fileName, "<root><element>value</element></root>" );

    var dynamicXml = DynamicXml.Load( fileName );

    Assert.NotNull( dynamicXml );
    Assert.Equal( "value", dynamicXml._root.Element( "element" ).Value );

    File.Delete( fileName );
  }

  [Fact]
  public void Load_FileNotFound_ThrowsFileNotFoundException()
  {
    Assert.Throws<FileNotFoundException>( () => DynamicXml.Load( "nonexistent.xml" ) );
  }

  [Fact]
  public void Load_ValidStream_ReturnsDynamicXml()
  {
    using ( var stream = new MemoryStream() )
    using ( var writer = new StreamWriter( stream ) )
    {
      writer.Write( "<root><element>value</element></root>" );
      writer.Flush();
      stream.Position = 0;

      var dynamicXml = DynamicXml.Load( stream );

      Assert.NotNull( dynamicXml );
      Assert.Equal( "value", dynamicXml._root.Element( "element" ).Value );
    }
  }

  [Fact]
  public void TryGetMember_ExistingAttribute_ReturnsAttributeValue()
  {
    string xmlString = "<root attribute='value'></root>";
    dynamic dynamicXml = DynamicXml.Parse( xmlString );

    Assert.Equal( "value", dynamicXml.attribute );
  }

  [Fact]
  public void TryGetMember_ExistingElement_ReturnsElementValue()
  {
    string xmlString = "<root><element>value</element></root>";
    dynamic dynamicXml = DynamicXml.Parse( xmlString );

    Assert.Equal( "value", dynamicXml.element );
  }

  [Fact]
  public void TryGetMember_MultipleElements_ReturnsList()
  {
    string xmlString = "<root><element>value1</element><element>value2</element></root>";
    dynamic dynamicXml = DynamicXml.Parse( xmlString );

    var elements = dynamicXml.element as List<object>;

    Assert.NotNull( elements );
    Assert.Equal( 2, elements.Count );
    Assert.Equal( "value1", elements[ 0 ] );
    Assert.Equal( "value2", elements[ 1 ] );
  }

  [Fact]
  public void TryGetMember_NonExistingMember_ReturnsNull()
  {
    string xmlString = "<root></root>";
    dynamic dynamicXml = DynamicXml.Parse( xmlString );

    Assert.Null( dynamicXml.nonExistingMember );
  }
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
