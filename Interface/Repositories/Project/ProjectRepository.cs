using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WEB_TENNIC.Data;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Repositories.Project
{
   
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;
        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }
       

        public async Task<List<ProjectViewModel>> GetProjectList(int endFlag)
        {            
            var param = new SqlParameter("@EndFlag", endFlag);

            var result = await _context.Set<ProjectViewModel>()
                .FromSqlRaw(
                    "EXEC sp_Project_Select_ByEndFlg @EndFlag",
                    param)
                .AsNoTracking()
                .ToListAsync();

            return result;

        }

        public async Task<int> WT_Logging_Delete(ProjectViewModel m)
        {
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC WT_Logging_Insert " +
                "@ProjectCD={0},@CustomerCD={1}," +
                "@OrderAmt={2},@Note={3},@FileName={4}," +
                "@DateTimeFlg={5},@EndFlag={6}",
                m.ProjectCD,
                null,
                null,
                null,
                m.FileName,
                3,
                null
                );

            return result;
        }

        public async Task<bool> DeleteProjectName(string id)
        {
            var projectCdParam = new SqlParameter(
                 "@ProjectCd",
                 id
             );

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC WT_M_Project_Delete @ProjectCd",
                projectCdParam
            );


            return true;

        }

    

    }
}
