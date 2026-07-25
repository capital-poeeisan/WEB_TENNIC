using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models.ViewModels
{
    public class ProjectViewModel
    {
        public string ProjectName { get; set; } = string.Empty;
            
        public string EndFlag { get; set; } = string.Empty;
       
        public int No { get; set; }

    }
   
}