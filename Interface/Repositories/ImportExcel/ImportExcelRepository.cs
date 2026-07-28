using Microsoft.EntityFrameworkCore;
using WEB_TENNIC.Data;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Repositories.ImportExcel
{
    public class ImportExcelRepository:IImportExcelRepository
    {
        private readonly AppDbContext _context;
        public ImportExcelRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> ImportExcelAsync(ImportExcelViewModel m)
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
            "EXEC WT_M_Project_Insert " +
            "@ProjectCD={0}, @CustomerCD={1},@ProjectName={2}, @OrderAmt={3},@FileName={4}",
            m.ProjectCd,
            m.CustomerCd,
            m.ProjectName,
            m.OrderAmt,
            m.fileName.FileName);
            return result;
        }
    }
}
