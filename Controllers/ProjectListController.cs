using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WEB_TENNIC.Data;
using WEB_TENNIC.Models.ViewModels;
using WEB_TENNIC.Service.Project;

namespace WEB_TENNIC.Controllers
{
    public class ProjectListController : Controller
    {
        
        private readonly IProjectService _service;

        public ProjectListController(IProjectService service)
        {
            _service = service;
        }
        
        public async Task<IActionResult> Index(int EndFlag)
        {
            ViewBag.EndFlag = EndFlag;
            var  data = await _service.GetProjectList(EndFlag);
            

            return View(data);
            
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select a project."
                });
            }
            if (id == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "There is no data to save."
                });
            }
            try
            {


                await _service.DeleteProjectName(id);

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
