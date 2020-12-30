using System.Threading.Tasks;
using Orleans;

namespace Foundry.Web.Grains.GuildBuilder
{
    public interface IGuild : IGrainWithGuidKey
    {
        Task<string> CreateMemberApplication(string applicationData);
    }
}
