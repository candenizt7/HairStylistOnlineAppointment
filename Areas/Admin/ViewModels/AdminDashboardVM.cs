using Microsoft.AspNetCore.Mvc;

namespace OnlineAppointment.Areas.Admin.ViewModels
{
    public class AdminDashboardVM
    {
        public int ServiceCount { get; set; }
        public int PendingCount { get; set; }
        public int TodayCount { get; set; }
    }
}
