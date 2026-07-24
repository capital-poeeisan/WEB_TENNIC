using Microsoft.EntityFrameworkCore;
using WEB_TENNIC.Data;
using WEB_TENNIC.Interface.Repositories;
using WEB_TENNIC.Interface.Repositories.ImportExcel;
using WEB_TENNIC.Interface.Service.ImportExcel;
using WEB_TENNIC.Interface.Services;
using WEB_TENNIC.Repositories.Project;
using WEB_TENNIC.Service.Project;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.UseStaticFiles
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IImportExcelService, ImportExcelService>();
builder.Services.AddScoped<IImportExcelRepository, ImportExcelRepository>();
builder.Services.AddScoped<IProjectDetailRepository, ProjectDetailRepository>();
builder.Services.AddScoped<IProjectDetailService, ProjectDetailService>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ProjectDetail}/{action=Index}/{id?}");

app.Run();
