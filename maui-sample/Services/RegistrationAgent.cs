using MauiSample.Models;
using System.Threading.Tasks;

namespace MauiSample
{
  public partial class RegistrationAgent
  {
    private static RegistrationAgent? _instance;
    public static RegistrationAgent Instance => _instance ??= new();
    public partial Task<RegistrationDetails?> RegisterAsync(string applicationID);
    public void Initialize() { }
  }
}
