using Maui_sample.Models;
using System.Threading.Tasks;

namespace Maui_sample
{
  public partial class RegistrationAgent
  {
    private static RegistrationAgent? _instance;
    public static RegistrationAgent Instance => _instance ??= new();
    public partial Task<RegistrationDetails?> RegisterAsync(string applicationID);
    public void Initialize() { }
  }
}
