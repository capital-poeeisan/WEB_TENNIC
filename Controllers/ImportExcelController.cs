using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WEB_TENNIC.Data;
using WEB_TENNIC.Interface.Service.ImportExcel;
using WEB_TENNIC.Models.ViewModels;
using WEB_TENNIC.Service.Project;


namespace WEB_TENNIC.Controllers
{
    public class ImportExcelController : Controller
    {
        private readonly IImportExcelService _importExcelService;
        private readonly AppDbContext _context;
        public ImportExcelController(IImportExcelService importExcelService, AppDbContext context)
        {
            _importExcelService = importExcelService;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(ImportExcelViewModel model)
        {
            TempData["ProjectName"] = null;
            TempData["fileName"] = null;

            if (!ModelState.IsValid) 
            {
               
                if (model.ProjectName == null) {
                    TempData["ProjectName"] = "no_pj";
                }
                else if(model.fileName==null)
                {
                    TempData["fileName"] = "no_file";
                }
                
                    return View("index",model); 
            }
        
            TempData["ErrorMessage"] = null;
            TempData["SuccessMessage"] = null;
            string ProjectCD = "";

            ExcelPackage.License.SetNonCommercialPersonal("CKM");

            if (model.fileName != null && model.fileName.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await model.fileName.CopyToAsync(stream);                 

                    using var package = new ExcelPackage(stream);
                    int wcount = package.Workbook.Worksheets.Count();
                    var worksheet = package.Workbook.Worksheets[0];
                    try
                    {                   
                    //Check Header
                    string header1 = worksheet.Cells[1, 1].Text.Trim();
                    string header2 = worksheet.Cells[1, 2].Text.Trim();

                    if (header1 != "CustomerCD" && header2 != "OrderAmt")
                    {
                        TempData["ErrorMessage"] = "無効なExcel形式です。必要な列：CustomerCD、OrderAmt です。";
                        return View("Index");                        
                    }
                    else
                    {
                        //Check ProjectName
                        bool pj_exists = await _context.WT_M_Project
                                .AnyAsync(p => p.ProjectName == model.ProjectName);
                        if (pj_exists)
                        {
                            TempData["ErrorMessage"] = $"'{model.ProjectName}'は既に存在します。";
                            return View("Index");
                            //ModelState.AddModelError("", $"This Project Name :'{model.ProjectName}'is already exist!");
                            //return View("Index", model);
                        }

                        //Check Data in Excel
                        var customerCDValues = worksheet.Cells[2,        // Start Row
                                                               1,        // CustomerCD Column
                                                               worksheet.Dimension.Rows,
                                                               1
                                                               ]
                                                                .Select(x => x.Text.Trim())
                                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                .ToList();


                        if (!customerCDValues.Any())
                        {
                            TempData["ErrorMessage"] = "Excelファイルにデータがありません。";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ProjectCD = GenerateProjectCD();
                            //Check CustoerCd 
                            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                            {
                                string customerCd = worksheet.Cells[row, 1].Text.Trim();


                                if (string.IsNullOrWhiteSpace(customerCd))
                                {
                                    continue;

                                }
                                else
                                {
                                    bool exists = await _context.WT_M_Customer
                                       .AnyAsync(c => c.CustomerCd == customerCd);

                                    if (!exists)
                                    {
                                      
                                        TempData["ErrorMessage"] = $"{row}行：CustomerCD  '{customerCd}' が存在しません。";
                                        return View("Index");

                                    }

                                    bool cusCD_exit = await _context.WT_M_Project
                                        .AnyAsync(c => c.CustomerCd == customerCd
                                                    && c.ProjectCd == ProjectCD);
                                    if(cusCD_exit)
                                    {
                                        //TempData["ErrorMessage"] = $"{row}行：CustomerCD  '{customerCd}' 重複している";
                                        continue;
                                        //return View("Index");
                                    }

                                    int orderAmt = int.Parse(worksheet.Cells[row, 2].Text);

                                    model.CustomerCd = customerCd;
                                    model.OrderAmt = orderAmt;
                                    model.ProjectCd = ProjectCD;
                                  

                                    await _importExcelService.ImportExcelAsync(model);

                                }


                            }

                            TempData["SuccessMessage"] = $"'{model.fileName.FileName}'登録が完了しました。";

                           

                        }

                    }


                }
                    catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return View("Index");

                }


                }         
            
            return RedirectToAction("Index");        


            
        }

        public string GenerateProjectCD()
        {
            var today = DateTime.Today;
            var datePart = today.ToString("MMdd");

            var prefix = $"P{datePart}";

            var lastProject = _context.WT_M_Project
                .Where(x => x.ProjectCd.StartsWith(prefix))
                .OrderByDescending(x => x.ProjectCd)
                .FirstOrDefault();

            int runningNo = 1;

            if (lastProject != null)
            {
                string lastNo = lastProject.ProjectCd.Substring(5, 2);
                runningNo = int.Parse(lastNo) + 1;
            }

            return $"{prefix}{runningNo:D2}";
        }





    }
}
