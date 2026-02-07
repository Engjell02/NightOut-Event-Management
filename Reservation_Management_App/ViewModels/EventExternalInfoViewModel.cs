namespace Reservation_Management_App.Web.ViewModels
{
    public class EventExternalInfoViewModel
    {
        public bool HasExternalData { get; set; }

        public decimal TotalNightCost { get; set; }
        public decimal DjCost { get; set; }
        public decimal MainActCost { get; set; }
        public decimal VenueFee { get; set; }

        public string Timeline { get; set; } = string.Empty;   // transformed
        public string Source { get; set; } = "GitHub JSON API";
    }
}
