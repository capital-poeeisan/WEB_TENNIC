using System.ComponentModel.DataAnnotations;

namespace WEB_TENNIC.Models.ViewModels
{
    public class CustomerViewModel
    {
        [Key]
       public string CustomerCD { get; set; }
       public string  CustomerName { get; set; }
    }
}
