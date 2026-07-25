namespace WEB_TENNIC.Models.ViewModels
{
    public class ProjectInputViewModel
    {
        public string? ShopName { get; set; }
        public string? CustomerCD { get; set; }
        public string? StaffCD { get; set; }

        public bool Active { get; set; }

        public string? Remark { get; set; }


        public int? Amount { get; set; }
    }
}
