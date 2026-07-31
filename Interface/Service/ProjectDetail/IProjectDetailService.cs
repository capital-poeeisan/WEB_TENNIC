using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Services
{
    public interface IProjectDetailService
    {
        ProjectDetailViewModel GetProjectList(string projectcd);

        ProjectDetailViewModel GetStaffList(string projectcd);

        ProjectDetailViewModel GetData(string projectCd, List<string> staffCDs);

        Task SaveProjectDetailsAsync(ProjectDetailViewModel details, bool endFlg, string projectCD);
    }
}
