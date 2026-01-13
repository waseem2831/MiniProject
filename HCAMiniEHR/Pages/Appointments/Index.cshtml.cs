using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Appointments
{
    public class IndexModel : PageModel
    {
        private readonly AppointmentService _appointmentService;

        public IndexModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public List<Appointment> Appointments { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Appointments = await _appointmentService.GetAllAppointmentsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _appointmentService.DeleteAppointmentAsync(id);
            if (success)
            {
                SuccessMessage = "Appointment deleted successfully.";
            }
            return RedirectToPage();
        }
    }
}
