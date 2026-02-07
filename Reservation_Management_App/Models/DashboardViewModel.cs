namespace Reservation_Management_App.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalLocations { get; set; }
        public int TotalPerformers { get; set; }
        public int PendingReservations { get; set; }
        public int ApprovedReservations { get; set; }
        public int TotalReservations { get; set; }
        public decimal ApprovedRevenue { get; set; }

    }
}