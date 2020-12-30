using System.Threading.Tasks;
using Orleans;

namespace Foundry.Web.Grains.Authentication
{
    public interface IAuthentication : IGrainWithGuidKey
    {
        Task<UserDetails> Login(LoginParameters loginParameters);

        Task<UserDetails> Register();

        Task Logout();
    }
}
