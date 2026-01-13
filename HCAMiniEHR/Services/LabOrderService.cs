using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services
{
    public class LabOrderService
    {
        private readonly EHRDbContext _context;

        public LabOrderService(EHRDbContext context)
        {
            _context = context;
        }

        public async Task<List<LabOrder>> GetAllLabOrdersAsync()
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderByDescending(l => l.OrderDate)
                .ToListAsync();
        }

        public async Task<List<LabOrder>> GetLabOrdersByAppointmentAsync(int appointmentId)
        {
            return await _context.LabOrders
                .Where(l => l.AppointmentId == appointmentId)
                .OrderBy(l => l.OrderDate)
                .ToListAsync();
        }

        public async Task<LabOrder?> GetLabOrderByIdAsync(int id)
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(l => l.LabOrderId == id);
        }

        public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
        {
            // Prevent duplicate: same appointment and test name (case-insensitive)
            var exists = await _context.LabOrders
                .AnyAsync(l => l.AppointmentId == labOrder.AppointmentId && l.TestName.ToLower() == (labOrder.TestName ?? string.Empty).ToLower());

            if (exists)
            {
                throw new InvalidOperationException("A lab order for the selected appointment and test already exists.");
            }

            labOrder.OrderDate = DateTime.Now;
            _context.LabOrders.Add(labOrder);
            await _context.SaveChangesAsync();
            return labOrder;
        }

        public async Task<LabOrder> UpdateLabOrderAsync(LabOrder labOrder)
        {
            if (labOrder.Status == "Completed" && labOrder.CompletedDate == null)
            {
                labOrder.CompletedDate = DateTime.Now;
            }

            _context.LabOrders.Update(labOrder);
            await _context.SaveChangesAsync();
            return labOrder;
        }

        public async Task<bool> DeleteLabOrderAsync(int id)
        {
            var labOrder = await _context.LabOrders.FindAsync(id);
            if (labOrder == null) return false;

            // Do not allow deleting pending lab orders
            if (labOrder.Status == "Pending")
            {
                throw new InvalidOperationException("Cannot delete a lab order with status 'Pending'.");
            }

            _context.LabOrders.Remove(labOrder);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
