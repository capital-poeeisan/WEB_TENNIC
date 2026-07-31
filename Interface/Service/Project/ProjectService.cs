using System.Threading.Tasks;
using WEB_TENNIC.Models;
using WEB_TENNIC.Models.ViewModels;
using WEB_TENNIC.Repositories.Project;
namespace WEB_TENNIC.Service.Project
{
    public class ProjectService :IProjectService
    {
        private readonly IProjectRepository _repository;

        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }
       
        public async Task<List<ProjectViewModel>> GetProjectList(int EndFlg)
        {
            return await _repository.GetProjectList(EndFlg);
        }

        public async Task DeleteProjectName(string id)
        {
            if (id == null)
            {
                throw new Exception("削除するデータがありません。");
            }
           await _repository.DeleteProjectName(id);
        }
    }
}
