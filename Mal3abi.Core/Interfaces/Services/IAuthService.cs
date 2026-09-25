using Mal3abi.Core.Resources;
using System.Threading.Tasks;

namespace Mal3abi.Core.Interfaces.Services
{
    public interface IAuthService : IScopeInjectable
    {

        Task<AuthResponseResource> RegisterAsync(RegisterRequestResource request);
        Task<AuthResponseResource> LoginAsync(LoginRequestResource request);
        Task<UserResource> GetByIdAsync(string id);
    }
}
