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
                "@CreateDateTime={5},@UpdateDateTime={6},@DeleteDateTime={7},@EndFlag={8}",
                m.ProjectCD,
                null,
                null,
                null,
                m.FileName,
                null,
                null,
                DateTime.Now.ToString(),
                null
                );

            return result;
        }

        public async Task<bool> DeleteProjectName(string id)
        {
            var project = await _context.WT_M_Project
                .FirstOrDefaultAsync(x => x.ProjectCd == id);

            if (project == null)
                return false;

            project.DeleteDateTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;

        }

    

    }
}
