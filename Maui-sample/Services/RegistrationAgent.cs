using Maui_sample.Models;
using System.Threading.Tasks;

namespace Maui_sample
{
  internal partial class RegistrationAgent
  {
    public partial Task<RegistrationDetails?> RegisterAsync(string applicationID);
  }
}
