using System.Threading.Tasks;

namespace WFBot.Features.Resource
{
    public interface IWFResource
    {
        Task<bool> Update();
        Task Reload(bool isFirstTime);
    }
}