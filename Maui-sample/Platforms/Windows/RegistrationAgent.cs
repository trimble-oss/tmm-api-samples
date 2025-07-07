using Maui_sample.Models;
using System.Threading.Tasks;

namespace Maui_sample;

internal partial class RegistrationAgent
{
    public RegistrationAgent()
    {
    }

    public partial Task<RegistrationDetails?> RegisterAsync(string applicationID)
    {

        return Task.FromResult<RegistrationDetails?>(null);
    }
}
