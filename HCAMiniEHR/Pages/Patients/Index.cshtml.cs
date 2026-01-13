using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;

namespace HCAMiniEHR.Pages.Patients
{
    public class IndexModel : PageModel
    {
        private readonly PatientService _patientService;

        public IndexModel(PatientService patientService)
        {
            _patientService = patientService;
        }

        public List<Patient> Patients { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Patients = await _patientService.GetAllPatientsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _patientService.DeletePatientAsync(id);
            if (success)
            {
                SuccessMessage = "Patient deleted successfully.";
            }
            return RedirectToPage();
        }
    }
}
