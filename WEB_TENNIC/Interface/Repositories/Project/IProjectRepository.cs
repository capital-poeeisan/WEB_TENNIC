using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Repositories.Project
{
    public interface IProjectRepository
    {       

        Task<List<ProjectViewModel>> GetProjectList(int EndFlg);
        
    }
}
