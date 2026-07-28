using MauiSample.Models;
using System.Threading.Tasks;

namespace MauiSample
{
  public partial class PlatformRequestService
  {
    private static PlatformRequestService? _instance;
    public static PlatformRequestService Instance => _instance ??= new();

    private PlatformRequestService()
    {
      InitializePlatform();
    }

    partial void InitializePlatform();

    public partial Task<RegistrationDetails?> RegisterAsync(string applicationID);
    public partial Task ShowReceiverSelectionAsync();
    public void Initialize() { }
  }
}
