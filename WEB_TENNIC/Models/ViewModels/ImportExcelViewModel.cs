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
        public string   ProjectName { get; set; } = string.Empty;

        public IFormFile fileName { get; set; }
    }
}
                                                                                                                                                                                                                                                                                                        