using System.Threading.Tasks;
using OLabWebAPI.Model;

namespace OLabWebAPI.Importer
{
  public interface IImporter
  {
    public bool LoadAll(string archiveFileName);
    public bool SaveAll();
  }
}