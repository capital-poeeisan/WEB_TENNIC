using System.Data;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Repositories.ImportExcel
{
    public interface IImportExcelRepository
    {
        Task<int> ImportExcelAsync(DataTable dt, string P_cd, string P_name);
        Task<int> WT_Logging_Insert(ImportExcelViewModel m);
        Task<int> WT_Logging_Update(ImportExcelViewModel m);
        //Task<int> Update_ImportExcelAsync(DataTable dt);
    }
}
