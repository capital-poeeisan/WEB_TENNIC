using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WEB_TENNIC.Models.ViewModels
{
    public class ImportExcelViewModel
    {
        [Key]
        public string   CustomerCd { get; set; } = string.Empty;
        public string   ProjectCd { get; set; } = string.Empty;
        public int      OrderAmt { get; set; }
        [Required(ErrorMessage = "プロジェクト名")]
        public string   ProjectName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Excelファイル")]
        public IFormFile fileName { get; set; }
        public string? F_name { get; set; } = string.Empty;
        public int UpdateFlag { get; set; }
    }
}
                                                                                                                                                                                                                                                                                                        