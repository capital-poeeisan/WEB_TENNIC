using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WEB_TENNIC.Data;
using WEB_TENNIC.Interface.Service.ImportExcel;
using WEB_TENNIC.Models;
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
                model.ProjectCd = projectCd;
            }
            
                            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(ImportExcelViewModel model)
        {
            TempData["ProjectName"] = null;
            TempData["fileName"] = null;
            TempData["ErrorMessage"] = null;
            TempData["SuccessMessage"] = null;
            TempData["WarningMessage"] = null;

            string ProjectCD = "";
            List<string> notFoundCustomerCd = new List<string>();
            List<string> order_amt_errorList = new List<string>();
            List<string> warningMessages = new();

            if (!ModelState.IsValid)
            {

                if (model.ProjectName == null)
                {
                    TempData["ProjectName"] = "no_pj";
                }
                else if (model.fileName == null)
                {
                    TempData["fileName"] = "no_file";
                }

                return View("index", model);
            }


            ExcelPackage.License.SetNonCommercialPersonal("CKM");

            if (model.fileName != null && model.fileName.Length > 0)
            {
                using var stream = new MemoryStream();
                await model.fileName.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                int wcount = package.Workbook.Worksheets.Count();
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
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
                                .AnyAsync(p => p.ProjectName == model.ProjectName && p.ProjectCd!=model.ProjectCd);
                        if (pj_exists)
                        {
                            TempData["ErrorMessage"] = $"'{model.ProjectName}'プロジェクト名は既に存在します。";
                            return View("Index");
                            
                            

                        }

                        //Check Excel File

                        bool excelfile_exists = await _context.WT_M_Project
                                .AnyAsync(p => p.FileName == model.fileName.FileName && p.ProjectCd != model.ProjectCd);
                        if (excelfile_exists)
                        {

                            TempData["ErrorMessage"] = $"'{model.fileName.FileName}'Excelファイル名は既に存在します。";
                            return View("Index");
                           


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
                            int update_status = 0;
                            for (int row = 2; row <= rowCount; row++)
                            {
                                string customerCD = worksheet.Cells[row, 1].Text.Trim();

                                if (string.IsNullOrWhiteSpace(customerCD))
                                {
                                    continue;
                                }

                                //Check OrderAmout is Character?
                                if (!decimal.TryParse(worksheet.Cells[row, 2].Text, out decimal orderAmt))
                                {
                                    order_amt_errorList.Add(worksheet.Cells[row, 2].Text);
                                    continue;
                                }                                
                                int ord_Amt = int.Parse(worksheet.Cells[row, 2].Text);

                                //Check CustomerCD is Exit in CustomerTable?
                                bool customer = await _context.WT_M_Customer
                                   .AnyAsync(c => c.CustomerCd == customerCD);

                                if (!customer)
                                {
                                    notFoundCustomerCd.Add(customerCD);
                                    continue;

                                }

                                bool pjProect = await _context.WT_M_Project.AnyAsync(x => x.ProjectCd == model.ProjectCd);
                                bool pjCustomer = await _context.WT_M_Project.AnyAsync(x => x.ProjectCd == model.ProjectCd && x.CustomerCd == customerCD);
                               
                                if (pjCustomer)
                                {
                                    // Update
                                    update_status += 1;
                                    model.CustomerCd = customerCD;
                                    model.OrderAmt = ord_Amt;
                                    model.UpdateFlag = 1;
                                    await _importExcelService.Update_ImportExcelAsync(model);
                                   


                                }
                                else if(pjProect==true && pjCustomer==false)
                                {
                                    //insert
                                    update_status += 1;
                                    model.CustomerCd = customerCD;
                                    model.OrderAmt = ord_Amt;
                                    model.UpdateFlag = 0;
                                    await _importExcelService.Update_ImportExcelAsync(model);
                                   
                                }

                                if(update_status==1)
                                {
                                    model.UpdateFlag = 2;
                                    await _importExcelService.Update_ImportExcelAsync(model);
                                }
                            }

                        }

                        
                    }


                  

                   
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return View("Index");

                }
                //ErrorMessage For Warning 
                if (order_amt_errorList.Any())
                {
                    warningMessages.Add(string.Join(", ", order_amt_errorList.Distinct()) + "数字が無効です。");

                }
                if (notFoundCustomerCd.Any())
                {
                    warningMessages.Add(string.Join(", ", notFoundCustomerCd.Distinct()) + "はテーブルに登録されていません。");

                }
                if (warningMessages.Any())
                {
                    TempData["WarningMessage"] = string.Join("<br><br>", warningMessages);

                }
                //Success Message
                else
                {
                    TempData["SuccessMessage"] = $"修正　終わりました。";
                   
                }


            }

            return RedirectToAction("Index");



        }

     











    }
}
