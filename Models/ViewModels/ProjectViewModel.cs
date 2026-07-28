using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models.ViewModels
{
    public class ProjectViewModel
    {
        [Key]
        public string ProjectCD { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
            
        public string EndFlag { get; set; } = string.Empty;
       
        public int No { get; set; }

    }
   
}