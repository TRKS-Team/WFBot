using System.Threading.Tasks;

namespace WFBot.Features.Resource
{
    public interface IWFResource
    {
        Task Update();
        Task Reload(bool isFirstTime);
    }
}