using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
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
        public async Task<int> ImportExcelAsync(DataTable dt, string P_cd, string P_name)
        {
            if (dt.Rows.Count > 0)
            {
                dt = dt.AsEnumerable()
              .GroupBy(r => new
              {
                  ProjectCD = r.Field<string>("ProjectCD"),
                  CustomerCD = r.Field<string>("CustomerCD")
              })
              .Select(g => g.First())
              .CopyToDataTable();
            }
           

            var projectsParameter = new SqlParameter("@Projects", dt)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.WT_ProjectType"
            };
            var projectCDParameter = new SqlParameter(
                    "@ProjectCD",
                    P_cd
                );

            var projectNameParameter = new SqlParameter(
                "@ProjectName",
                P_name
            );
            var result = await _context.Database.ExecuteSqlRawAsync(
                    @"EXEC WT_M_Project_Insert_Update 
                    @Projects,
                    @ProjectCD,
                    @ProjectName",

                    projectsParameter,
                    projectCDParameter,
                    projectNameParameter

            );

            return result;
        }

        public async Task<int> WT_Logging_Insert(ImportExcelViewModel m)
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC WT_Logging_Insert " +
                "@ProjectCD={0},@CustomerCD={1}," +
                "@OrderAmt={2},@Note={3},@FileName={4}," +
                "@DateTimeFlg={5},@EndFlag={6}",
                m.ProjectCd,
                null,
                null,
                null,
                m.fileName.FileName,
                1,
                null
                );

            return result;
        }
        public async Task<int> WT_Logging_Update(ImportExcelViewModel m)
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC WT_Logging_Insert " +
                 "@ProjectCD={0},@CustomerCD={1}," +
                "@OrderAmt={2},@Note={3},@FileName={4}," +
                "@DateTimeFlg={5},@EndFlag={6}",
                m.ProjectCd,
                null,
                null,
                null,
                m.F_name,
                2,
                null
                );

            return result;
        }
        

    }
}
