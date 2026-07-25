using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Repositories.ImportExcel
{
    public interface IImportExcelRepository
    {
         Task<int> ImportExcelAsync(ImportExcelViewModel m);
    }
}
