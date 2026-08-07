using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Repositories.Project
{
    public interface IProjectRepository
    {       

        Task<List<ProjectViewModel>> GetProjectList(int EndFlg);
        Task<int> WT_Logging_Delete(ProjectViewModel m);
        Task<bool> DeleteProjectName(string id);
    }
}
