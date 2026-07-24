using System.Threading.Tasks;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Service.Project
{
    public interface IProjectService
    {
        
        Task<List<ProjectViewModel>> GetProjectList(int EndFlg);
        
    }
}
