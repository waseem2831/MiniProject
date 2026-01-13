using HCAMiniEHR.Data;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services
{
    public class ReportService
    {
        private readonly EHRDbContext _context;

        public ReportService(EHRDbContext context)
        {
            _context = context;
        }

        // Report 1: Pending Lab Orders
        public async Task<List<object>> GetPendingLabOrdersAsync()
        {
            return await _context.LabOrders
                .Where(l => l.Status == "Pending")
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderBy(l => l.OrderDate)
                .Select(l => new
                {
                    l.LabOrderId,
                    PatientName = l.Appointment.Patient.FirstName + " " + l.Appointment.Patient.LastName,
                    l.TestName,
                    l.OrderDate,
                    DaysWaiting = (DateTime.Now - l.OrderDate).Days,
                    l.Appointment.DoctorName,
                    l.Notes
                })
                .ToListAsync<object>();
        }

        // Report 2: Patients Without Future Appointments
        public async Task<List<object>> GetPatientsWithoutFollowUpAsync()
        {
            var patientsWithFutureAppts = await _context.Appointments
                .Where(a => a.AppointmentDate > DateTime.Now)
                .Select(a => a.PatientId)
                .Distinct()
                .ToListAsync();

            return await _context.Patients
                .Where(p => !patientsWithFutureAppts.Contains(p.PatientId))
                .OrderBy(p => p.LastName)
                .Select(p => new
                {
                    p.PatientId,
                    PatientName = p.FirstName + " " + p.LastName,
                    p.PhoneNumber,
                    p.Email,
                    LastAppointment = _context.Appointments
                        .Where(a => a.PatientId == p.PatientId)
                        .OrderByDescending(a => a.AppointmentDate)
                        .Select(a => a.AppointmentDate)
                        .FirstOrDefault(),
                    DaysSinceLastVisit = _context.Appointments
                        .Where(a => a.PatientId == p.PatientId)
                        .Any()
                        ? (DateTime.Now - _context.Appointments
                            .Where(a => a.PatientId == p.PatientId)
                            .OrderByDescending(a => a.AppointmentDate)
                            .Select(a => a.AppointmentDate)
                            .FirstOrDefault()).Days
                        : (int?)null
                })
                .ToListAsync<object>();
        }

        // Report 3: Doctor Productivity Report
        public async Task<List<object>> GetDoctorProductivityAsync()
        {
            return await _context.Appointments
                .GroupBy(a => a.DoctorName)
                .Select(g => new
                {
                    DoctorName = g.Key,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.Status == "Completed"),
                    ScheduledAppointments = g.Count(a => a.Status == "Scheduled"),
                    CancelledAppointments = g.Count(a => a.Status == "Cancelled"),
                    LabOrdersGenerated = g.SelectMany(a => a.LabOrders).Count(),
                    LastAppointmentDate = g.Max(a => a.AppointmentDate),
                    NextAppointmentDate = g.Where(a => a.AppointmentDate > DateTime.Now)
                        .Min(a => (DateTime?)a.AppointmentDate)
                })
                .OrderByDescending(d => d.TotalAppointments)
                .ToListAsync<object>();
        }



        // Bonus Report: Lab Test Summary
        public async Task<List<object>> GetLabTestSummaryAsync()
        {
            return await _context.LabOrders
                .GroupBy(l => l.TestName)
                .Select(g => new
                {
                    TestName = g.Key,
                    TotalOrders = g.Count(),
                    PendingOrders = g.Count(l => l.Status == "Pending"),
                    CompletedOrders = g.Count(l => l.Status == "Completed"),
                    InProgressOrders = g.Count(l => l.Status == "InProgress"),
                    // Use SQL-translatable DateDiffDay to compute average days between order and completion
                    AverageCompletionDays = g
                        .Where(l => l.CompletedDate.HasValue)
                        .Select(l => (double?)EF.Functions.DateDiffDay(l.OrderDate, l.CompletedDate.Value))
                        .Average()
                })
                .OrderByDescending(t => t.TotalOrders)
                .ToListAsync<object>();
        }
    }
}
