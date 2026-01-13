using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using System.Threading.Tasks;

namespace HCAMiniEHR.Pages.Patients
{
    public class CreateModel : PageModel
    {
        private readonly PatientService _patientService;

        public CreateModel(PatientService patientService)
        {
            _patientService = patientService;
        }

        [BindProperty]
        public Patient Patient { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _patientService.CreatePatientAsync(Patient);
            TempData["SuccessMessage"] = "Patient created successfully!";
            return RedirectToPage("./Index");
        }
    }
}
