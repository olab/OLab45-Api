using System;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;

// https://stackoverflow.com/questions/13704752/deserialize-xml-to-object-using-dynamic

namespace OLabWeOLabWebAPI.ObjectMapper
{
  public class DynamicXml : DynamicObject
  {
    public readonly XElement _root;

    public DynamicXml()
    {
    }

    private DynamicXml(XElement root)
    {
      _root = root;
    }

    public static DynamicXml Parse(string xmlString)
    {
      return new DynamicXml(XDocument.Parse(xmlString).Root);
    }

    public static DynamicXml Load(string filename)
    {
      if (File.Exists(filename))
        return new DynamicXml(XDocument.Load(filename).Root);

      throw new FileNotFoundException("File not found", filename);
    }

    public IEnumerable<XElement> Elements() { return _root.Elements(); }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      result = null;

      var att = _root.Attribute(binder.Name);
      if (att != null)
      {
        result = att.Value;
        return true;
      }

      var nodes = _root.Elements(binder.Name);
      if (nodes.Count() > 1)
      {
        result = nodes.Select(n => n.HasElements ? (object)new DynamicXml(n) : n.Value).ToList();
        return true;
      }

      var node = _root.Element(binder.Name);
      if (node != null)
      {
        result = node.HasElements || node.HasAttributes ? (object)new DynamicXml(node) : node.Value;
        return true;
      }

      return true;
    }
  }
}