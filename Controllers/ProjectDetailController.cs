using Microsoft.AspNetCore.Mvc;
using WEB_TENNIC.Interface.Services;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Controllers
{
    public class ProjectDetailController : Controller
    {
        private readonly IProjectDetailService _service;

        public ProjectDetailController(IProjectDetailService service)
        {
            _service = service;
        }
        public IActionResult Index(string projectCD)
        {
            var model = _service.GetProjectList();
            model.ProjectCd = projectCD;
            return View(model);
        }
        
        public IActionResult GetStaff(string projectCD)
        {

            if (string.IsNullOrEmpty(projectCD))
            {
                return BadRequest();
            }

            var model = _service.GetStaffList(projectCD);

            return Json(model.StaffList);
        }

        [HttpPost]
        public IActionResult GetData(ProjectDetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select a project."
                });
            }
            var data = _service.GetData(model.ProjectCd, model.StaffCD);
            return PartialView("_ProjectDetailTable", data);
        }


       

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ProjectDetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select a project."
                });
            }

            if (model.ProjectProgress == null || !model.ProjectProgress.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "There is no data to save."
                });
            }
            try
            {
                await _service.SaveProjectDetailsAsync(model, model.EndFlag, model.ProjectCd);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }

        }
    }
}
