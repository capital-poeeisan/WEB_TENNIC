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
        public async Task<int> ImportExcelAsync(ImportExcelViewModel m)
        {
            return await _repository.ImportExcelAsync(m);
        }
    }
}
