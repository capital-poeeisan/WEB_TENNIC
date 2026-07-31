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
