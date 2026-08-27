using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_TENNIC.Data;
using WEB_TENNIC.Interface.Repositories;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Services
{
    public class ProjectDetailService: IProjectDetailService
    {
        private readonly IProjectDetailRepository _repository;
        private readonly AppDbContext _context;

        public ProjectDetailService(IProjectDetailRepository repository, AppDbContext context)
        {
            _context=context;
            _repository = repository;
        }

        public async Task<ProjectDetailViewModel> GetProjectList(string projectCd)
        {
            var model = new ProjectDetailViewModel();

           
            if (!string.IsNullOrEmpty(projectCd))
            {
                var selectedProject = await _context.WT_M_Project
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.DeleteDateTime == null &&
                        p.ProjectCd == projectCd);

                if (selectedProject != null &&
                    selectedProject.EndFlag == 1)
                {
                    model.ProjectCd = selectedProject.ProjectCd;
                    model.EndFlag = true;
                    model.IsProjectLocked = true;

                    model.ProjectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = selectedProject.ProjectCd,
                    Text = selectedProject.ProjectName,
                    Selected = true
                }
            };

                    return model;
                }
            }

            // projectCd == null 
            var projects = await _context.WT_M_Project
                            .AsNoTracking()
                            .Where(p =>
                                p.DeleteDateTime == null &&
                                p.EndFlag == 0)
                            .Select(p => new
                            {
                                p.ProjectCd,
                                p.ProjectName
                            })
                            .Distinct()
                            .OrderByDescending(p => p.ProjectCd)
                            .ToListAsync();

            model.ProjectList = projects
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectCd,
                    Text = p.ProjectName
                })
                .ToList();

            if (model.ProjectList.Any())
            {
                model.ProjectCd = model.ProjectList.First().Value;
            }

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
               .OrderByDescending(s => s.StaffCD)
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
