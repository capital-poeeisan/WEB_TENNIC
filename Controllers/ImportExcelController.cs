
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using System.Text;
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
            var response = new
            {
                success = false,
                message = "",
                warnings = new List<string>()
            };

            string ProjectCD = "";
            List<string> notFoundCustomerCd = new List<string>();
            List<string> order_amt_errorList = new List<string>();
            List<string> warningMessages = new List<string>();

            if (!ModelState.IsValid)
            {
                if (model.ProjectName == null)
                {
                    return Json(new
                    {
                        success = false,
                        type = "warning",
                        message = "プロジェクト名を入力してください。"
                    });
                }

                if (model.fileName == null || model.fileName.Length == 0)
                {
                    return Json(new
                    {
                        success = false,
                        type = "warning",
                        message = "Excelファイルを選択してください。"
                    });
                }
            }


            try
            {

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Save file to server
                var filePath = Path.Combine(uploadsFolder, Path.GetFileName(model.fileName.FileName));
                using (var streamm = new FileStream(filePath, FileMode.Create))
                {
                    await model.fileName.CopyToAsync(streamm);
                }

                // Read Excel file from server path
                ExcelPackage.License.SetNonCommercialPersonal("CKM");
                var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0];



                //Header Check
                string header1 = worksheet.Cells[1, 1].Text.Trim();
                string header2 = worksheet.Cells[1, 2].Text.Trim();

                if (header1 != "CustomerCD" || header2 != "OrderAmt")
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message = "無効なExcel形式です。必要な列：CustomerCD、OrderAmt です。"
                    });
                }

               

                // Project Name Check
                bool pj_exists = await _context.WT_M_Project
                    .AnyAsync(p => p.ProjectName == model.ProjectName);

                if (pj_exists)
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message = $"'{model.ProjectName}'プロジェクト名は既に存在します。"
                    });
                }


                // Excel File Check
                bool excelfile_exists = await _context.WT_M_Project
                    .AnyAsync(p => p.FileName == model.fileName.FileName);


                if (excelfile_exists)
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message = $"'{model.fileName.FileName}'Excelファイル名は既に存在します。"
                    });
                }


                ProjectCD = GenerateProjectCD();
                model.ProjectCd = ProjectCD;


                DataTable dt = new DataTable();

                dt.Columns.Add("ProjectCD", typeof(string));
                dt.Columns.Add("CustomerCD", typeof(string));
                dt.Columns.Add("ProjectName", typeof(string));
                dt.Columns.Add("OrderAmt", typeof(int));
                dt.Columns.Add("FileName", typeof(string));
                                
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        string customerCd = worksheet.Cells[row, 1].Text.Trim();


                    if (string.IsNullOrWhiteSpace(customerCd))
                        {
                            continue;
                        }


                        bool exists = await _context.WT_M_Customer
                            .AnyAsync(c => c.CustomerCd == customerCd);


                        if (!exists)
                        {
                            notFoundCustomerCd.Add(customerCd);
                            continue;
                        }


                        int orderAmt = 0;


                        if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                        {
                            if (!int.TryParse(
                               worksheet.Cells[row, 2].Text,
                                out orderAmt))
                            {
                                order_amt_errorList.Add(
                                    worksheet.Cells[row, 2].Text);

                                continue;
                            }
                        }


                        dt.Rows.Add(
                            ProjectCD,
                            customerCd,
                            model.ProjectName,
                            orderAmt,
                            model.fileName.FileName
                        );
                    }

                if (dt.Rows.Count > 0)
                {

                    await _importExcelService.ImportExcelAsync(dt);
                    await _importExcelService.WT_Logging_Insert(model);

                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message = $"'{model.fileName.FileName}'Excelにデータがないよ"
                    });

                }

                // Warning
                if (order_amt_errorList.Any())
                {
                    warningMessages.Add(
                        string.Join(", ",
                        order_amt_errorList.Distinct())
                        + " 数字が無効です。");
                }


                if (notFoundCustomerCd.Any())
                {
                    warningMessages.Add(
                        string.Join(", ",
                        notFoundCustomerCd.Distinct())
                        + " はテーブルに登録されていません。");
                }



                if (warningMessages.Any())
                {
                    return Json(new
                    {
                        success = true,
                        type = "warning",
                        message = "インポート完了しました。",
                        warnings = warningMessages
                    });
                }


                return Json(new
                {
                    success = true,
                    type = "success",
                    message = "インポートが完了しました。"
                });


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
