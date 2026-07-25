namespace WEB_TENNIC.Models.ViewModels
{
    public class ProjectSummaryViewModel
    {
        public string? StaffCD { get; set; }

        public string? StaffName { get; set; }

        // Count of all shops
        public int? AllShop { get; set; }

        // Count of completed shops (Status = 1)
        public int? Shop { get; set; }

        // Percentage
        public decimal? Percentage { get; set; }
    }
}
