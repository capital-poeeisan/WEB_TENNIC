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
               
                    string message = "";

                    if (model.ProjectName == null)
                    {
                        message = "プログラム名を入力してください。";
                        return Json(new
                        {
                            success = false,
                            type = "warning",
                            message = message
                        });
                    }
                    else if (model.fileName == null && model.F_name ==null)
                    {
                        message = "Excelファイルをインポートしてください。";
                        return Json(new
                        {
                            success = false,
                            type = "warning",
                            message = message
                        });
                    }

                   
               
                //change project name only
                if (model.F_name!=null && model.fileName==null)
                {
                    var project = await _context.WT_M_Project
                                   .FirstOrDefaultAsync(x =>
                                    x.ProjectCd == model.ProjectCd );

                    if (project != null)
                    {
                        //Check ProjectName 
                        bool pj_exists = await _context.WT_M_Project
                            .AnyAsync(p =>
                                p.ProjectName == model.ProjectName &&
                                p.ProjectCd != model.ProjectCd);


                        if (pj_exists)
                        {
                            return Json(new
                            {
                                success = false,
                                type = "error",
                                message = $"'{model.ProjectName}'プロジェクト名は既に存在します。"
                            });
                        }
                        else
                        {
                            project.ProjectName = model.ProjectName;
                            

                            await _context.SaveChangesAsync();
                            await _importExcelService.WT_Logging_Update(model);
                        }
                       
                    }
                }
                //change also file data
                else if(model.fileName != null && model.fileName.FileName == model.F_name)
                {
                    ExcelPackage.License.SetNonCommercialPersonal("CKM");


                    if (model.fileName == null || model.fileName.Length == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            type = "warning",
                            message = "Excelファイルを選択してください。"
                        });
                    }


                    using var stream = new MemoryStream();
                    await model.fileName.CopyToAsync(stream);

                    using var package = new ExcelPackage(stream);

                    var worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null)
                    {
                        return Json(new
                        {
                            success = false,
                            type = "error",
                            message = "Excelファイルにデータがありません。"
                        });
                    }


                    int rowCount = worksheet.Dimension.Rows;


                    //Check Header 
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



                   



                    //// Excel File Check
                    //bool excelfile_exists = await _context.WT_M_Project
                    //    .AnyAsync(p =>
                    //        p.FileName == model.fileName.FileName &&
                    //        p.ProjectCd != model.ProjectCd);


                    //if (excelfile_exists)
                    //{
                    //    return Json(new
                    //    {
                    //        success = false,
                    //        type = "error",
                    //        message = $"'{model.fileName.FileName}'Excelファイル名は既に存在します。"
                    //    });
                    //}



                    DataTable dt = new DataTable();

                    dt.Columns.Add("ProjectCD", typeof(string));
                    dt.Columns.Add("CustomerCD", typeof(string));
                    dt.Columns.Add("ProjectName", typeof(string));
                    dt.Columns.Add("OrderAmt", typeof(int));
                    dt.Columns.Add("FileName", typeof(string));


                    for (int row = 2; row <= rowCount; row++)
                    {
                        string customerCD = worksheet.Cells[row, 1].Text.Trim();


                        if (string.IsNullOrWhiteSpace(customerCD))
                        {
                            continue;
                        }


                        int orderAmt = 0;


                        if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                        {
                            if (!int.TryParse(worksheet.Cells[row, 2].Text, out orderAmt))
                            {
                                order_amt_errorList.Add(
                                    worksheet.Cells[row, 2].Text
                                );

                                continue;
                            }
                        }



                        bool customer = await _context.WT_M_Customer
                            .AnyAsync(c => c.CustomerCd == customerCD);



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


                    //Check Data Exit?
                    if (dt.Rows.Count > 0)
                    {
                        await _importExcelService.ImportExcelAsync(dt);

                        await _importExcelService.WT_Logging_Update(model);
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            type = "error",
                            message = "Excelファイルにデータがありません。"
                        });
                    }


            
                    if (order_amt_errorList.Any())
                    {
                        warningMessages.Add(
                            string.Join(", ", order_amt_errorList.Distinct())
                            + "数字が無効です。"
                        );
                    }


                    if (notFoundCustomerCd.Any())
                    {
                        warningMessages.Add(
                            string.Join(", ", notFoundCustomerCd.Distinct())
                            + "はテーブルに登録されていません。"
                        );
                    }



                   


                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        type = "error",
                        message = $"you need to choose the same file name '{model.F_name}' for your data update."
                    });

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

    }
}
