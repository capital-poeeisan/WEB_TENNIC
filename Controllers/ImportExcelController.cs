
using ClosedXML.Excel;
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
            try
            {
                // Validation
                var validationResult = await ValidateUploadAsync(model);

                if (!validationResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        type = validationResult.Type,
                        message = validationResult.Message
                    });
                }

                
                  var stream = new MemoryStream();
                  await model.fileName.CopyToAsync(stream);
               

                // Read Excel
                ExcelPackage.License.SetNonCommercialPersonal("CKM");

                using var package = new ExcelPackage(stream);
                int wcount = package.Workbook.Worksheets.Count();
                var worksheet = package.Workbook.Worksheets[0];


                // Header Check
                string header1 =
                    worksheet.Cells[1, 1].Text.Trim();

                string header2 =
                    worksheet.Cells[1, 2].Text.Trim();

                if (header1 != "得意先CD" ||
                    header2 != "目標等")
                {                   

                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message =
                            "無効なExcel形式です。必要な列：得意先CD、目標等 です。"
                    });
                }


                // Generate ProjectCD
                string projectCD = GenerateProjectCD();

                model.ProjectCd = projectCD;


                DataTable dt = new DataTable();

                dt.Columns.Add("ProjectCD", typeof(string));
                dt.Columns.Add("CustomerCD", typeof(string));
                dt.Columns.Add("ProjectName", typeof(string));
                dt.Columns.Add("OrderAmt", typeof(int));
                dt.Columns.Add("FileName", typeof(string));


                List<string> notFoundCustomerCd = new();
                //List<string> order_amt_errorList = new();

                //Check CustomerCD
                var customerCds = new List<string>();                

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string customerCd = worksheet.Cells[row, 1].Text.Trim();

                    if (!string.IsNullOrWhiteSpace(customerCd))
                    {
                        customerCds.Add(customerCd);
                    }
                }

                var distinctCustomerCds = customerCds.Distinct().ToList();//Remove Duplicate Data

                //var customers = await _context.WT_M_Customer
                //                .Where(x => distinctCustomerCds.Contains(x.CustomerCd))
                //                .Select(x => x.CustomerCd)
                //                .ToListAsync();

                var customers = await _context.WT_M_Customer
                                .FromSqlRaw(@"
                                    SELECT *
                                    FROM dbo.WT_F_Customer(GETDATE())
                                ")
                                .ToListAsync();

                var customerSet = customers.Select(x => x.CustomerCd).ToHashSet();

                //Loop Excel file
                for (int row = 2;row <= worksheet.Dimension.End.Row;row++)
                {
                    string customerCd = worksheet.Cells[row, 1].Text.Trim();
                    
                    if (string.IsNullOrWhiteSpace(customerCd))
                    {
                        continue;
                    }


                    if (!customerSet.Contains(customerCd))
                    {
                        notFoundCustomerCd.Add(customerCd);
                        continue;
                    }

                    int orderAmt = 0;

                    if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                    {
                        if (!int.TryParse(worksheet.Cells[row, 2].Text,out orderAmt))
                        {
                            orderAmt = 0;
                            //order_amt_errorList.Add(worksheet.Cells[row, 2].Text);
                            //continue;
                        }
                    }
                    else
                    {
                        orderAmt = 0;
                    }
                    

                        dt.Rows.Add(
                            projectCD,
                            customerCd,
                            model.ProjectName,
                            orderAmt,
                            model.fileName.FileName
                        );
                }


                if (dt.Rows.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message =
                            $"'{model.fileName.FileName}'Excelにデータがないよ"
                    });
                }


                await _importExcelService.ImportExcelAsync(dt,model.ProjectCd,model.ProjectName);

                await _importExcelService.WT_Logging_Insert(model);


                List<string> warningMessages = new();


                //if (order_amt_errorList.Any())
                //{
                //    warningMessages.Add(
                //        string.Join(", ",
                //            order_amt_errorList.Distinct())
                //        + " 数字が無効です。"
                //    );
                //}


                if (notFoundCustomerCd.Any())
                {
                    warningMessages.Add(
                        string.Join(", ",
                            notFoundCustomerCd.Distinct())
                        + " はテーブルに登録されていません。"
                    );
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
        public class ValidationResultModel
        {
            public bool Success { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }
        private async Task<ValidationResultModel> ValidateUploadAsync(ImportExcelViewModel model)
        {
            // Project Name Check
            if (string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "warning",
                    Message = "プロジェクト名を入力してください。"
                };
            }

            // Excel File Check
            if (model.fileName == null || model.fileName.Length == 0)
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "warning",
                    Message = "Excelファイルを選択してください。"
                };
            }

            // Project Name Duplicate Check
            bool pj_exists = await _context.WT_M_Project
                .AnyAsync(p => p.ProjectName == model.ProjectName);

            if (pj_exists)
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "error",
                    Message = $"'{model.ProjectName}'プロジェクト名は既に存在します。"
                };
            }

            // Excel File Duplicate Check
            bool excelfile_exists = await _context.WT_M_Project
                .AnyAsync(p =>
                    p.FileName == model.fileName.FileName &&
                    p.DeleteDateTime == null);

            if (excelfile_exists)
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "error",
                    Message = $"'{model.fileName.FileName}'Excelファイル名は既に存在します。"
                };
            }

            return new ValidationResultModel
            {
                Success = true
            };
        }

        [HttpGet]
        public IActionResult DownloadExcel()
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Sheet");


            worksheet.Cell("A1").Value = "得意先CD";
            worksheet.Cell("B1").Value = "目標等";


            var headerRange = worksheet.Range("A1:B1");
            headerRange.Style.Font.Bold = true;


            using var stream = new MemoryStream();

            workbook.SaveAs(stream);
            stream.Position = 0;
            string datetime = DateTime.Now.ToString("yyyyMMdd_HHmm");



            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ExcelImport" + datetime + ".xlsx"
            );
        }


    }
}
