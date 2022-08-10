using System.Threading.Tasks;

namespace WFBot.Features.Resource
{
    public interface IWFResource
    {
        Task Update();
        Task Reload(bool isFirstTime);
        public string Category { get; }
        public bool IsGitHub { get; }

    }
}