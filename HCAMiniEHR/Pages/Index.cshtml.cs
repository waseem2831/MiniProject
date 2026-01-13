using Microsoft.AspNetCore.Mvc.RazorPages;
using HCAMiniEHR.Services;

namespace HCAMiniEHR.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PatientService _patientService;
        private readonly AppointmentService _appointmentService;
        private readonly LabOrderService _labOrderService;

        public IndexModel(PatientService patientService, AppointmentService appointmentService,
            LabOrderService labOrderService)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _labOrderService = labOrderService;
        }

        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingLabOrders { get; set; }
        public int UpcomingAppointments { get; set; }

        public async Task OnGetAsync()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var labOrders = await _labOrderService.GetAllLabOrdersAsync();

            TotalPatients = patients.Count;
            TotalAppointments = appointments.Count;
            PendingLabOrders = labOrders.Count(l => l.Status == "Pending");
            UpcomingAppointments = appointments.Count(a => a.AppointmentDate > DateTime.Now && a.Status == "Scheduled");
        }
    }
}
