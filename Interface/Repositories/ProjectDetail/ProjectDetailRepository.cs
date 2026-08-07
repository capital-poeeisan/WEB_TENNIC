using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WEB_TENNIC.Data;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Interface.Repositories
{
    public class ProjectDetailRepository: IProjectDetailRepository
    {
        private readonly AppDbContext _context;
        public ProjectDetailRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<WtMProject> GetProjects()
        {
            return _context.WT_M_Project
                .ToList();
        } 
        public List<ProjectStaffViewModel> GetStaffList(string projectCd)
        {
            return _context.ProjectStaffs
               .FromSqlRaw(
                   "EXEC Select_StaffName @ProjectCD={0}",
                   projectCd)
               .ToList();
        }

        public List<ProjectInputViewModel> GetProgressDetail(string projectCd, List<string> staffCDs)
        {
            var staff = staffCDs == null || !staffCDs.Any()
        ? null
        : string.Join(",", staffCDs);
            return _context.ProjectInputViewModels
                .FromSqlInterpolated(
                    $"EXEC Select_ProgressDetail {projectCd}, {staff}")
                .ToList();
        }
        // Table 1
        public List<ProjectSummaryViewModel> GetSummary(string projectCd)
        {
            return _context.ProjectSummary
                .FromSqlRaw(
                    "EXEC Select_ProjectSummary @ProjectCD={0}",
                    projectCd)
                .ToList();
        }


        public async Task SaveProjectDetailsAsync(ProjectDetailViewModel details, bool endFlg, string projectCD)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                DataTable table = new DataTable();

            table.Columns.Add("ProjectCD", typeof(string));
            table.Columns.Add("CustomerCD", typeof(string));
            table.Columns.Add("StaffCD", typeof(string));
            table.Columns.Add("Status", typeof(bool));
            table.Columns.Add("Remark", typeof(string));
            table.Columns.Add("Amount", typeof(string));

            foreach (var item in details.ProjectProgress)
            {
                table.Rows.Add(
                    projectCD,
                    item.CustomerCD,
                    item.StaffCD,
                    item.Active,
                    item.Remark ?? string.Empty,
                    item.Amount
                );
            }
              
                var tableParam = new SqlParameter("@ProjectDetails", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.ProjectDetailType"
            };
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC WT_ProjectDetail_Insert {tableParam}, {endFlg}, {projectCD}");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ;
            }
        }
    }
}
