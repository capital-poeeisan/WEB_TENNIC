using System.Data;
using WEB_TENNIC.Interface.Repositories.ImportExcel;
using WEB_TENNIC.Models.ViewModels;


namespace WEB_TENNIC.Interface.Service.ImportExcel
{
    public interface IImportExcelService
    {
       Task<int> ImportExcelAsync(DataTable dt);
       //Task<int> Update_ImportExcelAsync(DataTable dt);
       Task<int> WT_Logging_Insert(ImportExcelViewModel m);
       Task<int> WT_Logging_Update(ImportExcelViewModel m);
    }
}
