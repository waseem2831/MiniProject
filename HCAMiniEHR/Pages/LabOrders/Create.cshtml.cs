using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.LabOrders
{
    public class CreateModel : PageModel
    {
        private readonly LabOrderService _labOrderService;
        private readonly AppointmentService _appointmentService;

        public CreateModel(LabOrderService labOrderService, AppointmentService appointmentService)
        {
            _labOrderService = labOrderService;
            _appointmentService = appointmentService;
        }

        [BindProperty]
        public LabOrder LabOrder { get; set; } = new();

        public List<Appointment> Appointments { get; set; } = new();

        public async Task OnGetAsync(int? appointmentId)
        {
            Appointments = await _appointmentService.GetAllAppointmentsAsync();

            if (appointmentId.HasValue)
            {
                LabOrder.AppointmentId = appointmentId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Page();
            }

            try
            {
                await _labOrderService.CreateLabOrderAsync(LabOrder);
                TempData["SuccessMessage"] = "Lab order created successfully!";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Page();
            }
        }
    }
}
