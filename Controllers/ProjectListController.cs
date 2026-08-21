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
        private readonly AppDbContext _context;
        public ProjectListController(IProjectService service, AppDbContext context)
        {
            _service = service;
            _context = context;
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

                var m = _context.WT_M_Project
                           .Where(x => x.ProjectCd == id)
                           .FirstOrDefault();
                ProjectViewModel model = new ProjectViewModel();
                model.ProjectCD = m.ProjectCd;
                model.FileName = m.FileName;


                await _service.DeleteProjectName(id);
                
                await _service.WT_Logging_Delete(model);

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

        [HttpPost]
        public async Task<IActionResult> DownloadProject(string id)
        {
            try
            {
                var PP = _context.WT_M_Project
                         .Where(x => x.ProjectCd == id)
                         .Select(u => new { u.FileName }).FirstOrDefault();
                string filename = PP.FileName;

                // wwwroot/uploads/FileName.xlsx
                var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );

                    var filePath = Path.Combine(
                        uploadsFolder,
                       filename
                    );

                    // File not found
                    if (!System.IO.File.Exists(filePath))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "ファイルがないよ。"
                        });
                    }

                    // read file to byte array 
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                    // return  Download  file
                    return File(
                        fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        filename
                    );

                }

                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            
            }

           
        


    }
}
