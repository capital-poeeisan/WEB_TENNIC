using WEB_TENNIC.Interface.Repositories.ImportExcel;
using WEB_TENNIC.Models.ViewModels;


namespace WEB_TENNIC.Interface.Service.ImportExcel
{
    public interface IImportExcelService
    {
       Task<int> ImportExcelAsync(ImportExcelViewModel m);
    }
}
