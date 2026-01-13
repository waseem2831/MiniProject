using HCAMiniEHR.Data.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.LabOrders
{
    public class IndexModel : PageModel
    {
        private readonly LabOrderService _labOrderService;

        public IndexModel(LabOrderService labOrderService)
        {
            _labOrderService = labOrderService;
        }

        public List<LabOrder> LabOrders { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            LabOrders = await _labOrderService.GetAllLabOrdersAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var success = await _labOrderService.DeleteLabOrderAsync(id);
                if (success)
                {
                    SuccessMessage = "Lab order deleted successfully.";
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage();
        }
    }
}