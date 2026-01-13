using Microsoft.EntityFrameworkCore;
using HCAMiniEHR.Data;
using HCAMiniEHR.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EHRConnection")));

// Register services
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<LabOrderService>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

// Ensure database is created and seed sample data when running in Development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<EHRDbContext>();
        // Create database if it does not exist
        db.Database.EnsureCreated();

        // If no patients exist, seed sample data so reports and scheduling have data
        if (!db.Patients.Any())
        {
            // Seed patients
            var patients = new[]
            {
                new HCAMiniEHR.Data.Models.Patient { FirstName = "John", LastName = "Smith", DateOfBirth = new DateTime(1985,5,15), Gender = "Male", PhoneNumber = "555-0101", Email = "john.smith@email.com", Address = "123 Main St", CreatedDate = DateTime.Now.AddMonths(-6) },
                new HCAMiniEHR.Data.Models.Patient { FirstName = "Mary", LastName = "Johnson", DateOfBirth = new DateTime(1990,8,22), Gender = "Female", PhoneNumber = "555-0102", Email = "mary.j@email.com", Address = "456 Oak Ave", CreatedDate = DateTime.Now.AddMonths(-5) },
                new HCAMiniEHR.Data.Models.Patient { FirstName = "Robert", LastName = "Williams", DateOfBirth = new DateTime(1978,3,10), Gender = "Male", PhoneNumber = "555-0103", Email = "r.williams@email.com", Address = "789 Pine Rd", CreatedDate = DateTime.Now.AddMonths(-4) }
            };
            db.Patients.AddRange(patients);
            db.SaveChanges();

            // Seed some appointments and lab orders
            var appts = new[]
            {
                new HCAMiniEHR.Data.Models.Appointment { PatientId = db.Patients.OrderBy(p => p.PatientId).First().PatientId, AppointmentDate = DateTime.Now.AddDays(-10), AppointmentType = "Checkup", DoctorName = "Dr. Sarah Connor", Status = "Completed", Notes = "Annual physical examination", CreatedDate = DateTime.Now.AddDays(-15) },
                new HCAMiniEHR.Data.Models.Appointment { PatientId = db.Patients.Skip(1).First().PatientId, AppointmentDate = DateTime.Now.AddDays(3), AppointmentType = "Follow-up", DoctorName = "Dr. John McClane", Status = "Scheduled", Notes = "Blood pressure follow-up", CreatedDate = DateTime.Now.AddDays(-2) }
            };
            db.Appointments.AddRange(appts);
            db.SaveChanges();

            var labOrders = new[]
            {
                new HCAMiniEHR.Data.Models.LabOrder { AppointmentId = db.Appointments.OrderBy(a => a.AppointmentId).First().AppointmentId, TestName = "Complete Blood Count (CBC)", Status = "Completed", OrderDate = DateTime.Now.AddDays(-10), CompletedDate = DateTime.Now.AddDays(-8), Results = "All values within normal range" },
                new HCAMiniEHR.Data.Models.LabOrder { AppointmentId = db.Appointments.Skip(0).First().AppointmentId, TestName = "Lipid Panel", Status = "Pending", OrderDate = DateTime.Now.AddDays(-1), Notes = "Urgent" }
            };
            db.LabOrders.AddRange(labOrders);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating or seeding the DB.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();