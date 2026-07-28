using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WEB_TENNIC.Data;
using WEB_TENNIC.Interface.Service.ImportExcel;
using WEB_TENNIC.Models.ViewModels;
using WEB_TENNIC.Service.Project;


namespace WEB_TENNIC.Controllers
{
    public class ImportExcelUpdateController : Controller
    {
        private readonly IImportExcelService _importExcelService;
        private readonly AppDbContext _context;
        public ImportExcelUpdateController(IImportExcelService importExcelService, AppDbContext context)
        {
            _importExcelService = importExcelService;
            _context = context;
        }
        public IActionResult Index(string projectCd)
        {
            ImportExcelViewModel model = new ImportExcelViewModel();
            if(projectCd != null)
            {
                var projectName = _context.WT_M_Project
                            .Where(x => x.ProjectCd == projectCd)
                            .Select(x => x.ProjectName).FirstOrDefault();



                model.ProjectName = projectName;
            }
            
                            
            return View(model);
        }



 





    }
}
