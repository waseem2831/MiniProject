using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.LabOrders
{
    public class EditModel : PageModel
    {
        private readonly LabOrderService _labOrderService;

        public EditModel(LabOrderService labOrderService)
        {
            _labOrderService = labOrderService;
        }

        [BindProperty]
        public LabOrder LabOrder { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var labOrder = await _labOrderService.GetLabOrderByIdAsync(id);
            if (labOrder == null)
            {
                return NotFound();
            }

            LabOrder = labOrder;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _labOrderService.UpdateLabOrderAsync(LabOrder);
            TempData["SuccessMessage"] = "Lab order updated successfully!";
            return RedirectToPage("./Index");
        }
    }
}