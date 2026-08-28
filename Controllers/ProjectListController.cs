using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
            try
            {
                ViewBag.EndFlag = EndFlag;
                var data = await _service.GetProjectList(EndFlag);


                return View(data);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex); 
                return View();

            }
           
            
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
                var projectData = await _context.WT_M_Project
                                    .Where(x => x.ProjectCd == id)
                                    .Select(x => new
                                    {
                                        x.CustomerCd,
                                        x.OrderAmt,
                                        x.FileName
                                    })
                                    .ToListAsync();

                if (!projectData.Any())
                {
                    return NotFound("データが見つかりません。");
                }
                
                string fileName = projectData.First().FileName;

                // Excel Create
                ExcelPackage.License.SetNonCommercialPersonal("CKM");

                using var package = new ExcelPackage();

                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Header
                worksheet.Cells[1, 1].Value = "CustomerCD";
                worksheet.Cells[1, 2].Value = "OrderAmt";

                // Data
                int row = 2;
                foreach (var item in projectData)
                {
                    worksheet.Cells[row, 1].Value = item.CustomerCd;
                    worksheet.Cells[row, 2].Value = item.OrderAmt;

                    row++;
                }

                // read file to byte array 
                var fileBytes = package.GetAsByteArray();

                // return  Download  file
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
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
