using Microsoft.EntityFrameworkCore;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;

namespace WEB_TENNIC.Data
{
    public class AppDbContext:DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) :
           base(options)
        {

        }
       public DbSet<WtMProject> WT_M_Project { get; set; }
        public DbSet<WtMCustomer> WT_M_Customer { get; set; }
        public DbSet<ProjectDetailViewModel> ProjectDetails { get; set; }
        public DbSet<ProjectStaffViewModel> ProjectStaffs { get; set; }
        public DbSet<ProjectInputViewModel> ProjectInputViewModels { get; set; }
        public DbSet<ProjectSummaryViewModel> ProjectSummary { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProjectViewModel>().HasNoKey();
            modelBuilder.Entity<CustomerViewModel>().HasNoKey();
            modelBuilder.Entity<ProjectStaffViewModel>()
               .HasNoKey();

            modelBuilder.Entity<ProjectSummaryViewModel>()
                 .HasNoKey();


            modelBuilder.Entity<ProjectDetailViewModel>()
               .HasNoKey();


            modelBuilder.Entity<ProjectInputViewModel>()
               .HasNoKey();

        }
       

    } 

}
