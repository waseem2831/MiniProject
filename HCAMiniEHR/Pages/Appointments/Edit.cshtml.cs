using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Appointments
{
    public class EditModel : PageModel
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;

        public EditModel(AppointmentService appointmentService, PatientService patientService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
        }

        [BindProperty]
        public Appointment Appointment { get; set; } = new();

        public List<Patient> Patients { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appt == null)
            {
                return NotFound();
            }

            Appointment = appt;
            Patients = await _patientService.GetAllPatientsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Appointment.PatientId <= 0)
            {
                ModelState.AddModelError("Appointment.PatientId", "Please select a patient.");
            }

            var now = DateTime.Now;
            if (Appointment.AppointmentDate < now)
            {
                ModelState.AddModelError("Appointment.AppointmentDate", "Appointment date cannot be in the past.");
            }

            var threeMonths = now.AddMonths(3);
            if (Appointment.AppointmentDate > threeMonths)
            {
                ModelState.AddModelError("Appointment.AppointmentDate", "Appointment must be within the next 3 months.");
            }

            if (!string.IsNullOrWhiteSpace(Appointment.DoctorName))
            {
                var available = await _appointmentService.IsDoctorAvailableAsync(Appointment.DoctorName, Appointment.AppointmentDate, Appointment.AppointmentId);
                if (!available)
                {
                    ModelState.AddModelError("Appointment.DoctorName", "Selected doctor is not available at that time.");
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                System.Diagnostics.Debug.WriteLine("ModelState errors: " + string.Join(" | ", errors));

                Patients = await _patientService.GetAllPatientsAsync();
                return Page();
            }

            await _appointmentService.UpdateAppointmentAsync(Appointment);
            TempData["SuccessMessage"] = "Appointment updated successfully!";
            return RedirectToPage("./Index");
        }
    }
}
