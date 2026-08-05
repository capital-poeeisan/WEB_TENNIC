using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models.ViewModels
{
    public class ProjectDetailViewModel
    {
        [Required(ErrorMessage = "Please select a project.")]
        public string ProjectCd { get; set; }
        public bool IsProjectLocked { get; set; }

        public string? ProjectName { get; set; }
        public List<string> StaffCD { get; set; } = new();
        // Result

        public bool EndFlag { get; set; }
        public List<SelectListItem> ProjectList { get; set; } = new();
        // Dropdown 2
        public List<SelectListItem> StaffList { get; set; } = new();
        public List<ProjectSummaryViewModel> SummaryList { get; set; } = new();
        // Table 2
        public List<ProjectInputViewModel> ProjectProgress { get; set; } = new();

    }
}
