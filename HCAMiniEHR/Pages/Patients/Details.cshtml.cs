// ===================================================================
// 2. Pages/Patients/Details.cshtml.cs
// ===================================================================
using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Patients
{
    public class DetailsModel : PageModel
    {
        private readonly PatientService _patientService;
        private readonly AppointmentService _appointmentService;

        public DetailsModel(PatientService patientService, AppointmentService appointmentService)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
        }

        public Patient Patient { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            Patient = patient;
            Appointments = await _appointmentService.GetAppointmentsByPatientAsync(id);
            return Page();
        }
    }
}
