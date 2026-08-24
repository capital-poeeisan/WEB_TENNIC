using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
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
        public  string org_fileName;
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
                var PP = _context.WT_M_Project
                            .Where(x => x.ProjectCd == projectCd)
                            .Select(u => new { u.ProjectName, u.FileName }).FirstOrDefault();



                model.ProjectName = PP.ProjectName;
                model.ProjectCd = projectCd;
                model.F_name = PP.FileName;
               
            }
            else
            {               
                 return RedirectToAction("Index", "ProjectList");
               
            }
            
                           
            return View(model);
        }       
        [HttpPost]
        public async Task<IActionResult> UploadExcel(ImportExcelViewModel model)
        {
            List<string> notFoundCustomerCd = new();
            List<string> order_amt_errorList = new();
            List<string> warningMessages = new();

            try
            {
                // Validation
                var validationResult =
                    await ValidateUpdateAsync(model);

                if (!validationResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        type = validationResult.Type,
                        message = validationResult.Message
                    });
                }

                if (model.F_name != null && model.fileName == null)
                {
                    var project =await _context.WT_M_Project
                                .FirstOrDefaultAsync(x =>
                                x.ProjectCd == model.ProjectCd);

                    if (project != null)
                    {
                        project.ProjectName = model.ProjectName;
                        project.UpdateDateTime = DateTime.Now;

                        await _context.SaveChangesAsync();
                        await _importExcelService.WT_Logging_Update(model);
                    }
                }

                else if (model.fileName != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }


                    string oldFileName = model.F_name;
                    string newFileName = model.fileName.FileName;

                    string oldFilePath = Path.Combine(uploadsFolder, oldFileName);

                    string newFilePath = Path.Combine(uploadsFolder, newFileName);


                    // Delete old file
                    if (!string.Equals(oldFileName,newFileName,StringComparison.OrdinalIgnoreCase))
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    // Save new file
                    using (var stream = new FileStream(newFilePath,FileMode.Create))
                    {
                        await model.fileName.CopyToAsync(stream);
                    }

                    // Read Excel
                    ExcelPackage.License.SetNonCommercialPersonal("CKM");

                    using var package = new ExcelPackage(new FileInfo(newFilePath));

                    var worksheet = package.Workbook.Worksheets[0];


                    // Excel Header Check
                    if (worksheet.Dimension == null)
                    {
                        return Json(new
                        {
                            success = false,
                            type = "error",
                            message =
                                "Excelファイルにデータがありません。"
                        });
                    }


                    string header1 = worksheet.Cells[1, 1].Text.Trim();

                    string header2 = worksheet.Cells[1, 2].Text.Trim();


                    if (header1 != "CustomerCD" || header2 != "OrderAmt")
                    {
                        return Json(new
                        {
                            success = false,
                            type = "error",
                            message =
                                "無効なExcel形式です。必要な列：CustomerCD、OrderAmt です。"
                        });
                    }


                    int rowCount = worksheet.Dimension.Rows;


                    DataTable dt = new DataTable();

                    dt.Columns.Add("ProjectCD",typeof(string));
                    dt.Columns.Add("CustomerCD",typeof(string));
                    dt.Columns.Add("ProjectName",typeof(string));
                    dt.Columns.Add("OrderAmt",typeof(int));
                    dt.Columns.Add("FileName",typeof(string));

                    // Excel Data Check
                    for (int row = 2;row <= rowCount;row++)
                    {
                        string customerCD =worksheet.Cells[row, 1].Text.Trim();

                        if (string.IsNullOrWhiteSpace(customerCD))
                        {
                            continue;
                        }


                        int orderAmt = 0;

                        string orderAmtText =worksheet.Cells[row, 2].Text.Trim();


                        if (!string.IsNullOrWhiteSpace(orderAmtText))
                        {
                            if (!int.TryParse(orderAmtText,out orderAmt))
                            {
                                order_amt_errorList.Add(orderAmtText);
                                continue;
                            }
                        }


                        bool customer = await _context.WT_M_Customer
                                        .AnyAsync(c =>
                                        c.CustomerCd == customerCD);


                        if (!customer)
                        {
                            notFoundCustomerCd.Add(customerCD);

                            continue;
                        }


                        dt.Rows.Add(
                            model.ProjectCd,
                            customerCD,
                            model.ProjectName,
                            orderAmt,
                            model.fileName.FileName
                        );
                    }


                    // No Valid Data
                    if (dt.Rows.Count == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            type = "error",
                            message =
                                "Excelファイルにデータがありません。"
                        });
                    }


                    // Import
                    await _importExcelService.ImportExcelAsync(dt);


                    // Update Project FileName
                    var update_Project =await _context.WT_M_Project.Where(x =>
                                x.ProjectCd ==
                                model.ProjectCd).ToListAsync();


                    foreach (var project in update_Project)
                    {
                        project.FileName = model.fileName.FileName;

                        project.UpdateDateTime = DateTime.Now;
                    }


                    await _context.SaveChangesAsync();


                    await _importExcelService.WT_Logging_Update(model);


                    // Warning
                    if (order_amt_errorList.Any())
                    {
                        warningMessages.Add(string.Join(", ",order_amt_errorList.Distinct())+ "数字が無効です。");
                    }


                    if (notFoundCustomerCd.Any())
                    {
                        warningMessages.Add(string.Join(", ",notFoundCustomerCd.Distinct())+ "はテーブルに登録されていません。");
                    }
                }


                return Json(new
                {
                    success = true,
                    message = "修正　終わりました。",
                    warnings = warningMessages
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    type = "error",
                    message = ex.Message
                });
            }
        }        
        private async Task<ValidationResultModel> ValidateUpdateAsync(ImportExcelViewModel model)
        {
            // Project Name Check
            if (string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "warning",
                    Message = "プログラム名を入力してください。"
                };
            }


            // File Check
            if (model.fileName == null && model.F_name == null)
            {
                return new ValidationResultModel
                {
                    Success = false,
                    Type = "warning",
                    Message = "Excelファイルをインポートしてください。"
                };
            }


            // Project Name Change
            if (model.F_name != null && model.fileName == null)
            {
                bool pj_exists = await _context.WT_M_Project
                    .AnyAsync(p =>
                        p.ProjectName == model.ProjectName &&
                        p.ProjectCd != model.ProjectCd);

                if (pj_exists)
                {
                    return new ValidationResultModel
                    {
                        Success = false,
                        Type = "error",
                        Message =
                            $"'{model.ProjectName}'プロジェクト名は既に存在します。"
                    };
                }
            }


            // Excel File Update
            if (model.fileName != null)
            {
                if (model.fileName.Length == 0)
                {
                    return new ValidationResultModel
                    {
                        Success = false,
                        Type = "warning",
                        Message = "Excelファイルを選択してください。"
                    };
                }


                // Excel File Duplicate Check
                bool excelfile_exists =
                    await _context.WT_M_Project
                        .AnyAsync(p =>
                            p.FileName == model.fileName.FileName &&
                            p.ProjectCd != model.ProjectCd &&
                            p.DeleteDateTime == null);

                if (excelfile_exists)
                {
                    return new ValidationResultModel
                    {
                        Success = false,
                        Type = "error",
                        Message =
                            $"'{model.fileName.FileName}'Excelファイル名は既に存在します。"
                    };
                }
            }


            return new ValidationResultModel
            {
                Success = true
            };
        }
        public class ValidationResultModel
        {
            public bool Success { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }


        //[HttpPost]
        //public async Task<IActionResult> UploadExcel(ImportExcelViewModel model)
        //{
        //    List<string> notFoundCustomerCd = new();
        //    List<string> order_amt_errorList = new();
        //    List<string> warningMessages = new();


        //    try
        //    {

        //            string message = "";

        //            if (model.ProjectName == null)
        //            {
        //                message = "プログラム名を入力してください。";
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "warning",
        //                    message = message
        //                });
        //            }
        //            else if (model.fileName == null && model.F_name ==null)
        //            {
        //                message = "Excelファイルをインポートしてください。";
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "warning",
        //                    message = message
        //                });
        //            }



        //        //change project name only
        //        if (model.F_name!=null && model.fileName==null)
        //        {
        //            var project = await _context.WT_M_Project
        //                           .FirstOrDefaultAsync(x =>
        //                            x.ProjectCd == model.ProjectCd );

        //            if (project != null)
        //            {
        //                //Check ProjectName 
        //                bool pj_exists = await _context.WT_M_Project
        //                    .AnyAsync(p =>
        //                        p.ProjectName == model.ProjectName &&
        //                        p.ProjectCd != model.ProjectCd);


        //                if (pj_exists)
        //                {
        //                    return Json(new
        //                    {
        //                        success = false,
        //                        type = "error",
        //                        message = $"'{model.ProjectName}'プロジェクト名は既に存在します。"
        //                    });
        //                }
        //                else
        //                {
        //                    project.ProjectName = model.ProjectName;
        //                    project.UpdateDateTime= DateTime.Now;


        //                    await _context.SaveChangesAsync();
        //                    await _importExcelService.WT_Logging_Update(model);
        //                }

        //            }
        //        }
        //        //update file data
        //        else if(model.fileName != null)
        //        {

        //            if (model.fileName == null || model.fileName.Length == 0)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "warning",
        //                    message = "Excelファイルを選択してください。"
        //                });
        //            }

        //            var uploadsFolder = Path.Combine(
        //            Directory.GetCurrentDirectory(),
        //            "wwwroot",
        //            "uploads"
        //                        );
        //            if (!Directory.Exists(uploadsFolder))
        //            {
        //                Directory.CreateDirectory(uploadsFolder);
        //            }

        //            string oldFileName = model.F_name;

        //            string newFileName = model.fileName.FileName;

        //            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);

        //            var newFilePath = Path.Combine(uploadsFolder, newFileName);

        //            if (!string.Equals(oldFileName, newFileName,
        //                StringComparison.OrdinalIgnoreCase))
        //            {
        //                if (System.IO.File.Exists(oldFilePath))
        //                {
        //                    System.IO.File.Delete(oldFilePath);
        //                }
        //            }

        //            using (var stream = new FileStream(
        //                newFilePath,
        //                FileMode.Create
        //            ))
        //            {
        //                await model.fileName.CopyToAsync(stream);
        //            }

        //            // Read Excel file from server path
        //            ExcelPackage.License.SetNonCommercialPersonal("CKM");
        //            var package = new ExcelPackage(new FileInfo(newFilePath));
        //            var worksheet = package.Workbook.Worksheets[0];


        //            if (worksheet.Dimension == null)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "error",
        //                    message = "Excelファイルにデータがありません。"
        //                });
        //            }


        //            int rowCount = worksheet.Dimension.Rows;


        //            //Check Header 
        //            string header1 = worksheet.Cells[1, 1].Text.Trim();
        //            string header2 = worksheet.Cells[1, 2].Text.Trim();


        //            if (header1 != "CustomerCD" || header2 != "OrderAmt")
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "error",
        //                    message = "無効なExcel形式です。必要な列：CustomerCD、OrderAmt です。"
        //                });
        //            }

        //            // Excel File Check
        //            bool excelfile_exists = await _context.WT_M_Project
        //                .Where(p =>
        //                    p.FileName == model.fileName.FileName &&
        //                    p.ProjectCd != model.ProjectCd)
        //                .Where(x=>x.DeleteDateTime==null).AnyAsync();


        //            if (excelfile_exists)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "error",
        //                    message = $"'{model.fileName.FileName}'Excelファイル名は既に存在します。"
        //                });
        //            }


        //            DataTable dt = new DataTable();

        //            dt.Columns.Add("ProjectCD", typeof(string));
        //            dt.Columns.Add("CustomerCD", typeof(string));
        //            dt.Columns.Add("ProjectName", typeof(string));
        //            dt.Columns.Add("OrderAmt", typeof(int));
        //            dt.Columns.Add("FileName", typeof(string));


        //            for (int row = 2; row <= rowCount; row++)
        //            {
        //                string customerCD = worksheet.Cells[row, 1].Text.Trim();


        //                if (string.IsNullOrWhiteSpace(customerCD))
        //                {
        //                    continue;
        //                }


        //                int orderAmt = 0;


        //                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
        //                {
        //                    if (!int.TryParse(worksheet.Cells[row, 2].Text, out orderAmt))
        //                    {
        //                        order_amt_errorList.Add(
        //                            worksheet.Cells[row, 2].Text
        //                        );

        //                        continue;
        //                    }
        //                }



        //                bool customer = await _context.WT_M_Customer
        //                    .AnyAsync(c => c.CustomerCd == customerCD);



        //                if (!customer)
        //                {
        //                    notFoundCustomerCd.Add(customerCD);
        //                    continue;
        //                }



        //                dt.Rows.Add(
        //                    model.ProjectCd,
        //                    customerCD,
        //                    model.ProjectName,
        //                    orderAmt,
        //                    model.fileName.FileName
        //                );

        //            }


        //            //Check Data Exit?
        //            if (dt.Rows.Count > 0)
        //            {
        //                await _importExcelService.ImportExcelAsync(dt);
        //                var update_Project = await _context.WT_M_Project
        //                    .Where(x => x.ProjectCd == model.ProjectCd)
        //                    .ToListAsync();
        //                if (update_Project != null) {
        //                    foreach (var project in update_Project)
        //                    {
        //                        project.FileName = model.fileName.FileName;
        //                        project.UpdateDateTime = DateTime.Now;
        //                    }
        //                    await _context.SaveChangesAsync();
        //                }                       


        //                await _importExcelService.WT_Logging_Update(model);
        //            }
        //            else
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    type = "error",
        //                    message = "Excelファイルにデータがありません。"
        //                });
        //            }



        //            if (order_amt_errorList.Any())
        //            {
        //                warningMessages.Add(
        //                    string.Join(", ", order_amt_errorList.Distinct())
        //                    + "数字が無効です。"
        //                );
        //            }


        //            if (notFoundCustomerCd.Any())
        //            {
        //                warningMessages.Add(
        //                    string.Join(", ", notFoundCustomerCd.Distinct())
        //                    + "はテーブルに登録されていません。"
        //                );
        //            }






        //        }


        //        return Json(new
        //        {
        //            success = true,
        //            message = "修正　終わりました。",
        //            warnings = warningMessages
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            type = "error",
        //            message = ex.Message
        //        });
        //    }
        //}
    }
}
