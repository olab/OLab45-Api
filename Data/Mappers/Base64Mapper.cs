using AutoMapper;

namespace OLabWeOLabWebAPI.ObjectMapper
{
  public class Base64Converter : ITypeConverter<string, byte[]>, ITypeConverter<byte[], string>
  {
    public byte[] Convert(string source, byte[] destination, ResolutionContext context)
        => System.Convert.FromBase64String(source);

    public string Convert(byte[] source, string destination, ResolutionContext context)
        => System.Convert.ToBase64String(source);
  }
}