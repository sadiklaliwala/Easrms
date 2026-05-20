namespace Easrms.Application.DTOs.Dashboard
{
    public class SLADashboardDto
    {
        public int TotalOpen { get; set; }
        public int WithinSLACount { get; set; }
        public int NearingBreachCount { get; set; }
        public int BreachedCount { get; set; }
        public int EscalatedCount { get; set; }
    }
}