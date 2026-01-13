using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HCAMiniEHR.Services
{
    public class AppointmentService
    {
        private readonly EHRDbContext _context;

        public AppointmentService(EHRDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.LabOrders)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<bool> IsDoctorAvailableAsync(string doctorName, DateTime appointmentDate, int? excludeAppointmentId = null)
        {
            if (string.IsNullOrWhiteSpace(doctorName)) return false;

            var query = _context.Appointments.AsQueryable()
                .Where(a => a.DoctorName == doctorName && a.AppointmentDate == appointmentDate);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            appointment.CreatedDate = DateTime.Now;

            // Use raw ADO to insert and retrieve identity using SCOPE_IDENTITY() to avoid SQL Server OUTPUT issue when triggers exist
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Healthcare.Appointment (PatientId, AppointmentDate, AppointmentType, DoctorName, Status, Notes, CreatedDate)
VALUES (@PatientId, @AppointmentDate, @AppointmentType, @DoctorName, @Status, @Notes, @CreatedDate);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var p1 = command.CreateParameter(); p1.ParameterName = "@PatientId"; p1.Value = appointment.PatientId; p1.DbType = DbType.Int32; command.Parameters.Add(p1);
                var p2 = command.CreateParameter(); p2.ParameterName = "@AppointmentDate"; p2.Value = appointment.AppointmentDate; p2.DbType = DbType.DateTime2; command.Parameters.Add(p2);
                var p3 = command.CreateParameter(); p3.ParameterName = "@AppointmentType"; p3.Value = appointment.AppointmentType ?? (object)DBNull.Value; p3.DbType = DbType.String; command.Parameters.Add(p3);
                var p4 = command.CreateParameter(); p4.ParameterName = "@DoctorName"; p4.Value = appointment.DoctorName ?? (object)DBNull.Value; p4.DbType = DbType.String; command.Parameters.Add(p4);
                var p5 = command.CreateParameter(); p5.ParameterName = "@Status"; p5.Value = appointment.Status ?? (object)DBNull.Value; p5.DbType = DbType.String; command.Parameters.Add(p5);
                var p6 = command.CreateParameter(); p6.ParameterName = "@Notes"; p6.Value = appointment.Notes ?? (object)DBNull.Value; p6.DbType = DbType.String; command.Parameters.Add(p6);
                var p7 = command.CreateParameter(); p7.ParameterName = "@CreatedDate"; p7.Value = appointment.CreatedDate; p7.DbType = DbType.DateTime2; command.Parameters.Add(p7);

                var result = await command.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out var newId))
                {
                    appointment.AppointmentId = newId;
                }

                return appointment;
            }
            finally
            {
                // don't dispose the connection (context owns it), just close if we opened it
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        // Create appointment using stored procedure
        public async Task<int> CreateAppointmentViaSPAsync(int patientId, DateTime appointmentDate,
            string appointmentType, string doctorName, string? notes)
        {
            var patientIdParam = new Microsoft.Data.SqlClient.SqlParameter("@PatientId", patientId);
            var dateParam = new Microsoft.Data.SqlClient.SqlParameter("@AppointmentDate", appointmentDate);
            var typeParam = new Microsoft.Data.SqlClient.SqlParameter("@AppointmentType", appointmentType);
            var doctorParam = new Microsoft.Data.SqlClient.SqlParameter("@DoctorName", doctorName);
            var notesParam = new Microsoft.Data.SqlClient.SqlParameter("@Notes", notes ?? (object)DBNull.Value);
            var appointmentIdParam = new Microsoft.Data.SqlClient.SqlParameter("@AppointmentId", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC Healthcare.CreateAppointment @PatientId, @AppointmentDate, @AppointmentType, @DoctorName, @Notes, @AppointmentId OUTPUT",
                patientIdParam, dateParam, typeParam, doctorParam, notesParam, appointmentIdParam);

            return (int)appointmentIdParam.Value!;
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            // Use raw ADO to update to avoid EF Core OUTPUT clause when triggers exist on the table
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Healthcare.Appointment
SET PatientId = @PatientId,
    AppointmentDate = @AppointmentDate,
    AppointmentType = @AppointmentType,
    DoctorName = @DoctorName,
    Status = @Status,
    Notes = @Notes
WHERE AppointmentId = @AppointmentId;";

                var pId = command.CreateParameter(); pId.ParameterName = "@PatientId"; pId.Value = appointment.PatientId; pId.DbType = DbType.Int32; command.Parameters.Add(pId);
                var pDate = command.CreateParameter(); pDate.ParameterName = "@AppointmentDate"; pDate.Value = appointment.AppointmentDate; pDate.DbType = DbType.DateTime2; command.Parameters.Add(pDate);
                var pType = command.CreateParameter(); pType.ParameterName = "@AppointmentType"; pType.Value = appointment.AppointmentType ?? (object)DBNull.Value; pType.DbType = DbType.String; command.Parameters.Add(pType);
                var pDoctor = command.CreateParameter(); pDoctor.ParameterName = "@DoctorName"; pDoctor.Value = appointment.DoctorName ?? (object)DBNull.Value; pDoctor.DbType = DbType.String; command.Parameters.Add(pDoctor);
                var pStatus = command.CreateParameter(); pStatus.ParameterName = "@Status"; pStatus.Value = appointment.Status ?? (object)DBNull.Value; pStatus.DbType = DbType.String; command.Parameters.Add(pStatus);
                var pNotes = command.CreateParameter(); pNotes.ParameterName = "@Notes"; pNotes.Value = appointment.Notes ?? (object)DBNull.Value; pNotes.DbType = DbType.String; command.Parameters.Add(pNotes);
                var pApptId = command.CreateParameter(); pApptId.ParameterName = "@AppointmentId"; pApptId.Value = appointment.AppointmentId; pApptId.DbType = DbType.Int32; command.Parameters.Add(pApptId);

                await command.ExecuteNonQueryAsync();

                // reload the appointment including navigation properties
                var updated = await GetAppointmentByIdAsync(appointment.AppointmentId);
                return updated ?? appointment;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            // Use raw ADO delete to avoid OUTPUT clause problem when triggers exist
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Healthcare.Appointment WHERE AppointmentId = @AppointmentId";
                var pid = command.CreateParameter(); pid.ParameterName = "@AppointmentId"; pid.Value = id; pid.DbType = DbType.Int32; command.Parameters.Add(pid);

                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}