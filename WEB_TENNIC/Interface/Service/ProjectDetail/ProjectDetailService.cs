using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_TENNIC.Interface.Repositories;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Services
{
    public class ProjectDetailService: IProjectDetailService
    {
        private readonly IProjectDetailRepository _repository;

        public ProjectDetailService(IProjectDetailRepository repository)
        {
            _repository = repository;
        } 


        public ProjectDetailViewModel GetProjectList()
        {
            var model = new ProjectDetailViewModel();
            // =========================
            // Project Dropdown
            // =========================

            var projects = _repository.GetProjects();


            model.ProjectList = projects
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectCd,
                    Text = p.ProjectName
                })
                .ToList();


            return model;
        } 

        public ProjectDetailViewModel GetStaffList(string projectCd)
        {
            var model = new ProjectDetailViewModel();
            if (!string.IsNullOrEmpty(projectCd))
            {
                var staffs = _repository
                .GetStaffList(projectCd);

                model.StaffList = staffs
               .Select(s => new SelectListItem
               {
                   Value = s.StaffCD,
                   Text = s.StaffName
               })
               .ToList();
            }

            return model;
        }

        public ProjectDetailViewModel GetData(string projectCd, List<string> staffCDs)
        {

            var model = new ProjectDetailViewModel();
            //// =========================
            //// Table 1
            //// =========================

            model.ProjectProgress =
                _repository.GetProgressDetail(projectCd, staffCDs);

            // =========================
            // Table 2
            // =========================
            if (!string.IsNullOrEmpty(projectCd))
            {
                model.SummaryList =
              _repository.GetSummary(projectCd);
            }
            return model;

        }
        public async Task SaveProjectDetailsAsync(ProjectDetailViewModel details, bool endFlg, string projectCD)
        {
            if (details.ProjectProgress == null || !details.ProjectProgress.Any())
            {
                throw new Exception("登録するデータがありません。");
            }
            await _repository.SaveProjectDetailsAsync(details, endFlg, projectCD);
        }
    }
}
