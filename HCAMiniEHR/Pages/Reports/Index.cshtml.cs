using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ReportService _reportService;

        public IndexModel(ReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public string? ReportType { get; set; }

        public List<object> ReportData { get; set; } = new();
        public string ReportTitle { get; set; } = "";

        public async Task OnGetAsync()
        {
            // Support legacy query parameter 'type' in addition to 'ReportType'
            if (string.IsNullOrEmpty(ReportType) && Request.Query.ContainsKey("type"))
            {
                ReportType = Request.Query["type"].ToString();
            }

            var type = ReportType?.ToLower();

            switch (type)
            {
                case "pending":
                    ReportTitle = "Pending Lab Orders Report";
                    ReportData = await _reportService.GetPendingLabOrdersAsync();
                    break;

                case "followup":
                    ReportTitle = "Patients Without Follow-Up Appointments";
                    ReportData = await _reportService.GetPatientsWithoutFollowUpAsync();
                    break;

                case "doctor":
                    ReportTitle = "Doctor Productivity Report";
                    ReportData = await _reportService.GetDoctorProductivityAsync();
                    break;

                case "labsummary":
                    ReportTitle = "Lab Test Summary Report";
                    ReportData = await _reportService.GetLabTestSummaryAsync();
                    break;

                default:
                    ReportTitle = "Select a Report";
                    break;
            }
        }
    }
}
