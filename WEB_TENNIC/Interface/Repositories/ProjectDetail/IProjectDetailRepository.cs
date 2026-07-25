using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Repositories
{
    public interface IProjectDetailRepository
    {
        List<WtMProject> GetProjects();

        List<ProjectStaffViewModel> GetStaffList(string projectCd);
        List<ProjectInputViewModel> GetProgressDetail(string projectCd, List<string> staffCDs);

        // Table 2
        List<ProjectSummaryViewModel> GetSummary(string projectCd);

        Task SaveProjectDetailsAsync(ProjectDetailViewModel details, bool endFlg, string projectCD);
    }
}

