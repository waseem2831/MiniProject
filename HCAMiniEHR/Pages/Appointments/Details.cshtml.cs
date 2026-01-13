using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Appointments
{
    public class DetailsModel : PageModel
    {
        private readonly AppointmentService _appointmentService;

        public DetailsModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public Appointment Appointment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var appt = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appt == null)
            {
                return NotFound();
            }

            Appointment = appt;
            return Page();
        }
    }
}
