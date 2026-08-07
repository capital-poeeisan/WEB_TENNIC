using System.Data;
using System.Threading.Tasks;
using WEB_TENNIC.Interface.Repositories.ImportExcel;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;
using WEB_TENNIC.Repositories.Project;
namespace WEB_TENNIC.Interface.Service.ImportExcel
{
    public class ImportExcelService:IImportExcelService
    {
        private readonly IImportExcelRepository _repository;

        public ImportExcelService(IImportExcelRepository repository)
        {
            _repository = repository;
        }
        public async Task<int> ImportExcelAsync(DataTable dt)
        {
            return await _repository.ImportExcelAsync(dt);
        }

        public async Task<int> WT_Logging_Insert(ImportExcelViewModel m)
        {
            return await _repository.WT_Logging_Insert(m);
        }

        public async Task<int> WT_Logging_Update(ImportExcelViewModel m)
        {
            return await _repository.WT_Logging_Update(m);
        }



    }
}
