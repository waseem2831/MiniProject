using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Appointments
{
    public class CreateModel : PageModel
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;

        public CreateModel(AppointmentService appointmentService, PatientService patientService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
        }

        [BindProperty]
        public Appointment Appointment { get; set; } = new();

        public List<Patient> Patients { get; set; } = new();

        public async Task OnGetAsync(int? patientId)
        {
            Patients = await _patientService.GetAllPatientsAsync();

            if (patientId.HasValue)
            {
                Appointment.PatientId = patientId.Value;
            }

            // set a sensible default appointment date if none provided
            if (Appointment.AppointmentDate == default)
            {
                Appointment.AppointmentDate = DateTime.Now.AddDays(1).AddMinutes(-DateTime.Now.Minute).AddSeconds(-DateTime.Now.Second);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Explicit server-side validation for Patient selection
            if (Appointment.PatientId <= 0)
            {
                ModelState.AddModelError("Appointment.PatientId", "Please select a patient.");
            }

            // Date validation: not in the past
            var now = DateTime.Now;
            if (Appointment.AppointmentDate < now)
            {
                ModelState.AddModelError("Appointment.AppointmentDate", "Appointment date cannot be in the past.");
            }

            // Date validation: within 3 months from today
            var threeMonths = now.AddMonths(3);
            if (Appointment.AppointmentDate > threeMonths)
            {
                ModelState.AddModelError("Appointment.AppointmentDate", "Appointment must be within the next 3 months.");
            }

            // Doctor availability check
            if (!string.IsNullOrWhiteSpace(Appointment.DoctorName))
            {
                var available = await _appointmentService.IsDoctorAvailableAsync(Appointment.DoctorName, Appointment.AppointmentDate);
                if (!available)
                {
                    ModelState.AddModelError("Appointment.DoctorName", "Selected doctor is not available at that time.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Log ModelState errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                System.Diagnostics.Debug.WriteLine("ModelState errors: " + string.Join(" | ", errors));

                Patients = await _patientService.GetAllPatientsAsync();
                return Page();
            }

            await _appointmentService.CreateAppointmentAsync(Appointment);
            TempData["SuccessMessage"] = "Appointment scheduled successfully!";
            return RedirectToPage("./Index");
        }
    }
}